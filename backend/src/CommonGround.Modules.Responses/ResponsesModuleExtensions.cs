using CommonGround.Modules.Responses.Services;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Modules.Responses;

public static class ResponsesModuleExtensions
{
    public static IServiceCollection AddResponsesModule(this IServiceCollection services)
    {
        services.AddScoped<EfResponseRepository>();
        services.AddScoped<IResponseRepository>(sp => sp.GetRequiredService<EfResponseRepository>());
        services.AddScoped<IResponseReader>(sp => sp.GetRequiredService<EfResponseRepository>());
        return services;
    }
}
