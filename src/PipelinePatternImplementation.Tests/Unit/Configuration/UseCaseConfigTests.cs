using Domain.Ports.InBound;
using Domain.Services;
using Domain.UseCases.OrderUseCase;
using Domain.UseCases.Payment;
using Domain.UseCases.Shipping;
using Domain.UseCases.Validation;
using Domain.UseCases.XmlUseCase;
using Infra.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unit.Configuration
{
    [Trait("Category", "Unit")]
    [Trait("Component", "UseCaseConfig")]
    public class UseCaseConfigTests
    {
        [Fact]
        [Trait("Feature", "Registration")]
        public void CanCallAddDomainServices()
        {
            var services = new ServiceCollection();

            var result = services.AddDomainServices();

            Assert.Same(services, result);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotCallAddDomainServicesWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() =>
                UseCaseConfig.AddDomainServices(null!));
        }

        [Fact]
        [Trait("Feature", "PipelineStages")]
        public void CanRegisterOrderProcessingStagesInExpectedOrder()
        {
            var services = new ServiceCollection();

            services.AddDomainServices();

            var stages = services
                .Where(x =>
                    x.ServiceType == typeof(IOrderProcessingStage))
                .ToList();

            Assert.Collection(
                stages,
                stage =>
                {
                    Assert.Equal(
                        typeof(ValidationStage),
                        stage.ImplementationType);

                    Assert.Equal(
                        ServiceLifetime.Scoped,
                        stage.Lifetime);
                },
                stage =>
                {
                    Assert.Equal(
                        typeof(PaymentStage),
                        stage.ImplementationType);

                    Assert.Equal(
                        ServiceLifetime.Scoped,
                        stage.Lifetime);
                },
                stage =>
                {
                    Assert.Equal(
                        typeof(ShippingStage),
                        stage.ImplementationType);

                    Assert.Equal(
                        ServiceLifetime.Scoped,
                        stage.Lifetime);
                });
        }

        [Fact]
        [Trait("Feature", "PipelineRegistration")]
        public void CanRegisterOrderProcessingPipeline()
        {
            var services = new ServiceCollection();

            services.AddDomainServices();

            var registration = Assert.Single(
                services.Where(x =>
                    x.ServiceType == typeof(IOrderProcessingPipeline)));

            Assert.Equal(
                typeof(OrderProcessingPipeline),
                registration.ImplementationType);

            Assert.Equal(
                ServiceLifetime.Scoped,
                registration.Lifetime);
        }

        [Fact]
        [Trait("Feature", "UseCaseRegistration")]
        public void CanRegisterOrderUseCase()
        {
            var services = new ServiceCollection();

            services.AddDomainServices();

            var registration = Assert.Single(
                services.Where(x =>
                    x.ServiceType == typeof(IOrderUseCase)));

            Assert.Equal(
                typeof(OrderUseCase),
                registration.ImplementationType);

            Assert.Equal(
                ServiceLifetime.Scoped,
                registration.Lifetime);
        }

        [Fact]
        [Trait("Feature", "UseCaseRegistration")]
        public void CanRegisterXmlUseCase()
        {
            var services = new ServiceCollection();

            services.AddDomainServices();

            var registration = Assert.Single(
                services.Where(x =>
                    x.ServiceType == typeof(IXmlUseCase)));

            Assert.Equal(
                typeof(XmlUseCase),
                registration.ImplementationType);

            Assert.Equal(
                ServiceLifetime.Scoped,
                registration.Lifetime);
        }
    }
}
