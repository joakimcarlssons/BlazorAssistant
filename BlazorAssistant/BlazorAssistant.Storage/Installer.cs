using BlazorAssistant.Storage.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAssistant.Storage
{
    public static class Installer
    {
        public static IServiceCollection AddBlazorAssistantStorage(this IServiceCollection services)
        {
            services.AddScoped<ILocalStorageService, LocalStorageService>();
            return services;
        }
    }
}
