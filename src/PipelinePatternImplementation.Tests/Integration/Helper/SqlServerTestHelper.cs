using Adapter.OutBound.SqlAdapter.Context;
using Adapter.OutBound.SqlAdapter.UnitOfWork;
using Domain.Ports.OutBound;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;

namespace Integration.Helpers
{
    public sealed class SqlServerTestHelper : IAsyncLifetime
    {
        private readonly MsSqlContainer _container;
        private readonly string _databaseName;
        private readonly int _commandTimeout;

        private bool _disposed;

        public string ConnectionString { get; private set; } =
            string.Empty;

        public SqlServerTestHelper()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(
                    "appsettings.Development.json",
                    optional: false,
                    reloadOnChange: false)
                .Build();

            var settings = configuration
                .GetSection("SqlServerTests");

            var image = GetRequiredValue(settings, "Image");

            _databaseName = GetRequiredValue(
                settings,
                "Database");

            _commandTimeout =
                settings.GetValue<int?>("CommandTimeout") ?? 30;

            if (_commandTimeout <= 0)
            {
                throw new InvalidOperationException(
                    "SqlServerTests:CommandTimeout deve ser maior que zero.");
            }

            _container = new MsSqlBuilder(image)
                .Build();
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _container.StartAsync();

                var connectionStringBuilder =
                    new SqlConnectionStringBuilder(
                        _container.GetConnectionString())
                    {
                        InitialCatalog = _databaseName,
                        Pooling = false
                    };

                ConnectionString =
                    connectionStringBuilder.ConnectionString;

                await using var context = CreateContext();

                await context.Database.EnsureCreatedAsync();
            }
            catch
            {
                await DisposeAsync();
                throw;
            }
        }

        public ServiceProvider CreateServiceProvider()
        {
            EnsureAvailable();

            var services = new ServiceCollection();

            services.AddScoped<SqlConnection>(_ =>
                new SqlConnection(ConnectionString));

            services.AddScoped<SqlContext>(serviceProvider =>
            {
                var connection = serviceProvider
                    .GetRequiredService<SqlConnection>();

                var options =
                    new DbContextOptionsBuilder<SqlContext>()
                        .UseSqlServer(
                            connection,
                            sqlOptions =>
                            {
                                sqlOptions.CommandTimeout(
                                    _commandTimeout);
                            })
                        .Options;

                return new TransactionTestContext(options);
            });

            services.AddScoped<SqlUnitOfWork>();

            services.AddScoped<IUnitOfWork>(serviceProvider =>
                serviceProvider
                    .GetRequiredService<SqlUnitOfWork>());

            services.AddScoped<ITransactionContext>(serviceProvider =>
                serviceProvider
                    .GetRequiredService<SqlUnitOfWork>());

            return services.BuildServiceProvider(
                new ServiceProviderOptions
                {
                    ValidateOnBuild = true,
                    ValidateScopes = true
                });
        }

        public SqlContext CreateContext()
        {
            EnsureAvailable();

            var options = new DbContextOptionsBuilder<SqlContext>()
                .UseSqlServer(
                    ConnectionString,
                    sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(
                            _commandTimeout);
                    })
                .Options;

            return new TransactionTestContext(options);
        }

        private void EnsureAvailable()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                throw new InvalidOperationException(
                    "O SQL Server de testes ainda não foi inicializado.");
            }
        }

        private static string GetRequiredValue(
            IConfigurationSection section,
            string key)
        {
            var value = section[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"A configuração obrigatória " +
                    $"'SqlServerTests:{key}' não foi definida.");
            }

            return value;
        }

        public async Task DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            await _container.DisposeAsync();
        }
    }
}