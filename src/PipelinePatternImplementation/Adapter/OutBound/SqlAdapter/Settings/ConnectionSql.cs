using Microsoft.Data.SqlClient;
using System.Data;

namespace Adapter.OutBound.AdapterSql.Settings
{
    public record ConnectionSql
    {
        public string Cluster { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public int CommandTimeout { get; set; } = 30;
        public int ConnectTimeout { get; set; } = 10;
        public int ConnectionPool { get; set; } = 0;

        public string Chave { get; set; } = string.Empty;
        public bool IsTraceExecActive { get; set; }

        public virtual IDbConnection ConnectDataBase(string dataBaseName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(dataBaseName);

            string password = Crypt.decryptDES(Password, Chave);

            string maxPoolSize = ConnectionPool > 0
                ? $"Max Pool Size={ConnectionPool};"
                : string.Empty;

            string connectionString =
                $"Data Source={Cluster};" +
                $"Initial Catalog={dataBaseName};" +
                $"Persist Security Info=True;" +
                $"User ID={Username};" +
                $"Password={password};" +
                $"MultipleActiveResultSets=True;" +
                $"Connect Timeout={ConnectTimeout};" +
                $"TrustServerCertificate=True;" +
                $"Application Name=Catalogo;" +
                maxPoolSize;

            return new SqlConnection(connectionString);
        }
    }
}