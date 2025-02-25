using System.Security.Cryptography;
using System.Text;

namespace Salter.Core;

/// <summary>
/// The <see cref="PasswordHasher"/> class provides methods for hashing, salting and validating passwords.
/// </summary>
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

    public string GenerateHash(char[] password, out string salt)
    {
        if (password == null || password.Length == 0)
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        var saltBytes = RandomNumberGenerator.GetBytes(keySize);
        salt = Convert.ToHexString(saltBytes);

        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            saltBytes,
            iterations,
            hashAlgorithm,
            keySize);

        // Clear sensitive data from memory directly after use!
        Array.Clear(passwordBytes, 0, passwordBytes.Length);
        Array.Clear(password, 0, password.Length);

        return Convert.ToHexString(hash);
    }

    public bool Validate(char[] password, string hash, string salt)
    {
        if (password is null || password.Length == 0)
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(hash, nameof(hash));
        ArgumentException.ThrowIfNullOrWhiteSpace(salt, nameof(salt));

        var saltBytes = Convert.FromHexString(salt);
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var newHash = Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            saltBytes,
            iterations,
            hashAlgorithm,
            keySize);

        // Clear sensitive data from memory directly after use!

        Array.Clear(passwordBytes, 0, passwordBytes.Length);
        Array.Clear(password, 0, password.Length);

        var isValid = CryptographicOperations.FixedTimeEquals(newHash, Convert.FromHexString(hash));

        return isValid;
    }
}
