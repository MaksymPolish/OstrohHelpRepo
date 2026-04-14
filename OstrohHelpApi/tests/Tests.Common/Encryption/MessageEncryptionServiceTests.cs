using Application.Common.Interfaces.Services;
using Infrastructure.Encryption;
using Xunit;

namespace Tests.Common.Encryption;

// Tests for message encryption and decryption using AES-256-GCM
// Validates encryption of plaintext messages and decryption with proper keys
public class MessageEncryptionServiceTests
{
    private readonly IMessageEncryptionService _messageEncryptionService;
    private readonly string _masterKeyBase64;
    private readonly Guid _consultationId;

    public MessageEncryptionServiceTests()
    {
        _masterKeyBase64 = "XtLurkNiKAseW287LjZzbr4uzG39deKNrcMfEuxbg0o=";
        _consultationId = Guid.NewGuid();

        var encryptionService = new AesGcmEncryptionService();
        var keyDerivationService = new HkdfKeyDerivationService();
        _messageEncryptionService = new MessageEncryptionService(
            encryptionService,
            keyDerivationService,
            _masterKeyBase64);
    }

    [Fact]
    public void EncryptMessage_WithValidInput_ReturnsEncryptedMessageData()
    {
        // Arrange
        var plaintext = "This is a secret message";

        // Act
        var encrypted = _messageEncryptionService.EncryptMessage(plaintext, _consultationId);

        // Assert
        Assert.NotNull(encrypted);
        Assert.NotNull(encrypted.EncryptedContent);
        Assert.NotNull(encrypted.Iv);
        Assert.NotNull(encrypted.AuthTag);
        Assert.NotEmpty(encrypted.EncryptedContent);
        Assert.Equal(12, encrypted.Iv.Length);
        Assert.Equal(16, encrypted.AuthTag.Length);
    }

    [Fact]
    public void EncryptMessage_WithEmptyPlaintext_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _messageEncryptionService.EncryptMessage(string.Empty, _consultationId));
    }

    [Fact]
    public void EncryptMessage_WithNullPlaintext_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _messageEncryptionService.EncryptMessage(null, _consultationId));
    }

    [Fact]
    public void EncryptMessage_WithEmptyConsultationId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _messageEncryptionService.EncryptMessage("message", Guid.Empty));
    }

    [Fact]
    public void DecryptMessage_WithValidEncryptedData_ReturnsOriginalMessage()
    {
        // Arrange
        var originalMessage = "Secret consultation message";
        var encrypted = _messageEncryptionService.EncryptMessage(originalMessage, _consultationId);

        // Act
        var decrypted = _messageEncryptionService.DecryptMessage(encrypted, _consultationId);

        // Assert
        Assert.Equal(originalMessage, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_Roundtrip_WithVariousMessages_PreservesContent()
    {
        // Arrange
        var testMessages = new[]
        {
            "Simple message",
            "Message with special chars: !@#$%^&*()",
            "Message with newlines\nand\ttabs",
            "Українське повідомлення 🔐",
            string.Concat(Enumerable.Range(0, 1000).Select(i => $"Line {i}: "))
        };

        // Act & Assert
        foreach (var message in testMessages)
        {
            var encrypted = _messageEncryptionService.EncryptMessage(message, _consultationId);
            var decrypted = _messageEncryptionService.DecryptMessage(encrypted, _consultationId);
            Assert.Equal(message, decrypted);
        }
    }

    [Fact]
    public void EncryptMessage_SameMessageDifferentEncryptions_DueToRandomIv()
    {
        // Arrange
        var message = "Test message";

        // Act
        var encrypted1 = _messageEncryptionService.EncryptMessage(message, _consultationId);
        var encrypted2 = _messageEncryptionService.EncryptMessage(message, _consultationId);

        // Assert
        Assert.NotEqual(encrypted1.Iv, encrypted2.Iv); // Different random IVs
        Assert.NotEqual(encrypted1.EncryptedContent, encrypted2.EncryptedContent); // Different ciphertexts
    }

    [Fact]
    public void EncryptMessage_DifferentConsultations_ProduceDifferentCiphertexts()
    {
        // Arrange
        var message = "Test message";
        var consultationId2 = Guid.NewGuid();

        // Act
        var encrypted1 = _messageEncryptionService.EncryptMessage(message, _consultationId);
        var encrypted2 = _messageEncryptionService.EncryptMessage(message, consultationId2);

        // Assert
        // Since different consultation IDs derive different keys, ciphertexts should differ
        // even with the same IV (which shouldn't happen due to randomness anyway)
        Assert.NotEqual(encrypted1.EncryptedContent, encrypted2.EncryptedContent);
    }

    [Fact]
    public void DecryptMessage_WithNullEncryptedData_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _messageEncryptionService.DecryptMessage(null, _consultationId));
    }

    [Fact]
    public void DecryptMessage_WithEmptyConsultationId_ThrowsArgumentException()
    {
        // Arrange
        var encrypted = _messageEncryptionService.EncryptMessage("message", _consultationId);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _messageEncryptionService.DecryptMessage(encrypted, Guid.Empty));
    }

    [Fact]
    public void DecryptMessage_WithWrongConsultationId_ThrowsCryptographicException()
    {
        // Arrange
        var message = "Secret message";
        var encrypted = _messageEncryptionService.EncryptMessage(message, _consultationId);
        var wrongConsultationId = Guid.NewGuid();

        // Act & Assert
        // Different consultation ID = different derived key = auth tag validation fails
        Assert.Throws<System.Security.Cryptography.AuthenticationTagMismatchException>(
            () => _messageEncryptionService.DecryptMessage(encrypted, wrongConsultationId));
    }

    [Fact]
    public void EncryptDecrypt_ConsultationKeyIsDeterministic()
    {
        // Arrange
        var message = "Deterministic key test";

        // Encrypt with consultation ID
        var encrypted = _messageEncryptionService.EncryptMessage(message, _consultationId);

        // Create new service instance (simulates different request, same machine)
        var newService = new MessageEncryptionService(
            new AesGcmEncryptionService(),
            new HkdfKeyDerivationService(),
            _masterKeyBase64);

        // Act
        var decrypted = newService.DecryptMessage(encrypted, _consultationId);

        // Assert
        Assert.Equal(message, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_WithMaximumMessageLength_Succeeds()
    {
        // Arrange
        var largeMessage = string.Concat(Enumerable.Range(0, 100000).Select(i => "X"));

        // Act
        var encrypted = _messageEncryptionService.EncryptMessage(largeMessage, _consultationId);
        var decrypted = _messageEncryptionService.DecryptMessage(encrypted, _consultationId);

        // Assert
        Assert.Equal(largeMessage, decrypted);
    }

    [Fact]
    public void EncryptDecrypt_WithSpecialUnicodeCharacters_PreservesContent()
    {
        // Arrange
        var unicodeMessages = new[]
        {
            "🔐 Encrypted message 🔐",
            "Emoji: 😀 😃 😄 😁",
            "Arabic: مرحبا",
            "Chinese: 你好",
            "Symbols: ™ © ® € £ ¥",
            "Mathematical: ∑ ∫ √ ∞ ≠ ≤ ≥"
        };

        // Act & Assert
        foreach (var message in unicodeMessages)
        {
            var encrypted = _messageEncryptionService.EncryptMessage(message, _consultationId);
            var decrypted = _messageEncryptionService.DecryptMessage(encrypted, _consultationId);
            Assert.Equal(message, decrypted);
        }
    }
}
