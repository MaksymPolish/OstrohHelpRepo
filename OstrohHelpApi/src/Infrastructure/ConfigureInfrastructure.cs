using Infrastructure.Persistence;
using Infrastructure.Encryption;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DotNetEnv;

namespace Infrastructure;

public static class ConfigureInfrastructure
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddEncryption(configuration);
    }

    private static void AddEncryption(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();
        services.AddSingleton<IKeyDerivationService, HkdfKeyDerivationService>();
        
        // Get master key from environment
        var masterKeyBase64 = DotNetEnv.Env.GetString("ENCRYPTION_MASTER_KEY")
            ?? configuration["Encryption:MasterKey"]
            ?? throw new InvalidOperationException(
                "ENCRYPTION_MASTER_KEY environment variable or Encryption:MasterKey configuration is required");
        
        // Register message encryption service
        services.AddSingleton<IMessageEncryptionService>(provider =>
        {
            var encryptionService = provider.GetRequiredService<IEncryptionService>();
            var keyDerivationService = provider.GetRequiredService<IKeyDerivationService>();
            return new MessageEncryptionService(encryptionService, keyDerivationService, masterKeyBase64);
        });
        
        // Register consultation key provider with master key from appsettings or environment
        services.AddSingleton<IConsultationKeyProvider>(provider =>
        {
            var keyDerivationService = provider.GetRequiredService<IKeyDerivationService>();
            return new ConsultationKeyProvider(keyDerivationService, masterKeyBase64);
        });
    }
}