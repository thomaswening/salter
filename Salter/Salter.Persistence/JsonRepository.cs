using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using Salter.Core.DataManagement;

using Salter.Core.Encryption;

namespace Salter.Persistence;


/// <summary>
/// A repository implementation for managing JSON records of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the records managed by the repository. Must be a non-nullable type and inherit from <see cref="Entity"/>.</typeparam>
/// <typeparam name="TDto">The type of the DTO used for deserialization. Must implement <see cref="IDataTransferObject{T}"/>.</typeparam>
internal class JsonRepository<T, TDto> : Repository<T>
    where T : notnull, Entity
    where TDto : IDataTransferObject<T>
{
    private const string EmptyJsonArray = "[]";

    private readonly Mapper<T, TDto> mapper;

    /// <param name="location">The location of the JSON file used to store the records.</param>
    /// <param name="encryptor">The encryptor used to encrypt and decrypt the data.</param>
    public JsonRepository(Uri location, IEncryptor encryptor, Mapper<T, TDto> mapper) : base(location, encryptor)
    {
        if (!location.IsFile)
        {
            throw new ArgumentException("The location must be a file URI.", nameof(location));
        }

        this.mapper = mapper;
    }

    /// <inheritdoc/>
    public override async Task AddRecordAsync(T record)
    {
        Cache.Add(record);

        try
        {
            var data = await SerializeAndEncryptCacheAsync().ConfigureAwait(false);
            await File.WriteAllTextAsync(location.LocalPath, data).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Cache.Remove(record);
            ProcessAndThrowException(e, "add record");
        }
    }

    /// <inheritdoc/>
    public override async Task ClearAllRecordsAsync()
    {
        try
        {
            var utf8Bytes = Encoding.UTF8.GetBytes(EmptyJsonArray);
            var encryptedData = await encryptor.EncryptAsync(utf8Bytes).ConfigureAwait(false);
            await File.WriteAllTextAsync(location.LocalPath, Convert.ToBase64String(encryptedData)).ConfigureAwait(false);
            Cache.Clear();
        }
        catch (Exception e)
        {
            ProcessAndThrowException(e, "clear all records");
        }
    }

    /// <inheritdoc/>
    public override async Task<List<T>> GetRecordsAsync()
    {
        try
        {
            var data = await File.ReadAllTextAsync(location.LocalPath).ConfigureAwait(false);
            var encryptedBytes = Convert.FromBase64String(data);
            var decryptedBytes = await encryptor.DecryptAsync(encryptedBytes).ConfigureAwait(false);
            var json = Encoding.UTF8.GetString(decryptedBytes);
            var dtos = JsonSerializer.Deserialize<List<TDto>>(json) ?? [];
            return mapper.MapToModels(dtos);
        }
        catch (Exception e)
        {
            ProcessAndThrowException(e, "get records");
            return [];
        }
    }

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        var directory = Path.GetDirectoryName(location.LocalPath);

        try
        {
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(location.LocalPath))
            {
                var utf8Bytes = Encoding.UTF8.GetBytes(EmptyJsonArray);
                var encryptedData = await encryptor.EncryptAsync(utf8Bytes).ConfigureAwait(false);
                await File.WriteAllTextAsync(location.LocalPath, Convert.ToBase64String(encryptedData)).ConfigureAwait(false);
            }
            else
            {
                await RefreshCacheAsync().ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            ProcessAndThrowException(e, "initialize repository");
        }

        await RefreshCacheAsync().ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task RemoveRecordAsync(T record)
    {
        if (!Cache.Contains(record))
        {
            throw new RecordNotFoundException();
        }

        Cache.Remove(record);

        try
        {
            var data = await SerializeAndEncryptCacheAsync().ConfigureAwait(false);
            await File.WriteAllTextAsync(location.LocalPath, data).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Cache.Add(record);
            ProcessAndThrowException(e, "remove record");
        }
    }

    /// <inheritdoc/>
    public override async Task RemoveRecordAsync(Guid id)
    {
        var record = Cache.FirstOrDefault(u => u.Id.Equals(id))
            ?? throw new RecordNotFoundException();

        Cache.Remove(record);

        try
        {
            var data = await SerializeAndEncryptCacheAsync().ConfigureAwait(false);
            await File.WriteAllTextAsync(location.LocalPath, data).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Cache.Add(record);
            ProcessAndThrowException(e, "remove record");
        }
    }

    /// <inheritdoc/>
    public override async Task UpdateRecordAsync(T newRecord)
    {
        var oldUser = Cache.FirstOrDefault(u => u.Equals(newRecord))
            ?? throw new RecordNotFoundException();

        var index = Cache.IndexOf(oldUser);
        Cache[index] = newRecord;

        try
        {
            var data = await SerializeAndEncryptCacheAsync().ConfigureAwait(false);
            await File.WriteAllTextAsync(location.LocalPath, data).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Cache[index] = oldUser;
            ProcessAndThrowException(e, "update record");
        }
    }

    private async Task<string> SerializeAndEncryptCacheAsync()
    {
        var dtos = mapper.MapToDataTransferObjects(Cache);
        var json = JsonSerializer.Serialize(dtos);
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        var encryptedBytes = await encryptor.EncryptAsync(jsonBytes).ConfigureAwait(false);
        return Convert.ToBase64String(encryptedBytes);
    }

    private void ProcessAndThrowException(Exception ex, string operation)
    {
        const string ErrorMessagePrefix = "Error while attempting to";
        var operationMsg = $"{ErrorMessagePrefix} {operation}:";

        var message = ex switch
        {
            FileNotFoundException       => $"The file at {location.LocalPath} was not found.",
            UnauthorizedAccessException => $"Access to the file at {location.LocalPath} is denied.",
            IOException                 => $"An I/O error occurred while accessing the file at {location.LocalPath}.",
            JsonException               => "An error occurred while serializing or deserializing the data.",
            CryptographicException      => "An error occurred while encrypting or decrypting the data.",
            FormatException             => "The data is in an invalid format.",
            _                           => "An unexpected error occurred.",
        };

        message = $"{operationMsg} {message}";

        throw new RepositoryException(message, ex);
    }
}   