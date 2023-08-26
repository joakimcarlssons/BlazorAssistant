using Microsoft.Extensions.DependencyInjection;

namespace BlazorAssistant.StateManagement
{
    public static class Installer
    {
        public static IServiceCollection AddBlazorAssistantStateManagement(this IServiceCollection services)
        {
            services.AddScoped<IStateManager, StateManager>();
            return services;
        }
    }
}
