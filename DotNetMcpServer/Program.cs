using ModelContextProtocol.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithTools<WeatherTools>();

var app = builder.Build();

app.MapMcp();

app.Run();