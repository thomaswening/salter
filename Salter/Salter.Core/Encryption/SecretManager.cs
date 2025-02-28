using System.Text.RegularExpressions;

namespace Salter.Encryption;

/// <summary>
/// The <see cref="SecretManager"/> class provides an abstract base for managing secrets from different sources.
/// It supports loading and saving secrets from environment variables and files.
/// </summary>
public abstract partial class SecretManager(SecretManager.SourceType sourceType)
{
    // The pattern for a valid environment variable name.
    // - Must start with a letter or underscore.
    // - Can only contain letters, numbers, and underscores.

    [GeneratedRegex("^[a-zA-Z_][a-zA-Z0-9_]*$")]
    private static partial Regex EnvironmentVariableNamePattern();

    protected readonly SourceType sourceType = sourceType;
    public enum SourceType
    {
        Environment,
        File
    }

    protected static void ValidateSource(string source, SourceType sourceType, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source, paramName);

        // Check if source is valid environment variable name if source type is environment.
        if (sourceType is SourceType.Environment && !EnvironmentVariableNamePattern().IsMatch(source))
        {
            throw new ArgumentException(
                "The environment variable name must start with a letter or underscore and contain only letters, numbers, and underscores.",
                paramName);
        }

        // Check if source is valid file URI if source type is file.
        if (sourceType is SourceType.File
            && (!Uri.TryCreate(source, UriKind.Absolute, out var _) || Uri.TryCreate(source, UriKind.Absolute, out var uri) && !uri.IsFile))
        {
            throw new ArgumentException("The source path must be a valid file URI.", paramName);
        }
    }

    protected static byte[] LoadFromEnvironment(string source)
    {
        try
        {
            var base64 = Environment.GetEnvironmentVariable(source, EnvironmentVariableTarget.User);
            if (string.IsNullOrEmpty(base64))
            {
                throw new InvalidOperationException($"Error while attempting to load data: Environment variable {source} not found.");
            }

            return Convert.FromBase64String(base64);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load from environment variable '{source}'.", ex);
        }
    }

    protected static void SaveToEnvironment(string source, byte[] data)
    {
        try
        {
            var base64 = Convert.ToBase64String(data);

            Environment.SetEnvironmentVariable("TEMP_" + source, base64, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable(source, base64, EnvironmentVariableTarget.User);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save to environment variable '{source}'.", ex);
        }
    }

    protected static byte[] LoadFromFile(string source)
    {
        try
        {
            return File.ReadAllBytes(source);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load from file '{source}'.", ex);
        }
    }

    protected static async Task<byte[]> LoadFromFileAsync(string source)
    {
        try
        {
            return await File.ReadAllBytesAsync(source);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load from file asynchronously '{source}'.", ex);
        }
    }

    protected static void SaveToFile(string source, byte[] data)
    {
        try
        {
            File.WriteAllBytes(source, data);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save to file '{source}'.", ex);
        }
    }

    protected static async Task SaveToFileAsync(string source, byte[] data)
    {
        try
        {
            await File.WriteAllBytesAsync(source, data);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save to file asynchronously '{source}'.", ex);
        }
    }
}
