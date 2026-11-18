using Adapter.OutBound.AdapterSql.Settings;
using Adapter.OutBound.SqlAdapter.Context;
using Adapter.OutBound.SqlAdapter.Repository;
using Adapter.OutBound.SqlAdapter.UnitOfWork;
using Domain.Ports.OutBound;
using Infra.Configuration.OutBound;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;

namespace Integration.Configuration
{
    [Trait("Category", "Integration")]
    [Trait("Component", "SqlConfig")]
    public class SqlConfigIntegrationTests
    {
        [Fact]
        [Trait("Feature", "Options")]
        public void CanConfigureConnectionSql()
        {
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();

            services.AddSqlConfig(configuration);

            using var provider = services.BuildServiceProvider();

            var options = provider
                .GetRequiredService<IOptions<ConnectionSql>>()
                .Value;

            Assert.Equal("localhost", options.Cluster);
            Assert.Equal("test-user", options.Username);
            Assert.Equal("test-password", options.Password);
            Assert.Equal("TestDatabase", options.Database);
            Assert.Equal("test-key", options.Chave);

            Assert.Equal(45, options.CommandTimeout);
            Assert.Equal(15, options.ConnectTimeout);
            Assert.Equal(100, options.ConnectionPool);
            Assert.True(options.IsTraceExecActive);
        }

        [Fact]
        [Trait("Feature", "DependencyInjection")]
        public async Task CanResolveServicesWithSharedConnection()
        {
            var services = new ServiceCollection();
            var configuration = CreateConfiguration();

            services.AddSqlConfig(configuration);

            await using var expectedConnection = new SqlConnection();

            var connectionSql = new Mock<ConnectionSql>();

            connectionSql.Object.Cluster = "localhost";
            connectionSql.Object.Database = "TestDatabase";
            connectionSql.Object.Username = "test-user";
            connectionSql.Object.Password = "test-password";
            connectionSql.Object.Chave = "test-key";
            connectionSql.Object.CommandTimeout = 45;
            connectionSql.Object.ConnectTimeout = 15;
            connectionSql.Object.ConnectionPool = 100;
            connectionSql.Object.IsTraceExecActive = true;

            connectionSql
                .Setup(x => x.ConnectDataBase("TestDatabase"))
                .Returns(expectedConnection);

            services.Replace(
                ServiceDescriptor.Singleton(
                    Options.Create(connectionSql.Object)));

            await using var provider = services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });

            await using var scope = provider.CreateAsyncScope();

            var scopedProvider = scope.ServiceProvider;

            var connection = scopedProvider
                .GetRequiredService<SqlConnection>();

            var context = scopedProvider
                .GetRequiredService<SqlContext>();

            var repository = scopedProvider
                .GetRequiredService<IXmlRepository>();

            var sqlUnitOfWork = scopedProvider
                .GetRequiredService<SqlUnitOfWork>();

            var unitOfWork = scopedProvider
                .GetRequiredService<IUnitOfWork>();

            var transactionContext = scopedProvider
                .GetRequiredService<ITransactionContext>();

            Assert.IsType<XmlRepository>(repository);

            Assert.Same(expectedConnection, connection);

            Assert.Same(
                connection,
                context.Database.GetDbConnection());

            Assert.Same(
                connection,
                sqlUnitOfWork.Connection);

            Assert.Same(sqlUnitOfWork, unitOfWork);
            Assert.Same(sqlUnitOfWork, transactionContext);

            Assert.Same(
                context,
                scopedProvider.GetRequiredService<SqlContext>());

            Assert.Same(
                repository,
                scopedProvider.GetRequiredService<IXmlRepository>());

            Assert.Equal(
                45,
                context.Database.GetCommandTimeout());

            connectionSql.Verify(
                x => x.ConnectDataBase("TestDatabase"),
                Times.Once);
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