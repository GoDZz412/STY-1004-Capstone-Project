namespace IoTPlatform.Application.Abstractions.Repositories;

/// <summary>
/// Saves all changes made through the repositories as one atomic operation.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
