using Salter.Encryption;

namespace Salter.Core.Encryption;

/// <summary>
/// The <see cref="KeyManager"/> class extends the <see cref="SecretManager"/> class to manage encryption keys and initialization vectors.
/// It supports loading and saving keys and initialization vectors from environment variables and files.
/// </summary>
public class KeyManager : SecretManager
{
    public class Options
    {
        public SourceType SourceType { get; set; }

        /// <summary>
        /// The source of the encryption key: 
        /// For <see cref="SourceType.Environment"/>, the environment variable name.
        /// For <see cref="SourceType.File"/>, the local file path.
        /// </summary>
        public string KeySource { get; set; } = string.Empty;
        public string InitializationVectorSource { get; set; } = string.Empty;
    }

    private readonly Options options;

    public KeyManager(Options options) : base(options.SourceType)
    {
        ValidateSource(options.KeySource, options.SourceType, nameof(options.KeySource));
        ValidateSource(options.InitializationVectorSource, options.SourceType, nameof(options.InitializationVectorSource));

        this.options = options;
    }

    public (byte[] key, byte[] initializationVector) Load()
    {
        return options.SourceType switch
        {
            SourceType.Environment => (LoadFromEnvironment(options.KeySource), LoadFromEnvironment(options.InitializationVectorSource)),
            SourceType.File => (LoadFromFile(options.KeySource), LoadFromFile(options.InitializationVectorSource)),
            _ => throw new InvalidOperationException("Invalid key source type.")
        };
    }

    public async Task<(byte[] key, byte[] initializationVector)> LoadAsync()
    {
        return options.SourceType switch
        {
            SourceType.Environment => (LoadFromEnvironment(options.KeySource), LoadFromEnvironment(options.InitializationVectorSource)),
            SourceType.File => (await LoadFromFileAsync(options.KeySource), await LoadFromFileAsync(options.InitializationVectorSource)),
            _ => throw new InvalidOperationException("Invalid key source type.")
        };
    }

    public void Save(byte[] key, byte[] initializationVector)
    {
        switch (options.SourceType)
        {
            case SourceType.Environment:
                SaveToEnvironment(options.KeySource, key);
                SaveToEnvironment(options.InitializationVectorSource, initializationVector);
                break;
            case SourceType.File:
                SaveToFile(options.KeySource, key);
                SaveToFile(options.InitializationVectorSource, initializationVector);
                break;
            default:
                throw new InvalidOperationException("Invalid key source type.");
        }
    }

    public async Task SaveAsync(byte[] key, byte[] initializationVector)
    {
        switch (options.SourceType)
        {
            case SourceType.Environment:
                SaveToEnvironment(options.KeySource, key);
                SaveToEnvironment(options.InitializationVectorSource, initializationVector);
                break;
            case SourceType.File:
                await SaveToFileAsync(options.KeySource, key);
                await SaveToFileAsync(options.InitializationVectorSource, initializationVector);
                break;
            default:
                throw new InvalidOperationException("Invalid key source type.");
        }
    }
}
