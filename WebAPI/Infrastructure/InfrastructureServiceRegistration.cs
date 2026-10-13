using Application.Services.ImageService;
using Infrastructure.Adapters.ImageService;
using Application.Services.Time;
using Infrastructure.Adapters.Time;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ImageServiceBase, CloudinaryImageServiceAdapter>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
