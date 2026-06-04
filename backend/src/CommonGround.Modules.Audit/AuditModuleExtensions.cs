using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Modules.Audit;

public static class AuditModuleExtensions
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services)
    {
        return services;
    }
}
