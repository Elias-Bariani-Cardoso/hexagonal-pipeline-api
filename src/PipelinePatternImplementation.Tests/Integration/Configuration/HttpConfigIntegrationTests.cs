using Infra.Configuration.InBound;
using Microsoft.AspNetCore.Builder;

namespace Integration.Configuration
{
    [Trait("Category", "Integration")]
    [Trait("Component", "HttpConfig")]
    public class HttpConfigIntegrationTests
    {
        [Fact]
        [Trait("Feature", "PipelineConfiguration")]
        public async Task CanCallAddHttpConfig()
        {
            var builder = WebApplication.CreateBuilder(
                Array.Empty<string>());

            await using var app = builder.Build();

            var result = app.AddHttpConfig();

            Assert.Same(app, result);
        }
    }
}