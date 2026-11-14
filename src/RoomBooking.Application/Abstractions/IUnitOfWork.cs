namespace RoomBooking.Application.Abstractions;

public interface IUnitOfWork
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct = default);
}