using Infra.Configuration.OutBound;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Unit.Configuration
{
    [Trait("Category", "Unit")]
    [Trait("Component", "SqlConfig")]
    public class SqlConfigTests
    {
        [Fact]
        [Trait("Feature", "Registration")]
        public void CanCallAddSqlConfig()
        {
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();

            var result = services.AddSqlConfig(configuration);

            Assert.Same(services, result);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotCallAddSqlConfigWithNullServices()
        {
            var configuration = CreateConfiguration();

            Assert.Throws<ArgumentNullException>(() =>
                SqlConfig.AddSqlConfig(null!, configuration));
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotCallAddSqlConfigWithNullConfiguration()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() =>
                services.AddSqlConfig(null!));
        }

        [Theory]
        [InlineData("Cluster")]
        [InlineData("Username")]
        [InlineData("Password")]
        [InlineData("Database")]
        [InlineData("Chave")]
        [Trait("Feature", "ConfigurationValidation")]
        public void CannotCallAddSqlConfigWithMissingRequiredValue(
            string key)
        {
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();

            configuration[$"DBConnection:{key}"] = null;

            var exception = Assert.Throws<InvalidOperationException>(
                () => services.AddSqlConfig(configuration));

            Assert.Contains(
                $"DBConnection:{key}",
                exception.Message);
        }

        private static IConfigurationRoot CreateConfiguration()
        {
            var values = new Dictionary<string, string?>
            {
                ["DBConnection:Cluster"] = "localhost",
                ["DBConnection:Username"] = "test-user",
                ["DBConnection:Password"] = "test-password",
                ["DBConnection:Database"] = "TestDatabase",
                ["DBConnection:Chave"] = "test-key",
                ["DBConnection:CommandTimeout"] = "45",
                ["DBConnection:ConnectTimeout"] = "15",
                ["DBConnection:ConnectionPool"] = "100",
                ["DBConnection:IsTraceExecActive"] = "true"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();
        }
    }
}