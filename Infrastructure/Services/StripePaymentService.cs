using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.DTOs.Payment;
using Application.Abstractions.Persistence;
using Common.Constants;
using Common.Enums;
using Domain.ConfigModels;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using PaymentMethodEnum = Common.Enums.PaymentMethod;

namespace Infrastructure.Services;

public class StripePaymentService : IStripePaymentService
{
    private const string CheckoutSessionCompletedEventType = "checkout.session.completed";
    private const string CheckoutSessionExpiredEventType = "checkout.session.expired";
    private const string PaymentIntentPaymentFailedEventType = "payment_intent.payment_failed";
    private const string PaidStripePaymentStatus = "paid";
    private const string OpenStripeSessionStatus = "open";
    private const string ExpiredStripeSessionStatus = "expired";
    private const string OrderIdUrlPlaceholder = "{orderId}";

    private readonly StripeSettings stripeSettings;
    private readonly IPaymentAttemptRepository paymentAttemptRepository;
    private readonly IProcessedPaymentEventRepository processedPaymentEventRepository;
    private readonly IApplicationTransaction applicationTransaction;
    private readonly ILogger<StripePaymentService> logger;

    public StripePaymentService(
        IOptions<StripeSettings> options,
        IPaymentAttemptRepository paymentAttemptRepository,
        IProcessedPaymentEventRepository processedPaymentEventRepository,
        IApplicationTransaction applicationTransaction,
        ILogger<StripePaymentService> logger)
    {
        stripeSettings = options.Value;
        this.paymentAttemptRepository = paymentAttemptRepository;
        this.processedPaymentEventRepository = processedPaymentEventRepository;
        this.applicationTransaction = applicationTransaction;
        this.logger = logger;
    }

    private async Task<CreateStripeCheckoutSessionResult> CreateCheckoutSessionAsync(
        CreateStripeCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            CustomerEmail = request.CustomerEmail,

            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,

            ClientReferenceId = request.OrderId.ToString(),

            Metadata = new Dictionary<string, string>
            {
                ["orderId"] = request.OrderId.ToString(),
                ["paymentAttemptId"] = request.PaymentAttemptId.ToString()
            },

            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = request.OrderId.ToString(),
                    ["paymentAttemptId"] = request.PaymentAttemptId.ToString()
                }
            },

            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = stripeSettings.Currency,
                        UnitAmount = ToStripeMinorAmount(request.Amount),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.Description
                        }
                    }
                }
            }
        };

        var service = new SessionService();

        var session = await service.CreateAsync(
            options,
            new RequestOptions
            {
                ApiKey = stripeSettings.SecretKey
            },
            cancellationToken);

        if (string.IsNullOrWhiteSpace(session.Url))
        {
            throw new InvalidOperationException("Stripe did not return a Checkout URL.");
        }

        return new CreateStripeCheckoutSessionResult
        {
            SessionId = session.Id,
            PaymentUrl = session.Url,
            ExpiresAt = session.ExpiresAt
        };
    }

    public async Task HandleWebhookAsync(
        string json,
        string stripeSignature,
        CancellationToken cancellationToken)
    {
        Event stripeEvent = EventUtility.ConstructEvent(
            json,
            stripeSignature,
            stripeSettings.WebhookSecret);

        if (string.IsNullOrWhiteSpace(stripeEvent.Id))
        {
            return;
        }

        await applicationTransaction.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var alreadyProcessed = await processedPaymentEventRepository.ExistsAsync(
                    PaymentProviderConstants.STRIPE,
                    stripeEvent.Id,
                    transactionCancellationToken);

                if (alreadyProcessed)
                {
                    return false;
                }

                var processingResult = await ProcessStripeEventAsync(
                    stripeEvent,
                    transactionCancellationToken);

                if (!processingResult.ShouldTrack)
                {
                    return false;
                }

                await processedPaymentEventRepository.AddAsync(
                    new ProcessedPaymentEvent
                    {
                        Provider = PaymentProviderConstants.STRIPE,
                        ProviderEventId = stripeEvent.Id,
                        EventType = stripeEvent.Type,
                        PaymentAttemptId = processingResult.PaymentAttemptId,
                        OrderId = processingResult.OrderId,
                        ProcessedAt = DateTime.UtcNow,
                        RawStatus = processingResult.RawStatus
                    },
                    transactionCancellationToken);

                await paymentAttemptRepository.SaveChangesAsync(transactionCancellationToken);

                return true;
            },
            cancellationToken);
    }

    private async Task<PaymentEventProcessingResult> ProcessStripeEventAsync(
        Event stripeEvent,
        CancellationToken cancellationToken)
    {
        if (stripeEvent.Type == CheckoutSessionCompletedEventType
            && stripeEvent.Data.Object is Session completedSession)
        {
            return await HandleCheckoutSessionCompletedAsync(completedSession, cancellationToken);
        }

        if (stripeEvent.Type == CheckoutSessionExpiredEventType
            && stripeEvent.Data.Object is Session expiredSession)
        {
            return await HandleCheckoutSessionExpiredAsync(expiredSession, cancellationToken);
        }

        if (stripeEvent.Type == PaymentIntentPaymentFailedEventType
            && stripeEvent.Data.Object is PaymentIntent paymentIntent)
        {
            return await HandlePaymentIntentPaymentFailedAsync(paymentIntent, cancellationToken);
        }

        return PaymentEventProcessingResult.NotTracked();
    }

    private async Task<PaymentEventProcessingResult> HandleCheckoutSessionCompletedAsync(
        Session session,
        CancellationToken cancellationToken)
    {
        if (session.Metadata is null
            || !session.Metadata.TryGetValue("paymentAttemptId", out var paymentAttemptIdRaw))
        {
            return PaymentEventProcessingResult.NotTracked();
        }

        if (!Guid.TryParse(paymentAttemptIdRaw, out var paymentAttemptId))
        {
            return PaymentEventProcessingResult.NotTracked();
        }

        var paymentAttempt = await paymentAttemptRepository.GetPaymentAttemptWithOrderAsync(
            paymentAttemptId,
            cancellationToken);

        if (paymentAttempt is null)
        {
            return PaymentEventProcessingResult.NotTracked();
        }

        var order = paymentAttempt.Order;
        var rawStatus = BuildRawStatus(session);

        if (!IsValidCheckoutSessionForAttempt(session, paymentAttempt, stripeSettings.Currency))
        {
            return PaymentEventProcessingResult.NotTracked();
        }

        if (order.IsPaid)
        {
            return PaymentEventProcessingResult.Tracked(paymentAttempt.Id, order.Id, rawStatus);
        }

        var paidAt = paymentAttempt.PaidAt ?? DateTime.UtcNow;
        MarkPaymentAttemptPaid(paymentAttempt, session, paidAt);

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Delivered)
        {
            return PaymentEventProcessingResult.Tracked(paymentAttempt.Id, order.Id, rawStatus);
        }

        MarkOrderPaid(order, paidAt);

        return PaymentEventProcessingResult.Tracked(paymentAttempt.Id, order.Id, rawStatus);
    }

    private async Task<PaymentEventProcessingResult> HandleCheckoutSessionExpiredAsync(
        Session session,
        CancellationToken cancellationToken)
    {
        var paymentAttempt = await GetPaymentAttemptForCheckoutSessionAsync(session, cancellationToken);
        if (paymentAttempt is null)
        {
            return PaymentEventProcessingResult.NotTracked();
        }

        var order = paymentAttempt.Order;
        var rawStatus = BuildRawStatus(session);

        if (!IsCheckoutSessionForAttempt(session, paymentAttempt))
        {
            return PaymentEventProcessingResult.NotTracked();
        }

        if (paymentAttempt.Status == PaymentStatus.Paid || order.IsPaid)
        {
            return PaymentEventProcessingResult.Tracked(paymentAttempt.Id, order.Id, rawStatus);
        }

        var expiredAt = DateTime.UtcNow;
        MarkPaymentAttemptExpired(paymentAttempt);
        MarkOrderPaymentUnsuccessful(order, paymentAttempt, PaymentStatus.Expired, expiredAt);

        return PaymentEventProcessingResult.Tracked(paymentAttempt.Id, order.Id, rawStatus);
    }

    private async Task<PaymentEventProcessingResult> HandlePaymentIntentPaymentFailedAsync(
        PaymentIntent paymentIntent,
        CancellationToken cancellationToken)
    {
        var paymentAttempt = await GetPaymentAttemptForPaymentIntentAsync(paymentIntent, cancellationToken);
        if (paymentAttempt is null)
        {
            return PaymentEventProcessingResult.NotTracked();
        }

        var order = paymentAttempt.Order;
        var rawStatus = BuildRawStatus(paymentIntent);

        if (!IsPaymentIntentForAttempt(paymentIntent, paymentAttempt))
        {
            return PaymentEventProcessingResult.NotTracked();
        }

        if (paymentAttempt.Status == PaymentStatus.Paid || order.IsPaid)
        {
            return PaymentEventProcessingResult.Tracked(paymentAttempt.Id, order.Id, rawStatus);
        }

        var failedAt = DateTime.UtcNow;
        MarkPaymentAttemptFailed(paymentAttempt, paymentIntent);
        MarkOrderPaymentUnsuccessful(order, paymentAttempt, PaymentStatus.Failed, failedAt);

        return PaymentEventProcessingResult.Tracked(paymentAttempt.Id, order.Id, rawStatus);
    }

    private static long ToStripeMinorAmount(decimal amount)
    {
        return (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero);
    }

    private static bool IsValidCheckoutSessionForAttempt(
        Session session,
        PaymentAttempt paymentAttempt,
        string configuredCurrency)
    {
        if (string.IsNullOrWhiteSpace(paymentAttempt.StripeCheckoutSessionId)
            || !string.Equals(
                session.Id,
                paymentAttempt.StripeCheckoutSessionId,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (!TryGetMetadataGuid(session, "paymentAttemptId", out var metadataPaymentAttemptId)
            || metadataPaymentAttemptId != paymentAttempt.Id)
        {
            return false;
        }

        if (!TryGetMetadataGuid(session, "orderId", out var metadataOrderId)
            || metadataOrderId != paymentAttempt.OrderId)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(session.ClientReferenceId)
            && (!Guid.TryParse(session.ClientReferenceId, out var clientReferenceOrderId)
                || clientReferenceOrderId != paymentAttempt.OrderId))
        {
            return false;
        }

        if (paymentAttempt.Order is null || paymentAttempt.Order.Id != paymentAttempt.OrderId)
        {
            return false;
        }

        var expectedAttemptAmount = ToStripeMinorAmount(paymentAttempt.Amount);
        var expectedOrderAmount = ToStripeMinorAmount(paymentAttempt.Order.TotalAmount);
        var amountMatches = session.AmountTotal == expectedAttemptAmount
            && session.AmountTotal == expectedOrderAmount;
        var currencyMatches = string.Equals(
                session.Currency,
                paymentAttempt.Currency,
                StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                session.Currency,
                configuredCurrency,
                StringComparison.OrdinalIgnoreCase);

        if (session.PaymentStatus != PaidStripePaymentStatus
            || !amountMatches
            || !currencyMatches)
        {
            return false;
        }

        return true;
    }

    private async Task<PaymentAttempt?> GetPaymentAttemptForCheckoutSessionAsync(
        Session session,
        CancellationToken cancellationToken)
    {
        if (TryGetMetadataGuid(session, "paymentAttemptId", out var paymentAttemptId))
        {
            var paymentAttempt = await paymentAttemptRepository.GetPaymentAttemptWithOrderAsync(
                paymentAttemptId,
                cancellationToken);
            if (paymentAttempt is not null)
            {
                return paymentAttempt;
            }
        }

        return string.IsNullOrWhiteSpace(session.Id)
            ? null
            : await paymentAttemptRepository.GetPaymentAttemptByStripeCheckoutSessionIdWithOrderAsync(
                session.Id,
                cancellationToken);
    }

    private async Task<PaymentAttempt?> GetPaymentAttemptForPaymentIntentAsync(
        PaymentIntent paymentIntent,
        CancellationToken cancellationToken)
    {
        if (TryGetMetadataGuid(paymentIntent.Metadata, "paymentAttemptId", out var paymentAttemptId))
        {
            var paymentAttempt = await paymentAttemptRepository.GetPaymentAttemptWithOrderAsync(
                paymentAttemptId,
                cancellationToken);
            if (paymentAttempt is not null)
            {
                return paymentAttempt;
            }
        }

        return string.IsNullOrWhiteSpace(paymentIntent.Id)
            ? null
            : await paymentAttemptRepository.GetPaymentAttemptByStripePaymentIntentIdWithOrderAsync(
                paymentIntent.Id,
                cancellationToken);
    }

    private static bool IsCheckoutSessionForAttempt(
        Session session,
        PaymentAttempt paymentAttempt)
    {
        if (!string.IsNullOrWhiteSpace(paymentAttempt.StripeCheckoutSessionId)
            && !string.Equals(
                session.Id,
                paymentAttempt.StripeCheckoutSessionId,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (TryGetMetadataGuid(session, "orderId", out var metadataOrderId)
            && metadataOrderId != paymentAttempt.OrderId)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(session.ClientReferenceId)
            && (!Guid.TryParse(session.ClientReferenceId, out var clientReferenceOrderId)
                || clientReferenceOrderId != paymentAttempt.OrderId))
        {
            return false;
        }

        return true;
    }

    private static bool IsPaymentIntentForAttempt(
        PaymentIntent paymentIntent,
        PaymentAttempt paymentAttempt)
    {
        if (!string.IsNullOrWhiteSpace(paymentAttempt.StripePaymentIntentId)
            && !string.Equals(
                paymentIntent.Id,
                paymentAttempt.StripePaymentIntentId,
                StringComparison.Ordinal))
        {
            return false;
        }

        return !TryGetMetadataGuid(paymentIntent.Metadata, "orderId", out var metadataOrderId)
            || metadataOrderId == paymentAttempt.OrderId;
    }

    private static bool TryGetMetadataGuid(
        Session session,
        string key,
        out Guid value)
    {
        return TryGetMetadataGuid(session.Metadata, key, out value);
    }

    private static bool TryGetMetadataGuid(
        IReadOnlyDictionary<string, string>? metadata,
        string key,
        out Guid value)
    {
        value = Guid.Empty;

        return metadata is not null
            && metadata.TryGetValue(key, out var rawValue)
            && Guid.TryParse(rawValue, out value);
    }

    private static bool HasNewerActiveAttempt(
        Order order,
        PaymentAttempt paymentAttempt)
    {
        return order.PaymentAttempts.Any(orderPaymentAttempt =>
            orderPaymentAttempt.Id != paymentAttempt.Id
            && orderPaymentAttempt.CreatedAt > paymentAttempt.CreatedAt);
    }

    private static string? BuildRawStatus(Session session)
    {
        if (!string.IsNullOrWhiteSpace(session.PaymentStatus))
        {
            return session.PaymentStatus;
        }

        return string.IsNullOrWhiteSpace(session.Status)
            ? null
            : session.Status;
    }

    private static string? BuildRawStatus(PaymentIntent paymentIntent)
    {
        return string.IsNullOrWhiteSpace(paymentIntent.Status)
            ? null
            : paymentIntent.Status;
    }

    private async Task<Session> GetCheckoutSessionAsync(string checkoutSessionId, CancellationToken cancellationToken)
    {
        var service = new SessionService();

        return await service.GetAsync(
            checkoutSessionId,
            requestOptions: new RequestOptions
            {
                ApiKey = stripeSettings.SecretKey
            },
            cancellationToken: cancellationToken);
    }

    public async Task<PaymentUrlStatusResult> PaymentUrlStatusAsync(Order order, CancellationToken cancellationToken)
    {
        if (order is null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        return await applicationTransaction.ExecuteAsync(
            async transactionCancellationToken =>
            {
                var retryOrder = await paymentAttemptRepository.GetOrderForPaymentRetryUpdateAsync(
                    order.Id,
                    transactionCancellationToken);

                if (retryOrder is null)
                {
                    throw new KeyNotFoundException("Order not found.");
                }

                var result = await PaymentUrlStatusCoreAsync(retryOrder, transactionCancellationToken);
                CopyPaymentState(retryOrder, order);

                return result;
            },
            cancellationToken);
    }

    private async Task<PaymentUrlStatusResult> PaymentUrlStatusCoreAsync(Order order, CancellationToken cancellationToken)
    {
        if (order.IsPaid)
        {
            return new PaymentUrlStatusResult
            {
                IsPaid = true,
                PaymentUrl = null,
                Message = "Order is already paid."
            };
        }

        if (order.PaymentMethod == PaymentMethodEnum.CashOnDelivery)
        {
            return new PaymentUrlStatusResult
            {
                IsPaid = false,
                PaymentUrl = null,
                Message = "Online payment retry is not available for cash on delivery orders."
            };
        }

        if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
        {
            return new PaymentUrlStatusResult
            {
                IsPaid = false,
                PaymentUrl = null,
                Message = "Online payment retry is not available for this order."
            };
        }

        foreach (var attempt in order.PaymentAttempts.OrderByDescending(x => x.CreatedAt))
        {
            if (string.IsNullOrWhiteSpace(attempt.StripeCheckoutSessionId))
            {
                continue;
            }

            if (attempt.Status is PaymentStatus.Failed or PaymentStatus.Expired)
            {
                MarkOrderPaymentUnsuccessful(order, attempt, attempt.Status, DateTime.UtcNow);
                continue;
            }

            var session = await GetCheckoutSessionAsync(
                attempt.StripeCheckoutSessionId,
                cancellationToken);

            var expectedAmount = ToStripeMinorAmount(attempt.Amount);

            var amountMatches = session.AmountTotal == expectedAmount;
            var currencyMatches = string.Equals(
                session.Currency,
                attempt.Currency,
                StringComparison.OrdinalIgnoreCase);

            if (session.PaymentStatus == PaidStripePaymentStatus && amountMatches && currencyMatches)
            {
                var paidAt = DateTime.UtcNow;
                MarkPaymentAttemptPaid(attempt, session, paidAt);
                MarkOrderPaid(order, paidAt);

                await paymentAttemptRepository.SaveChangesAsync(cancellationToken);

                return new PaymentUrlStatusResult
                {
                    IsPaid = true,
                    PaymentUrl = null,
                    Message = "Order payment was confirmed from Stripe."
                };
            }

            if (session.Status == OpenStripeSessionStatus)
            {
                attempt.Status = PaymentStatus.PendingOnlinePayment;
                order.IsPaid = false;
                order.PaymentMethod = PaymentMethodEnum.OnlineCard;
                order.PaymentStatus = PaymentStatus.PendingOnlinePayment;
                order.UpdatedAt = DateTime.UtcNow;

                await paymentAttemptRepository.SaveChangesAsync(cancellationToken);

                return new PaymentUrlStatusResult
                {
                    IsPaid = false,
                    PaymentUrl = attempt.StripeCheckoutUrl ?? session.Url,
                    Message = "Existing payment link is still active."
                };
            }

            if (session.Status == ExpiredStripeSessionStatus)
            {
                MarkPaymentAttemptExpired(attempt);
                MarkOrderPaymentUnsuccessful(order, attempt, PaymentStatus.Expired, DateTime.UtcNow);
            }
        }

        var paymentAttemptResult = await CreateNewPaymentAttemptForOrderAsync(
            order,
            cancellationToken);

        await paymentAttemptRepository.SaveChangesAsync(cancellationToken);

        return new PaymentUrlStatusResult
        {
            IsPaid = false,
            PaymentUrl = paymentAttemptResult.PaymentUrl,
            Message = "New payment link created."
        };
    }

    private static void CopyPaymentState(Order source, Order target)
    {
        target.Status = source.Status;
        target.IsPaid = source.IsPaid;
        target.TotalAmount = source.TotalAmount;
        target.PaymentMethod = source.PaymentMethod;
        target.PaymentStatus = source.PaymentStatus;
        target.PaidAt = source.PaidAt;
        target.UpdatedAt = source.UpdatedAt;
        target.PaymentAttempts = source.PaymentAttempts.ToList();
    }

    public async Task<PaymentAttemptCreationResult> CreateNewPaymentAttemptForOrderAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        var paymentAttempt = new PaymentAttempt
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            Amount = order.TotalAmount,
            Currency = stripeSettings.Currency,
            Status = PaymentStatus.PendingOnlinePayment,
            CreatedAt = DateTime.UtcNow
        };
        await paymentAttemptRepository.AddPaymentAttemptAsync(paymentAttempt, cancellationToken);

        var stripeSession = await CreateCheckoutSessionAsync(
            new CreateStripeCheckoutSessionRequest
            {
                OrderId = order.Id,
                PaymentAttemptId = paymentAttempt.Id,
                CustomerEmail = order.GuestCustomer?.Email
                    ?? order.User?.Email
                    ?? "guest@example.com",
                Amount = order.TotalAmount,
                Description = $"Restaurant order #{order.Id}",

                SuccessUrl = CreateConfiguredRedirectUrl(stripeSettings.SuccessUrl, order.Id, nameof(stripeSettings.SuccessUrl)),
                CancelUrl = CreateConfiguredRedirectUrl(stripeSettings.CancelUrl, order.Id, nameof(stripeSettings.CancelUrl))
            },
            cancellationToken);

        paymentAttempt.StripeCheckoutSessionId = stripeSession.SessionId;
        paymentAttempt.StripeCheckoutUrl = stripeSession.PaymentUrl;
        paymentAttempt.ExpiresAt = stripeSession.ExpiresAt;

        order.IsPaid = false;
        order.PaymentMethod = PaymentMethodEnum.OnlineCard;
        order.PaymentStatus = PaymentStatus.PendingOnlinePayment;
        order.UpdatedAt = DateTime.UtcNow;
        order.PaymentAttempts.Add(paymentAttempt);
        await paymentAttemptRepository.SaveChangesAsync(cancellationToken);

        return new PaymentAttemptCreationResult(
            stripeSession.PaymentUrl,
            stripeSession.SessionId);
    }

    public async Task ExpireCheckoutSessionAsync(
        string checkoutSessionId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(checkoutSessionId))
        {
            return;
        }

        var service = new SessionService();

        try
        {
            var session = await service.GetAsync(
                checkoutSessionId,
                requestOptions: new RequestOptions
                {
                    ApiKey = stripeSettings.SecretKey
                },
                cancellationToken: cancellationToken);

            if (!string.Equals(session.Status, OpenStripeSessionStatus, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation(
                    "Stripe checkout session was not open and was not expired. CheckoutSessionId={CheckoutSessionId}, Status={Status}",
                    checkoutSessionId,
                    session.Status);
                return;
            }

            await service.ExpireAsync(
                checkoutSessionId,
                options: null,
                requestOptions: new RequestOptions
                {
                    ApiKey = stripeSettings.SecretKey
                },
                cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(
                exception,
                "Failed to expire Stripe checkout session. CheckoutSessionId={CheckoutSessionId}",
                checkoutSessionId);
        }
    }

    private static void MarkPaymentAttemptPaid(
        PaymentAttempt paymentAttempt,
        Session session,
        DateTime paidAt)
    {
        paymentAttempt.Status = PaymentStatus.Paid;
        paymentAttempt.StripePaymentIntentId = session.PaymentIntentId;
        paymentAttempt.PaidAt = paidAt;
    }

    private static void MarkPaymentAttemptExpired(PaymentAttempt paymentAttempt)
    {
        if (paymentAttempt.Status != PaymentStatus.Paid)
        {
            paymentAttempt.Status = PaymentStatus.Expired;
        }
    }

    private static void MarkPaymentAttemptFailed(
        PaymentAttempt paymentAttempt,
        PaymentIntent paymentIntent)
    {
        if (paymentAttempt.Status == PaymentStatus.Paid)
        {
            return;
        }

        paymentAttempt.Status = PaymentStatus.Failed;
        paymentAttempt.StripePaymentIntentId ??= paymentIntent.Id;
    }

    private static void MarkOrderPaid(Order order, DateTime paidAt)
    {
        order.IsPaid = true;
        order.PaymentMethod = PaymentMethodEnum.OnlineCard;
        order.PaymentStatus = PaymentStatus.Paid;
        order.PaidAt ??= paidAt;
        order.UpdatedAt = paidAt;

        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Paid;
        }
    }

    private static void MarkOrderPaymentUnsuccessful(
        Order order,
        PaymentAttempt paymentAttempt,
        PaymentStatus paymentStatus,
        DateTime updatedAt)
    {
        if (order.IsPaid || HasNewerActiveAttempt(order, paymentAttempt))
        {
            return;
        }

        order.IsPaid = false;
        order.PaymentMethod = PaymentMethodEnum.OnlineCard;
        order.PaymentStatus = paymentStatus;
        order.UpdatedAt = updatedAt;
    }

    private static string CreateConfiguredRedirectUrl(
        string configuredUrl,
        Guid orderId,
        string settingName)
    {
        if (string.IsNullOrWhiteSpace(configuredUrl))
        {
            throw new InvalidOperationException(
                $"StripeSettings:{settingName} is required.");
        }

        if (!configuredUrl.Contains(OrderIdUrlPlaceholder, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"StripeSettings:{settingName} must include the {OrderIdUrlPlaceholder} placeholder.");
        }

        var redirectUrl = configuredUrl.Replace(
            OrderIdUrlPlaceholder,
            Uri.EscapeDataString(orderId.ToString()),
            StringComparison.Ordinal);

        if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                $"StripeSettings:{settingName} must be an absolute HTTP or HTTPS URL.");
        }

        return uri.ToString();
    }

    private sealed class PaymentEventProcessingResult
    {
        public bool ShouldTrack { get; init; }
        public Guid? PaymentAttemptId { get; init; }
        public Guid? OrderId { get; init; }
        public string? RawStatus { get; init; }

        public static PaymentEventProcessingResult NotTracked()
        {
            return new PaymentEventProcessingResult();
        }

        public static PaymentEventProcessingResult Tracked(
            Guid? paymentAttemptId,
            Guid? orderId,
            string? rawStatus)
        {
            return new PaymentEventProcessingResult
            {
                ShouldTrack = true,
                PaymentAttemptId = paymentAttemptId,
                OrderId = orderId,
                RawStatus = rawStatus
            };
        }
    }
}
