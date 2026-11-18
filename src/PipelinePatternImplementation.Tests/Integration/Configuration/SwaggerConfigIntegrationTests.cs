using Infra.Configuration.InBound;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Integration.Configuration
{
    [Trait("Category", "Integration")]
    [Trait("Component", "SwaggerConfig")]
    public class SwaggerConfigIntegrationTests
    {
        [Fact]
        [Trait("Feature", "DocumentConfiguration")]
        public async Task CanConfigureSwaggerDocument()
        {
            var builder = WebApplication.CreateBuilder(
                new WebApplicationOptions
                {
                    EnvironmentName = Environments.Development
                });

            builder.Services.AddSwaggerConfiguration();

            await using var app = builder.Build();

            var options = app.Services
                .GetRequiredService<IOptions<SwaggerGenOptions>>()
                .Value;

            var documents = options
                .SwaggerGeneratorOptions
                .SwaggerDocs;

            var document = Assert.Single(documents);

            Assert.Equal("v1", document.Key);

            Assert.Equal(
                "Order Processing API",
                document.Value.Title);

            Assert.Equal(
                "v1",
                document.Value.Version);

            Assert.Equal(
                "API de processamento de pedidos com pipeline de etapas.",
                document.Value.Description);
        }

        [Theory]
        [InlineData("Development")]
        [InlineData("Staging")]
        [InlineData("Production")]
        [Trait("Feature", "MiddlewareConfiguration")]
        public async Task CanCallMapSwaggerInDifferentEnvironments(
            string environment)
        {
            var builder = WebApplication.CreateBuilder(
                new WebApplicationOptions
                {
                    EnvironmentName = environment
                });

            builder.Services.AddSwaggerConfiguration();

            await using var app = builder.Build();

            var result = app.MapSwagger();

            Assert.Same(app, result);
        }
    }
}
