using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Spectre.Console;

class Program
{
    static async Task Main()
    {
        var builder = Kernel.CreateBuilder();
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables().Build();

        builder.Services.Configure<Settings>(config);
        builder.Services.AddHttpClient();

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: config["AzureOpenAI:Deployment"]!,
            endpoint: config["AzureOpenAI:Endpoint"]!,
            apiKey: config["AzureOpenAI:ApiKey"]!
        );

        var kernel = builder.Build();

        // Navtive Plugin registration
        kernel.Plugins.AddFromObject(new HotelPlugin());
        var settings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = kernel.InvokePromptAsync("I want hotel for 2 nights in Noida, can you share rating and amount?",arguments:new KernelArguments(settings));

        AnsiConsole.MarkupLine($"[green]Result:[/] {await result}");
    }
}