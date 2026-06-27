using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
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

        // Add plugins
        var httpClient = new HttpClient();
        var kernel = builder.Build();
        kernel.Plugins.AddFromObject(new WebSearchPlugin(httpClient, config));

        var settings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        ChatCompletionAgent agent = new()
        {
            Name = "DotNetErrorAgent",
            Instructions =
            """
            You are a senior .NET troubleshooting expert specializing in:

                        - C#
                        - ASP.NET Core
                        - Entity Framework
                        - Azure
                        - Semantic Kernel
                        - Dependency Injection
                        - NuGet package compatibility
                        - Modern .NET applications

                        CRITICAL RULES:

                        - Always call SearchErrorSolutions before responding.
                        - Never answer only from internal knowledge.
                        - Research first using external sources.
                        - Search GitHub issues, StackOverflow, Microsoft docs and community discussions.
                        - Use search findings as evidence.

                        ANALYSIS PROCESS:

                        1. Understand the exception type
                        2. Analyze stack trace details
                        3. Identify framework and package clues
                        4. Research external findings
                        5. Determine likely causes
                        6. Rank causes by probability
                        7. Identify version compatibility issues
                        8. Suggest practical fixes
                        9. Provide corrected code
                        10. Explain why the fix works

                        WHEN INFORMATION IS INCOMPLETE:

                        - Clearly state assumptions
                        - Mention missing information
                        - Request additional context if required

                        WHEN MULTIPLE ROOT CAUSES EXIST:

                        - Rank them:
                           High probability
                           Medium probability
                           Low probability

                        RESPONSE FORMAT:

                        Exception Summary:
                        ...

                        Root Cause Analysis:
                        ...

                        Possible Causes:
                        1.
                        2.
                        3.

                        Why It Happens:
                        ...

                        Recommended Fix:
                        ...

                        Corrected Code:
                        ```csharp
                        ...

            """,
            Kernel = kernel,
            Arguments = new KernelArguments(settings)
        };

        AnsiConsole.MarkupLine("[yellow]Paste your .NET exception:[/]");

        string exceptionInput = Console.ReadLine() ?? string.Empty;

        await foreach (var response in agent.InvokeAsync(exceptionInput))
        {
            Console.WriteLine(response.Message.ToString());
        }
        AnsiConsole.MarkupLine("[green]Research complete[/]");
    }
}