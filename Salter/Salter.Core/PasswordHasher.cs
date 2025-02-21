using System.Security.Cryptography;
using System.Text;

namespace Salter.Core;

/// <summary>
/// The <see cref="PasswordHasher"/> class provides methods for hashing, salting and validating passwords.
/// </summary>s
public class PasswordHasher
{
    private readonly HashAlgorithmName hashAlgorithm;
    private readonly int keySize;
    private readonly int iterations;

    public PasswordHasher(int keySize = 64, int iterations = 350_000, HashAlgorithmName hashAlgorithm = default)
    {
        this.keySize = keySize;
        this.iterations = iterations;
        this.hashAlgorithm = hashAlgorithm == default ? HashAlgorithmName.SHA512 : hashAlgorithm;
    }

    public string GenerateHash(string password, out string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        var saltBytes = RandomNumberGenerator.GetBytes(keySize);
        salt = Convert.ToHexString(saltBytes);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            iterations,
            hashAlgorithm,
            keySize);

        return Convert.ToHexString(hash);
    }

    public bool Validate(string password, string hash, string salt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));
        ArgumentException.ThrowIfNullOrWhiteSpace(hash, nameof(hash));
        ArgumentException.ThrowIfNullOrWhiteSpace(salt, nameof(salt));

        var saltBytes = Convert.FromHexString(salt);
        var newHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            iterations,
            hashAlgorithm,
            keySize);

        var isValid = CryptographicOperations.FixedTimeEquals(newHash, Convert.FromHexString(hash));

        return isValid;
    }
}
