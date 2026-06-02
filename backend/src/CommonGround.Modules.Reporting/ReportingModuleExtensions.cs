using CommonGround.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Modules.Reporting;

public static class ReportingModuleExtensions
{
    public static IServiceCollection AddReportingModule(this IServiceCollection services)
    {
        services.AddScoped<ScoringEngine>();
        services.AddScoped<IReportingService, ReflectionAssembler>();
        return services;
    }
}
