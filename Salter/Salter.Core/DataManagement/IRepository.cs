using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salter.Core.DataManagement;

/// <summary>
/// Defines a repository interface for managing records of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the records managed by the repository. Must be a non-nullable type and inherit from <see cref="Entity"/>.</typeparam>
public interface IRepository<T> where T : notnull, Entity
{
    /// <summary>
    /// Gets the cache of records managed by the repository.
    /// If the cache is not initialized, this property returns <see langword="null"/>.
    /// It should be initialized by calling <see cref="Initialize"/> or <see cref="InitializeAsync"/> before accessing the cache.
    /// </summary>
    List<T> Cache { get; }
    Task AddRecordAsync(T record);
    Task ClearAllRecordsAsync();
    Task<List<T>> GetRecordsAsync();

    /// <summary>
    /// Asynchronously initializes the repository. This method should be called before any other repository operations.
    /// It also initializes the cache of records.
    /// </summary>
    Task InitializeAsync();
    Task RefreshCacheAsync();

    /// <summary>
    /// Asynchronously removes a record from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the record to remove.</param>
    Task RemoveRecordAsync(Guid id);

    /// <summary>
    /// Asynchronously removes a record from the repository.
    /// </summary>
    /// <param name="record">The record to remove.</param>
    Task RemoveRecordAsync(T record);

    /// <summary>
    /// Asynchronously updates an existing record in the repository.
    /// </summary>
    /// <param name="newRecord">The new record data to update.</param>
    Task UpdateRecordAsync(T newRecord);
}