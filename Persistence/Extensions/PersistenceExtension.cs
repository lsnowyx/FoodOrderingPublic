using Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using Application.Abstractions.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.Repositories;
using Persistence.Transactions;

namespace Persistence.Extensions
{
    public static class PersistenceExtension
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext(configuration);
            services.AddRepositories();
            return services;
        }

        private static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IRequestsRepository, RequestsRepository>();
            services.AddTransient<ICategoriesRepository, CategoriesRepository>();
            services.AddTransient<IIngredientsRepository, IngredientsRepository>();
            services.AddTransient<IMenuItemsRepository, MenuItemsRepository>();
            services.AddTransient<IMenuItemIngredientsRepository, MenuItemIngredientsRepository>();
            services.AddTransient<IMenuItemPicturesRepository, MenuItemPicturesRepository>();
            services.AddTransient<IOrdersRepository, OrdersRepository>();
            services.AddTransient<IOrderTrackingLinksRepository, OrderTrackingLinksRepository>();
            services.AddTransient<IDeliveryTrackingSessionsRepository, DeliveryTrackingSessionsRepository>();
            services.AddTransient<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IApplicationTransaction, EfApplicationTransaction>();
            services.AddTransient<IPaymentAttemptRepository, PaymentAttemptRepository>();
            services.AddTransient<IProcessedPaymentEventRepository, ProcessedPaymentEventRepository>();
            return services;
        }
    }
}
