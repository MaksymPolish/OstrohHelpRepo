using Infrastructure.Encryption;
using Xunit;

namespace Tests.Common.Encryption;

// Tests for AES-256-GCM encryption and decryption
// Validates encryption, decryption, tamper detection, and key validation
public class AesGcmEncryptionServiceTests
{
    private readonly IEncryptionService _encryptionService;
    private readonly byte[] _validKey;

    public AesGcmEncryptionServiceTests()
    {
        _encryptionService = new AesGcmEncryptionService();
        // Generate a valid 256-bit (32-byte) key
        _validKey = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(_validKey);
        }
    }

    [Fact]
    public void Encrypt_WithValidInput_ReturnsEncryptedDataWithIvAndAuthTag()
    {
        // Arrange
        var plaintext = "This is a secret message";

        // Act
        var (ciphertext, iv, authTag) = _encryptionService.Encrypt(plaintext, _validKey);

        // Assert
        Assert.NotNull(ciphertext);
        Assert.NotEmpty(ciphertext);
        Assert.NotNull(iv);
        Assert.Equal(12, iv.Length); // 96-bit IV
        Assert.NotNull(authTag);
        Assert.Equal(16, authTag.Length); // 128-bit auth tag
        Assert.NotEqual(plaintext, System.Text.Encoding.UTF8.GetString(ciphertext));
    }

    [Fact]
    public void Encrypt_WithEmptyPlaintext_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _encryptionService.Encrypt(string.Empty, _validKey));
    }

    [Fact]
    public void Encrypt_WithNullPlaintext_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _encryptionService.Encrypt(null, _validKey));
    }

    [Fact]
    public void Encrypt_WithInvalidKeySize_ThrowsArgumentException()
    {
        // Arrange
        var invalidKey = new byte[16]; // 128-bit key instead of 256-bit
        var plaintext = "test";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _encryptionService.Encrypt(plaintext, invalidKey));
    }

    [Fact]
    public void Encrypt_WithNullKey_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _encryptionService.Encrypt("test", null));
    }

    [Fact]
    public void Decrypt_WithValidEncryptedData_ReturnsOriginalPlaintext()
    {
        // Arrange
        var originalPlaintext = "This is a secret message";
        var (ciphertext, iv, authTag) = _encryptionService.Encrypt(originalPlaintext, _validKey);

        // Act
        var decryptedPlaintext = _encryptionService.Decrypt(ciphertext, iv, authTag, _validKey);

        // Assert
        Assert.Equal(originalPlaintext, decryptedPlaintext);
    }

    [Fact]
    public void Decrypt_WithTamperedCiphertext_ThrowsCryptographicException()
    {
        // Arrange
        var plaintext = "Secret message";
        var (ciphertext, iv, authTag) = _encryptionService.Encrypt(plaintext, _validKey);
        
        // Tamper with ciphertext
        ciphertext[0] ^= 0xFF;

        // Act & Assert
        Assert.Throws<System.Security.Cryptography.AuthenticationTagMismatchException>(
            () => _encryptionService.Decrypt(ciphertext, iv, authTag, _validKey));
    }

    [Fact]
    public void Decrypt_WithTamperedAuthTag_ThrowsCryptographicException()
    {
        // Arrange
        var plaintext = "Secret message";
        var (ciphertext, iv, authTag) = _encryptionService.Encrypt(plaintext, _validKey);
        
        // Tamper with auth tag
        authTag[0] ^= 0xFF;

        // Act & Assert
        Assert.Throws<System.Security.Cryptography.AuthenticationTagMismatchException>(
            () => _encryptionService.Decrypt(ciphertext, iv, authTag, _validKey));
    }

    [Fact]
    public void Decrypt_WithWrongKey_ThrowsCryptographicException()
    {
        // Arrange
        var plaintext = "Secret message";
        var (ciphertext, iv, authTag) = _encryptionService.Encrypt(plaintext, _validKey);
        
        // Generate different key
        var wrongKey = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(wrongKey);
        }

        // Act & Assert
        Assert.Throws<System.Security.Cryptography.AuthenticationTagMismatchException>(
            () => _encryptionService.Decrypt(ciphertext, iv, authTag, wrongKey));
    }

    [Fact]
    public void Decrypt_WithInvalidIvSize_ThrowsArgumentException()
    {
        // Arrange
        var plaintext = "Secret";
        var (ciphertext, _, authTag) = _encryptionService.Encrypt(plaintext, _validKey);
        var invalidIv = new byte[16]; // 128-bit IV instead of 96-bit

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _encryptionService.Decrypt(ciphertext, invalidIv, authTag, _validKey));
    }

    [Fact]
    public void Decrypt_WithInvalidAuthTagSize_ThrowsArgumentException()
    {
        // Arrange
        var plaintext = "Secret";
        var (ciphertext, iv, _) = _encryptionService.Encrypt(plaintext, _validKey);
        var invalidAuthTag = new byte[12]; // 96-bit tag instead of 128-bit

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _encryptionService.Decrypt(ciphertext, iv, invalidAuthTag, _validKey));
    }

    [Fact]
    public void Encrypt_SamePlaintextProducesDifferentCiphertexts()
    {
        // Arrange
        var plaintext = "Test message";

        // Act
        var (ciphertext1, iv1, _) = _encryptionService.Encrypt(plaintext, _validKey);
        var (ciphertext2, iv2, _) = _encryptionService.Encrypt(plaintext, _validKey);

        // Assert
        // IVs should be different (random)
        Assert.NotEqual(iv1, iv2);
        // Ciphertexts should be different because IVs are different
        Assert.NotEqual(ciphertext1, ciphertext2);
    }

    [Fact]
    public void EncryptDecrypt_WithUnicodeText_PreservesContent()
    {
        // Arrange
        var unicodeText = "Зашифровано 🔐 повідомлення!";

        // Act
        var (ciphertext, iv, authTag) = _encryptionService.Encrypt(unicodeText, _validKey);
        var decrypted = _encryptionService.Decrypt(ciphertext, iv, authTag, _validKey);

        // Assert
        Assert.Equal(unicodeText, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_WithLargeText_PreservesContent()
    {
        // Arrange
        var largeText = string.Concat(Enumerable.Range(0, 10000).Select(i => $"Line {i}: This is a test message\n"));

        // Act
        var (ciphertext, iv, authTag) = _encryptionService.Encrypt(largeText, _validKey);
        var decrypted = _encryptionService.Decrypt(ciphertext, iv, authTag, _validKey);

        // Assert
        Assert.Equal(largeText, decrypted);
    }
}
