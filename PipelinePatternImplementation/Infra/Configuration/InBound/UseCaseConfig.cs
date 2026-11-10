using Domain.Ports.InBound;
using Domain.Services;
using Domain.UseCases.OrderUseCase;
using Domain.UseCases.Payment;
using Domain.UseCases.Shipping;
using Domain.UseCases.Validation;

namespace Infra.Configuration
{
    public static class UseCaseConfig
    {
        public static IServiceCollection AddDomainServices(
            this IServiceCollection services)
        {
            services.AddScoped<IOrderProcessingStage, ValidationStage>();
            services.AddScoped<IOrderProcessingStage, PaymentStage>();
            services.AddScoped<IOrderProcessingStage, ShippingStage>();

            services.AddScoped<
                IOrderProcessingPipeline,
                OrderProcessingPipeline>();

            services.AddScoped<IOrderUseCase, OrderUseCase>();

            return services;
        }
    }
}