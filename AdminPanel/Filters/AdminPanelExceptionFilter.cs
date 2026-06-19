using AdminPanel.Models;
using AdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Filters;

public sealed class AdminPanelExceptionFilter : IExceptionFilter
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AdminPanelExceptionFilter> _logger;
    private readonly IModelMetadataProvider _modelMetadataProvider;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IUrlHelperFactory _urlHelperFactory;

    public AdminPanelExceptionFilter(
        IHostEnvironment environment,
        ILogger<AdminPanelExceptionFilter> logger,
        IModelMetadataProvider modelMetadataProvider,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IUrlHelperFactory urlHelperFactory)
    {
        _environment = environment;
        _logger = logger;
        _modelMetadataProvider = modelMetadataProvider;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _urlHelperFactory = urlHelperFactory;
    }

    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case KeyNotFoundException exception:
                _logger.LogWarning(exception, "AdminPanel resource was not found.");
                context.Result = CreateViewResult(
                    context,
                    "~/Views/Shared/NotFound.cshtml",
                    new ErrorViewModel { Message = MvcErrorHelper.GetDisplayMessage(exception) },
                    StatusCodes.Status404NotFound);
                context.ExceptionHandled = true;
                return;

            case UnauthorizedAccessException exception:
                _logger.LogWarning(exception, "AdminPanel authorization failed.");
                context.Result = new RedirectToActionResult(
                    nameof(Controllers.AuthController.AccessDenied),
                    "Auth",
                    null);
                context.ExceptionHandled = true;
                return;

            case ValidationException or ArgumentException or InvalidOperationException:
                HandleBusinessException(context);
                return;
        }

        _logger.LogError(context.Exception, "Unhandled AdminPanel exception.");

        if (_environment.IsDevelopment())
        {
            return;
        }

        context.Result = CreateViewResult(
            context,
            "~/Views/Shared/Error.cshtml",
            new ErrorViewModel
            {
                RequestId = context.HttpContext.TraceIdentifier,
                Message = "An unexpected error occurred while processing your request."
            },
            StatusCodes.Status500InternalServerError);
        context.ExceptionHandled = true;
    }

    private void HandleBusinessException(ExceptionContext context)
    {
        _logger.LogWarning(context.Exception, "AdminPanel business operation failed.");

        var message = MvcErrorHelper.GetDisplayMessage(context.Exception);
        if (HttpMethods.IsPost(context.HttpContext.Request.Method)
            || HttpMethods.IsPut(context.HttpContext.Request.Method)
            || HttpMethods.IsPatch(context.HttpContext.Request.Method)
            || HttpMethods.IsDelete(context.HttpContext.Request.Method))
        {
            var tempData = _tempDataDictionaryFactory.GetTempData(context.HttpContext);
            MvcErrorHelper.SetErrorMessage(tempData, message);

            context.Result = CreateSafeRedirect(context);
        }
        else
        {
            context.Result = CreateViewResult(
                context,
                "~/Views/Shared/Error.cshtml",
                new ErrorViewModel { Message = message },
                StatusCodes.Status400BadRequest);
        }

        context.ExceptionHandled = true;
    }

    private IActionResult CreateSafeRedirect(ExceptionContext context)
    {
        var urlHelper = _urlHelperFactory.GetUrlHelper(context);
        var referrer = context.HttpContext.Request.Headers["Referer"].ToString();
        var localUrl = GetLocalReferrer(context.HttpContext.Request, referrer);

        if (!string.IsNullOrWhiteSpace(localUrl) && urlHelper.IsLocalUrl(localUrl))
        {
            return new LocalRedirectResult(localUrl);
        }

        var controllerName = context.RouteData.Values["controller"]?.ToString() ?? "Home";
        return new RedirectToActionResult("Index", controllerName, null);
    }

    private static string? GetLocalReferrer(HttpRequest request, string? referrer)
    {
        if (string.IsNullOrWhiteSpace(referrer))
        {
            return null;
        }

        if (Uri.TryCreate(referrer, UriKind.Relative, out _))
        {
            return referrer;
        }

        if (!Uri.TryCreate(referrer, UriKind.Absolute, out var absoluteReferrer)
            || !string.Equals(
                absoluteReferrer.Authority,
                request.Host.Value,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return absoluteReferrer.PathAndQuery;
    }

    private IActionResult CreateViewResult(
        ExceptionContext context,
        string viewName,
        ErrorViewModel model,
        int statusCode)
    {
        var viewData = new ViewDataDictionary(
            _modelMetadataProvider,
            context.ModelState)
        {
            Model = model
        };

        return new ViewResult
        {
            ViewName = viewName,
            ViewData = viewData,
            TempData = _tempDataDictionaryFactory.GetTempData(context.HttpContext),
            StatusCode = statusCode
        };
    }
}
