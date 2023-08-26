using BlazorAssistant.Storage;
using BlazorAssistant.StateManagement;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAssistant
{
    public static class Installer
    {
        public static IServiceCollection AddBlazorAssistant(this IServiceCollection services)
        {
            services.AddBlazorAssistantStorage();
            services.AddBlazorAssistantStateManagement();

            return services;
        }
    }
}
