namespace Domain.Ports.OutBound
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        ITransactionContext TransactionContext { get; }

        bool HasActiveTransaction { get; }

        Task BeginTransactionAsync(
            CancellationToken cancellationToken = default);

        Task CommitAsync(
            CancellationToken cancellationToken = default);

        Task RollbackAsync(
            CancellationToken cancellationToken = default);
    }
}