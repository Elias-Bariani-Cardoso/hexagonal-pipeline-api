using Infra.Configuration.InBound;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Unit.Configuration
{
    [Trait("Category", "Unit")]
    [Trait("Component", "SwaggerConfig")]
    public class SwaggerConfigTests
    {
        [Fact]
        [Trait("Feature", "Registration")]
        public void CanCallAddSwaggerConfiguration()
        {
            var services = new ServiceCollection();

            var result = services.AddSwaggerConfiguration();

            Assert.Same(services, result);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotCallAddSwaggerConfigurationWithNullServices()
        {
            Assert.Throws<ArgumentNullException>(() =>
                SwaggerConfig.AddSwaggerConfiguration(null!));
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotCallMapSwaggerWithNullApp()
        {
            WebApplication app = null!;

            Assert.Throws<ArgumentNullException>(() =>
                app.MapSwagger());
        }
    }
}
