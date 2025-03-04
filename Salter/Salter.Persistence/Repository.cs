using Salter.Core.DataManagement;
using Salter.Core.Encryption;

namespace Salter.Persistence;

/// <summary>
/// An abstract repository class that implements the cache logic for managing records of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the records managed by the repository. Must be a non-nullable type and inherit from <see cref="Entity"/>.</typeparam>
public abstract class Repository<T> : IRepository<T> where T : notnull, Entity
{
    protected readonly Uri location;
    protected readonly IEncryptor encryptor;

    private List<T>? cache;

    public Repository(Uri location, IEncryptor encryptor)
    {
        this.location = location;
        this.encryptor = encryptor;
    }

    /// <summary>
    /// Gets the cache of records managed by the repository.
    /// If the cache is not initialized, the getter calls <see cref="GetRecordsAsync"/> to initialize the cache asynchronously.
    /// To initialize the cache asynchronously, call <see cref="RefreshCacheAsync"/> before accessing the cache 
    /// or call <see cref="InitializeAsync"/> after creating the repository instance.
    /// </summary>
    public List<T> Cache
    {
        get
        {
            cache ??= GetRecordsAsync().GetAwaiter().GetResult();
            return cache;
        }

        private set => cache = value;
    }

    /// <inheritdoc/>
    public async Task RefreshCacheAsync()
    {
        Cache = await GetRecordsAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public abstract Task InitializeAsync();

    /// <inheritdoc/>
    public abstract Task AddRecordAsync(T record);

    /// <inheritdoc/>
    public abstract Task<List<T>> GetRecordsAsync();

    /// <inheritdoc/>
    public abstract Task RemoveRecordAsync(T record);

    /// <inheritdoc/>
    public abstract Task RemoveRecordAsync(Guid id);

    /// <inheritdoc/>
    public abstract Task UpdateRecordAsync(T newRecord);

    /// <inheritdoc/>
    public abstract Task ClearAllRecordsAsync();

    /// <inheritdoc/>
    public abstract Task DeleteRepositoryAsync();
}

public class RepositoryException : Exception
{
    public RepositoryException(string message) : base(message)
    {
    }
    public RepositoryException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class RecordNotFoundException : RepositoryException
{
    private const string RecordDoesNotExistMessage = "The record does not exist.";
    public RecordNotFoundException() : base(RecordDoesNotExistMessage)
    {
    }

    public RecordNotFoundException(string message) : base(message)
    {
    }

    public RecordNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}