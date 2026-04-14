using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Encryption;

/// <summary>
/// AES-256-GCM encryption service implementation
/// Uses 256-bit keys, 96-bit (12-byte) IVs, and 128-bit (16-byte) authentication tags
/// </summary>
public class AesGcmEncryptionService : IEncryptionService
{
    private const int KeySizeBytes = 32;      // 256 bits
    private const int IvSizeBytes = 12;       // 96 bits (recommended for GCM)
    private const int TagSizeBytes = 16;      // 128 bits

    public (byte[] ciphertext, byte[] iv, byte[] authTag) Encrypt(string plaintext, byte[] key)
    {
        if (string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Plaintext cannot be null or empty", nameof(plaintext));

        if (key == null || key.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be exactly {KeySizeBytes} bytes (256 bits)", nameof(key));

        // Generate random IV
        var iv = new byte[IvSizeBytes];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        // Convert plaintext to bytes
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

        // Prepare output buffers
        var ciphertext = new byte[plaintextBytes.Length];
        var authTag = new byte[TagSizeBytes];

        // Perform encryption
        using (var aesGcm = new AesGcm(key, TagSizeBytes))
        {
            aesGcm.Encrypt(iv, plaintextBytes, ciphertext, authTag);
        }

        return (ciphertext, iv, authTag);
    }

    public string Decrypt(byte[] ciphertext, byte[] iv, byte[] authTag, byte[] key)
    {
        if (ciphertext == null || ciphertext.Length == 0)
            throw new ArgumentException("Ciphertext cannot be null or empty", nameof(ciphertext));

        if (iv == null || iv.Length != IvSizeBytes)
            throw new ArgumentException($"IV must be exactly {IvSizeBytes} bytes (96 bits)", nameof(iv));

        if (authTag == null || authTag.Length != TagSizeBytes)
            throw new ArgumentException($"Auth tag must be exactly {TagSizeBytes} bytes (128 bits)", nameof(authTag));

        if (key == null || key.Length != KeySizeBytes)
            throw new ArgumentException($"Key must be exactly {KeySizeBytes} bytes (256 bits)", nameof(key));

        // Prepare plaintext buffer
        var plaintext = new byte[ciphertext.Length];

        // Perform decryption (will throw if authentication fails)
        using (var aesGcm = new AesGcm(key, TagSizeBytes))
        {
            aesGcm.Decrypt(iv, ciphertext, authTag, plaintext);
        }

        // Convert decrypted bytes to UTF-8 string
        return Encoding.UTF8.GetString(plaintext);
    }
}
