using Microsoft.Extensions.DependencyInjection;
using Core.Translation.Abstraction;
using Core.Translation.AmazonTranslate;

namespace Core.Translation.AmazonTranslate.DependencyInjection;

public static class ServiceCollectionAmazonTranslateLocalizationExtension
{
    public static IServiceCollection AddAmazonTranslation(
        this IServiceCollection services,
        AmazonTranslateConfiguration configuration
    )
    {
        services.AddTransient<ITranslationService, AmazonTranslateLocalizationManager>(
            _ => new AmazonTranslateLocalizationManager(configuration)
        );
        return services;
    }
}
