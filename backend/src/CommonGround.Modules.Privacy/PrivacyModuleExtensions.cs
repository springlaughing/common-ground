using CommonGround.Modules.Privacy.Services;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Modules.Privacy;

public static class PrivacyModuleExtensions
{
    public static IServiceCollection AddPrivacyModule(this IServiceCollection services, byte[] hmacKey)
    {
        services.AddSingleton(new TokenService(hmacKey));
        // Expose the same instance through the SharedKernel abstraction so feature
        // modules can hash/generate tokens without referencing the Privacy module.
        services.AddSingleton<ITokenService>(sp => sp.GetRequiredService<TokenService>());
        return services;
    }
}
