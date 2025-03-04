using System.Security.Cryptography;

namespace Salter.Core.Encryption;

/// <summary>
/// The <see cref="KeyIvEncryptor"/> class provides methods for encrypting and decrypting data using symmetric encryption algorithms.
/// It utilizes a <see cref="KeyManager"/> to manage encryption keys and initialization vectors.
/// </summary>
public class KeyIvEncryptor(KeyManager keyManager, Func<SymmetricAlgorithm> algorithmFactory) : IEncryptor
{
    private readonly KeyManager keyManager = keyManager;
    private readonly Func<SymmetricAlgorithm> algorithmFactory = algorithmFactory;

    public byte[] Decrypt(byte[] encryptedData)
    {
        try
        {
            using var algorithm = algorithmFactory();
            (algorithm.Key, algorithm.IV) = keyManager.Load();

            using var decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV);
            using var ms = new MemoryStream(encryptedData);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

            return ReadFully(cs);
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Decryption failed.", ex);
        }
    }

    public async Task<byte[]> DecryptAsync(byte[] encryptedData)
    {
        try
        {
            using var algorithm = algorithmFactory();
            (algorithm.Key, algorithm.IV) = await keyManager.LoadAsync();

            using var decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV);
            using var ms = new MemoryStream(encryptedData);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);

            return await ReadFullyAsync(cs);
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Decryption failed.", ex);
        }
    }

    public byte[] Encrypt(byte[] data)
    {
        try
        {
            using var algorithm = algorithmFactory();
            GenerateKeyAndIv(algorithm);
            using var encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
            }

            keyManager.Save(algorithm.Key, algorithm.IV);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Encryption failed.", ex);
        }
        finally
        {
            Array.Clear(data, 0, data.Length);
        }
    }

    public async Task<byte[]> EncryptAsync(byte[] data)
    {
        try
        {
            using var algorithm = algorithmFactory();
            GenerateKeyAndIv(algorithm);
            using var encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                await cs.WriteAsync(data);
            }

            await keyManager.SaveAsync(algorithm.Key, algorithm.IV);
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Encryption failed.", ex);
        }
        finally
        {
            Array.Clear(data, 0, data.Length);
        }
    }

    private static void GenerateKeyAndIv(SymmetricAlgorithm algorithm)
    {
        algorithm.GenerateKey();
        algorithm.GenerateIV();
    }

    private static byte[] ReadFully(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    private static async Task<byte[]> ReadFullyAsync(Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}
