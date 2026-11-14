using RoomBooking.Application.Abstractions;

namespace RoomBooking.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default)
    {
        await Gate.WaitAsync(ct);
        try
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var result = await action();
                await transaction.CommitAsync(ct);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
        finally
        {
            Gate.Release();
        }
    }
}