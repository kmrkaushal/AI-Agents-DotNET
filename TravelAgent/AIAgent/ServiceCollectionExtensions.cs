using AIAgent.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AIAgent
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAIAgent(
            this IServiceCollection services,
            string deployment,
            string endpoint,
            string apiKey)
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders(); 
                builder.AddConsole();
                builder.AddDebug();

                builder.SetMinimumLevel(LogLevel.Information); 
            });

            services.AddSingleton<TravelPlugin>();
            services.AddSingleton<IFunctionInvocationFilter, LoggingFilter>();

            services.AddScoped<Kernel>(sp =>
            {
                var builder = Kernel.CreateBuilder();

                // Chat Completion
                builder.AddAzureOpenAIChatCompletion(
                    deploymentName: deployment,
                    endpoint: endpoint,
                    apiKey: apiKey);

                // Logging
                builder.Services.AddLogging();

                var kernel = builder.Build();
                
                //Register plugin
                kernel.ImportPluginFromObject(sp.GetRequiredService<TravelPlugin>(),"Travel");

                return kernel;
            });

            return services;
        }
    }
}