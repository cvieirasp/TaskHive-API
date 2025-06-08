using Npgsql;

namespace TaskHive.Infrastructure.Repositories;

public abstract class BaseRepository(NpgsqlConnection connection) : IDisposable
{
    private bool _disposed;

    protected async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken = default)
    {
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }
    }

    protected async Task<NpgsqlTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);
        return await connection.BeginTransactionAsync(cancellationToken);
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(
        Func<NpgsqlTransaction, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation(transaction);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                connection.Dispose();
            }
            _disposed = true;
        }
    }

    ~BaseRepository()
    {
        Dispose(false);
    }
} 