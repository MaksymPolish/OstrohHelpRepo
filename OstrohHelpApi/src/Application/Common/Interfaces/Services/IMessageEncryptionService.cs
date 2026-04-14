namespace Application.Common.Interfaces.Services;

/// <summary>
/// Structured representation of encrypted message data for storage
/// </summary>
public record EncryptedMessageData(
    byte[] EncryptedContent,
    byte[] Iv,
    byte[] AuthTag);

/// <summary>
/// Service for encrypting messages during storage
/// Handles the complete encryption workflow for message content
/// </summary>
public interface IMessageEncryptionService
{
    /// <summary>
    /// Encrypts a message and returns structured encryption data
    /// </summary>
    /// <param name="plaintext">Message content to encrypt</param>
    /// <param name="consultationId">Consultation identifier (used for key derivation)</param>
    /// <returns>Encrypted message data with IV and auth tag</returns>
    EncryptedMessageData EncryptMessage(string plaintext, Guid consultationId);

    /// <summary>
    /// Decrypts a message using derived consultation key
    /// </summary>
    /// <param name="encryptedData">Encrypted message data</param>
    /// <param name="consultationId">Consultation identifier (for key derivation)</param>
    /// <returns>Decrypted plaintext</returns>
    string DecryptMessage(EncryptedMessageData encryptedData, Guid consultationId);
}
