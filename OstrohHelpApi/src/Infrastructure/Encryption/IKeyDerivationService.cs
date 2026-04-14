using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Encryption;

/// <summary>
/// Service for deriving consultation-specific encryption keys from a master key using HKDF-SHA256
/// Provides deterministic key generation: same consultation ID always produces the same key
/// </summary>
public interface IKeyDerivationService
{
    /// <summary>
    /// Derives a 256-bit encryption key from master key and consultation ID using HKDF-SHA256
    /// </summary>
    /// <param name="masterKey">Master key (minimum 32 bytes)</param>
    /// <param name="consultationId">Consultation identifier (used as salt)</param>
    /// <returns>256-bit derived key</returns>
    byte[] DeriveKeyForConsultation(byte[] masterKey, Guid consultationId);
}

/// <summary>
/// HKDF-SHA256 based key derivation implementation
/// Uses NIST recommended parameters for key derivation
/// </summary>
public class HkdfKeyDerivationService : IKeyDerivationService
{
    private const int DerivedKeyLength = 32; // 256 bits

    public byte[] DeriveKeyForConsultation(byte[] masterKey, Guid consultationId)
    {
        if (masterKey == null || masterKey.Length == 0)
            throw new ArgumentException("Master key cannot be null or empty", nameof(masterKey));

        if (masterKey.Length < 32)
            throw new ArgumentException("Master key must be at least 32 bytes (256 bits)", nameof(masterKey));

        if (consultationId == Guid.Empty)
            throw new ArgumentException("Consultation ID cannot be empty", nameof(consultationId));

        // Use consultation ID as salt for deterministic derivation
        // Same consultation ID will always produce the same key
        var salt = Encoding.UTF8.GetBytes(consultationId.ToString("N")); // "N" format removes hyphens
        
        // HKDF-SHA256: Extract and Expand phases
        using (var hmac = new HMACSHA256(salt))
        {
            // Extract phase: PRK = HMAC-SHA256(salt, masterKey)
            var prk = hmac.ComputeHash(masterKey);

            // Expand phase: Generate output key material (OKM)
            // Use info string to bind key to specific purpose (message encryption)
            var info = Encoding.UTF8.GetBytes("OstrohHelp-MessageEncryption");
            var okm = HkdfExpand(prk, info, DerivedKeyLength);

            return okm;
        }
    }

    /// <summary>
    /// HKDF-SHA256 Expand function (RFC 5869)
    /// </summary>
    private static byte[] HkdfExpand(byte[] prk, byte[] info, int length)
    {
        var n = (int)Math.Ceiling((double)length / 32); // SHA256 hash length is 32 bytes
        var t = new byte[0];
        var okm = new List<byte>();

        for (int i = 1; i <= n; i++)
        {
            using (var hmac = new HMACSHA256(prk))
            {
                var ti = t.Concat(info).Concat(new[] { (byte)i }).ToArray();
                t = hmac.ComputeHash(ti);
                okm.AddRange(t);
            }
        }

        return okm.Take(length).ToArray();
    }
}
