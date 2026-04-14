namespace Infrastructure.Encryption;

/// <summary>
/// Service for encrypting and decrypting messages using AES-256-GCM
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts plaintext using AES-256-GCM with the provided key
    /// </summary>
    /// <param name="plaintext">Text to encrypt</param>
    /// <param name="key">256-bit encryption key</param>
    /// <returns>Tuple of (ciphertext, iv, authenticationTag)</returns>
    (byte[] ciphertext, byte[] iv, byte[] authTag) Encrypt(string plaintext, byte[] key);

    /// <summary>
    /// Decrypts ciphertext using AES-256-GCM with the provided key
    /// </summary>
    /// <param name="ciphertext">Encrypted data</param>
    /// <param name="iv">Initialization vector (12 bytes)</param>
    /// <param name="authTag">Authentication tag (16 bytes)</param>
    /// <param name="key">256-bit encryption key</param>
    /// <returns>Decrypted plaintext string</returns>
    /// <exception cref="System.Security.Cryptography.CryptographicException">If authentication fails</exception>
    string Decrypt(byte[] ciphertext, byte[] iv, byte[] authTag, byte[] key);
}
