using CommonGround.Modules.Privacy.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Modules.Privacy;

public static class PrivacyModuleExtensions
{
    public static IServiceCollection AddPrivacyModule(this IServiceCollection services, byte[] hmacKey)
    {
        services.AddSingleton(new TokenService(hmacKey));
        return services;
    }
}
