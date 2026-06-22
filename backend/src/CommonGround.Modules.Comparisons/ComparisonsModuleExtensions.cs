using CommonGround.Modules.Comparisons.Services;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Modules.Comparisons;

public static class ComparisonsModuleExtensions
{
    public static IServiceCollection AddComparisonsModule(this IServiceCollection services)
    {
        services.AddScoped<InviteTokenService>();
        services.AddScoped<ComparisonService>();
        services.AddScoped<IComparisonService>(sp => sp.GetRequiredService<ComparisonService>());
        return services;
    }
}
