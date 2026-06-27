using AIAgent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var builder = Kernel.CreateBuilder();
builder.Services.Configure<AzureOpenAISettings>(config.GetSection("AzureOpenAI"));

builder.Services.AddAIAgent(
    deployment: config["AzureOpenAI:Deployment"]!,
    endpoint: config["AzureOpenAI:Endpoint"]!,
    apiKey: config["AzureOpenAI:ApiKey"]!
);

var app = builder.Build();

// Agents (stateless → transient)
builder.Services.AddTransient<TravelSupervisor>();
builder.Services.AddTransient<ExtractionAgent>();
builder.Services.AddTransient<FlightSearchAgent>();
builder.Services.AddTransient<FlightSelectAgent>();
builder.Services.AddTransient<HotelSearchAgent>();
builder.Services.AddTransient<HotelSelectAgent>();
builder.Services.AddTransient<BookingAgent>();
builder.Services.AddTransient<ItineraryAgent>();
builder.Services.AddTransient<ConfirmationAgent>();
builder.Services.AddTransient<ClarificationAgent>();
builder.Services.AddTransient<TravelOrchestrator>();

var provider = builder.Services.BuildServiceProvider();
var orchestrator = provider.GetRequiredService<TravelOrchestrator>();

//Hint:  Plan a 4-day Goa trip under 40k from Delhi on 20-04-2026 for 2 days with two person

var ctx = new TravelContext();

Console.WriteLine("**** Travel Agent ****");

while (true)
{
    Console.Write("User: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    // Preserve original request
    if (string.IsNullOrEmpty(ctx.OriginalRequest))
        ctx.OriginalRequest = input;

    ctx.LatestUserInput = input;

    // Add to history
    ctx.ConversationHistory.Add(new ChatMessage
    {
        Role = "user",
        Content = input
    });

    var response = await orchestrator.RunAsync(ctx);

    Console.WriteLine("Agent: " + response.Message);

    // Add agent response
    ctx.ConversationHistory.Add(new ChatMessage
    {
        Role = "agent",
        Content = response.Message
    });

    if (response.IsFinal)
        break;
}