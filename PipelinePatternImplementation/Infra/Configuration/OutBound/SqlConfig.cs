using Adapter.OutBound.AdapterSql.Settings;
using Adapter.OutBound.SqlAdapter.Context;
using Adapter.OutBound.SqlAdapter.Repository;
using Adapter.OutBound.SqlAdapter.UnitOfWork;
using Domain.Ports.OutBound;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infra.Configuration.OutBound
{
    public static class SqlConfig
    {
        public static IServiceCollection AddSqlConfig(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            IConfigurationSection settings =
                configuration.GetSection("DBConnection");

            string cluster = GetRequiredValue(settings, "Cluster");
            string username = GetRequiredValue(settings, "Username");
            string password = GetRequiredValue(settings, "Password");
            string database = GetRequiredValue(settings, "Database");
            string chave = GetRequiredValue(settings, "Chave");

            services.Configure<ConnectionSql>(options =>
            {
                options.Cluster = cluster;
                options.Username = username;
                options.Password = password;
                options.Database = database;
                options.Chave = chave;

                options.CommandTimeout =
                    settings.GetValue<int?>("CommandTimeout") ?? 30;

                options.ConnectTimeout =
                    settings.GetValue<int?>("ConnectTimeout") ?? 10;

                options.ConnectionPool =
                    settings.GetValue<int?>("ConnectionPool") ?? 0;

                options.IsTraceExecActive =
                    settings.GetValue<bool?>("IsTraceExecActive") ?? false;
            });

            // conexão compartilhada durante o escopo.
            services.AddScoped<SqlConnection>(serviceProvider =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<ConnectionSql>>()
                    .Value;

                var connection = options.ConnectDataBase(options.Database);

                if (connection is not SqlConnection sqlConnection)
                {
                    connection.Dispose();

                    throw new InvalidOperationException(
                        "ConnectionSql deve retornar uma SqlConnection.");
                }

                return sqlConnection;
            });

            // configura o EF com a mesma conexão usada pelo unit of work.
            services.AddDbContext<SqlContext>((serviceProvider, options) =>
            {
                var connection = serviceProvider
                    .GetRequiredService<SqlConnection>();

                var settings = serviceProvider
                    .GetRequiredService<IOptions<ConnectionSql>>()
                    .Value;

                options.UseSqlServer(
                    connection,
                    sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(settings.CommandTimeout);
                    });
            });

            // repository.
            services.AddScoped<IXmlRepository, XmlRepository>();

            // uma única instância do unit of work por escopo.
            services.AddScoped<SqlUnitOfWork>();

            services.AddScoped<IUnitOfWork>(serviceProvider =>
                serviceProvider.GetRequiredService<SqlUnitOfWork>());

            services.AddScoped<ITransactionContext>(serviceProvider =>
                serviceProvider.GetRequiredService<SqlUnitOfWork>());

            return services;
        }

        private static string GetRequiredValue(
            IConfigurationSection section,
            string key)
        {
            string? value = section[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"A configuração obrigatória 'DBConnection:{key}' não foi definida.");
            }

            return value;
        }
    }
}