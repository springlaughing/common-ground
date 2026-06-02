using CommonGround.Modules.Questionnaires.Services;
using CommonGround.SharedKernel.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CommonGround.Modules.Questionnaires;

public static class QuestionnairesModuleExtensions
{
    public static IServiceCollection AddQuestionnairesModule(this IServiceCollection services)
    {
        services.AddScoped<IQuestionnaireReader, EfQuestionnaireReader>();
        return services;
    }
}
