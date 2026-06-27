using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

public class WebSearchPlugin
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public WebSearchPlugin(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Tavily:ApiKey"]!;
    }

    [KernelFunction]
    [Description("Searches the web for software errors, fixes, GitHub issues and troubleshooting discussions")]
    public async Task<string> SearchErrorSolutions([Description("Full exception message including stack trace")] string error)
    {
        var body = new
        {
            query =
                    $"""
                        Find solutions for:

                        {error}

                        Include:GitHub issues,StackOverflow,official docs, root causes
                        """,
            max_results = 5,
            include_answer = "advanced",
            search_depth = "advanced"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.tavily.com/search");

        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(request);

        return await response.Content.ReadAsStringAsync();
    }
}
