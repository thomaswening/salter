namespace Salter.Core.Encryption;

internal interface IEncryptor
{
    byte[] Decrypt(byte[] data);
    Task<byte[]> DecryptAsync(byte[] data);
    byte[] Encrypt(byte[] data);
    Task<byte[]> EncryptAsync(byte[] data);
}