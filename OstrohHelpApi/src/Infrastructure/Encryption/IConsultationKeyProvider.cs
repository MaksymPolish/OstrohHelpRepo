namespace Infrastructure.Encryption;

/// <summary>
/// Service for managing encryption keys for consultations
/// Provides derived keys to ChatHub for client transmission
/// </summary>
public interface IConsultationKeyProvider
{
    /// <summary>
    /// Gets the derived encryption key for a consultation
    /// Key is generated deterministically from consultation ID and master key
    /// </summary>
    /// <param name="consultationId">Consultation unique identifier</param>
    /// <returns>256-bit derived key as byte array</returns>
    byte[] GetConsultationKey(Guid consultationId);
}

/// <summary>
/// Implementation of consultation key provider using HKDF key derivation
/// </summary>
public class ConsultationKeyProvider : IConsultationKeyProvider
{
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly byte[] _masterKey;

    public ConsultationKeyProvider(IKeyDerivationService keyDerivationService, string masterKeyBase64)
    {
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

    public byte[] GetConsultationKey(Guid consultationId)
    {
        if (consultationId == Guid.Empty)
            throw new ArgumentException("Consultation ID cannot be empty", nameof(consultationId));

        return _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId);
    }
}
