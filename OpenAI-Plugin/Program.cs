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

        await kernel.ImportPluginFromOpenApiAsync(
            pluginName: "AdventureWorks",
            uri: new Uri("https://localhost:7040/swagger/v1/swagger.json")
            );

        var settings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = await kernel.InvokePromptAsync("How many total customers we do have?", new KernelArguments(settings));

        AnsiConsole.MarkupLine($"[green]Result:[/] {result}");
    }
}