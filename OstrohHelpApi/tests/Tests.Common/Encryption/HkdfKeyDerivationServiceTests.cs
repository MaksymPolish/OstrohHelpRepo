using Infrastructure.Encryption;
using Xunit;

namespace Tests.Common.Encryption;

// Tests for HKDF-SHA256 key derivation
// Validates deterministic key generation and proper validation of inputs
public class HkdfKeyDerivationServiceTests
{
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly byte[] _masterKey;

    public HkdfKeyDerivationServiceTests()
    {
        _keyDerivationService = new HkdfKeyDerivationService();
        
        // Use consistent master key for tests
        _masterKey = System.Convert.FromBase64String("XtLurkNiKAseW287LjZzbr4uzG39deKNrcMfEuxbg0o=");
    }

    [Fact]
    public void DeriveKeyForConsultation_WithValidInput_Returns32ByteKey()
    {
        // Arrange
        var consultationId = Guid.NewGuid();

        // Act
        var derivedKey = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId);

        // Assert
        Assert.NotNull(derivedKey);
        Assert.Equal(32, derivedKey.Length); // 256-bit key
    }

    [Fact]
    public void DeriveKeyForConsultation_WithNullMasterKey_ThrowsArgumentException()
    {
        // Arrange
        var consultationId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _keyDerivationService.DeriveKeyForConsultation(null, consultationId));
    }

    [Fact]
    public void DeriveKeyForConsultation_WithEmptyMasterKey_ThrowsArgumentException()
    {
        // Arrange
        var consultationId = Guid.NewGuid();
        var emptyKey = new byte[0];

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _keyDerivationService.DeriveKeyForConsultation(emptyKey, consultationId));
    }

    [Fact]
    public void DeriveKeyForConsultation_WithSmallMasterKey_ThrowsArgumentException()
    {
        // Arrange
        var consultationId = Guid.NewGuid();
        var smallKey = new byte[16]; // 128-bit key, less than minimum 256-bit

        // Act & Assert  
        Assert.Throws<ArgumentException>(
            () => _keyDerivationService.DeriveKeyForConsultation(smallKey, consultationId));
    }

    [Fact]
    public void DeriveKeyForConsultation_SameConsultationIdProducesSameKey()
    {
        // Arrange
        var consultationId = Guid.NewGuid();

        // Act
        var key1 = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId);
        var key2 = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId);

        // Assert
        Assert.Equal(key1, key2); // Deterministic HKDF
    }

    [Fact]
    public void DeriveKeyForConsultation_DifferentConsultationIdsProduceDifferentKeys()
    {
        // Arrange
        var consultationId1 = Guid.NewGuid();
        var consultationId2 = Guid.NewGuid();

        // Act
        var key1 = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId1);
        var key2 = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId2);

        // Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void DeriveKeyForConsultation_DifferentMasterKeysProduceDifferentKeys()
    {
        // Arrange
        var consultationId = Guid.NewGuid();
        
        var masterKey2 = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(masterKey2);
        }

        // Act
        var key1 = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId);
        var key2 = _keyDerivationService.DeriveKeyForConsultation(masterKey2, consultationId);

        // Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void DeriveKeyForConsultation_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => _keyDerivationService.DeriveKeyForConsultation(_masterKey, emptyGuid));
    }

    [Fact]
    public void DeriveKeyForConsultation_WithLargeMasterKey_Returns32ByteKey()
    {
        // Arrange
        var consultationId = Guid.NewGuid();
        var largeMasterKey = new byte[64]; // Larger than required
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(largeMasterKey);
        }

        // Act
        var derivedKey = _keyDerivationService.DeriveKeyForConsultation(largeMasterKey, consultationId);

        // Assert
        Assert.NotNull(derivedKey);
        Assert.Equal(32, derivedKey.Length);
    }

    [Fact]
    public void DeriveKeyForConsultation_MultipleCallsAreConsistent()
    {
        // Arrange
        var consultationIds = Enumerable.Range(0, 100)
            .Select(_ => Guid.NewGuid())
            .ToList();

        // Act
        var firstRun = consultationIds
            .Select(id => _keyDerivationService.DeriveKeyForConsultation(_masterKey, id))
            .ToList();

        var secondRun = consultationIds
            .Select(id => _keyDerivationService.DeriveKeyForConsultation(_masterKey, id))
            .ToList();

        // Assert
        for (int i = 0; i < consultationIds.Count; i++)
        {
            Assert.Equal(firstRun[i], secondRun[i]);
        }
    }

    [Fact]
    public void DeriveKeyForConsultation_ProducesNonZeroKey()
    {
        // Arrange
        var consultationId = Guid.NewGuid();

        // Act
        var derivedKey = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId);

        // Assert
        Assert.NotEqual(new byte[32], derivedKey); // Not all zeros
    }

    [Fact]
    public void DeriveKeyForConsultation_ProducesHighEntropyKey()
    {
        // Arrange
        var consultationId = Guid.NewGuid();

        // Act
        var derivedKey = _keyDerivationService.DeriveKeyForConsultation(_masterKey, consultationId);

        // Assert
        // Check that key has various byte values (basic entropy check)
        var uniqueBytes = derivedKey.Distinct().Count();
        Assert.True(uniqueBytes > 20, "Key should have good entropy"); // Should have diverse byte values
    }
}
