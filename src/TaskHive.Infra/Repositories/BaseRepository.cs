using Npgsql;

namespace TaskHive.Infrastructure.Repositories;

public abstract class BaseRepository : IDisposable
{
    protected readonly NpgsqlConnection _connection;
    private bool _disposed;

    protected BaseRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    protected async Task EnsureConnectionOpenAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
        {
            await _connection.OpenAsync(cancellationToken);
        }
    }

    protected async Task<NpgsqlTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await EnsureConnectionOpenAsync(cancellationToken);
        return await _connection.BeginTransactionAsync(cancellationToken);
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
                _connection.Dispose();
            }
            _disposed = true;
        }
    }

    ~BaseRepository()
    {
        Dispose(false);
    }
} 