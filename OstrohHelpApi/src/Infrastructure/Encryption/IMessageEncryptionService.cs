using Application.Common.Interfaces.Services;

namespace Infrastructure.Encryption;

/// <summary>
/// Implementation of message encryption service using HKDF-derived keys
/// </summary>
public class MessageEncryptionService : IMessageEncryptionService
{
    private readonly IEncryptionService _encryptionService;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly byte[] _masterKey;

    public MessageEncryptionService(
        IEncryptionService encryptionService,
        IKeyDerivationService keyDerivationService,
        string masterKeyBase64)
    {
        _encryptionService = encryptionService;
        _keyDerivationService = keyDerivationService;

        if (string.IsNullOrEmpty(masterKeyBase64))
            throw new ArgumentException("Master key cannot be null or empty", nameof(masterKeyBase64));

        try
        {
            _masterKey = Convert.FromBase64String(masterKeyBase64);
            if (_masterKey.Length < 32)
                throw new ArgumentException("Master key must be at least 32 bytes (256 bits)", nameof(masterKeyBase64));
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Master key must be valid base64 encoded", nameof(masterKeyBase64), ex);
        }
    }

    public EncryptedMessageData EncryptMessage(string plaintext, Guid consultationId)
    {
        if (string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Message content cannot be null or empty", nameof(plaintext));

        if (consultationId == Guid.Empty)
            throw new ArgumentException("Consultation ID cannot be empty", nameof(consultationId));

        // Derive consultation-specific key
        var consultationKey = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId);

        // Encrypt message content
        var (ciphertext, iv, authTag) = _encryptionService.Encrypt(plaintext, consultationKey);

        return new EncryptedMessageData(ciphertext, iv, authTag);
    }

    public string DecryptMessage(EncryptedMessageData encryptedData, Guid consultationId)
    {
        if (encryptedData == null)
            throw new ArgumentNullException(nameof(encryptedData));

        if (consultationId == Guid.Empty)
            throw new ArgumentException("Consultation ID cannot be empty", nameof(consultationId));

        // Derive consultation-specific key
        var consultationKey = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId);

        // Decrypt message content
        return _encryptionService.Decrypt(encryptedData.EncryptedContent, encryptedData.Iv, encryptedData.AuthTag, consultationKey);
    }
}
