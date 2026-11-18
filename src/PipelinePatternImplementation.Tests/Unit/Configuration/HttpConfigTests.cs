using Infra.Configuration.InBound;
using Microsoft.AspNetCore.Builder;

namespace Unit.Configuration
{
    [Trait("Category", "Unit")]
    [Trait("Component", "HttpConfig")]
    public class HttpConfigIntegrationTests
    {
        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotCallAddHttpConfigWithNullApp()
        {
            WebApplication app = null!;

            Assert.Throws<ArgumentNullException>(() =>
                app.AddHttpConfig());
        }
    }
}