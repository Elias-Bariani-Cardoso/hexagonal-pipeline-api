using System.Data;

namespace Domain.Ports.OutBound
{
    public interface ITransactionContext
    {
        IDbConnection Connection { get; }

        IDbTransaction Transaction { get; }
    }
}