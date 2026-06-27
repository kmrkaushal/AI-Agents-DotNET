using Microsoft.SemanticKernel.ChatCompletion;

public interface IAIChatService
{
    Task<string> GetResponseAsync(string userPrompt);
    IAsyncEnumerable<string> GetStreamingResponseAsync(string userPrompt);
}
public class AIChatService : IAIChatService
{
    private readonly IChatCompletionService _chatService;
    private readonly ChatHistory _chatHistory = new();
    public AIChatService(IChatCompletionService chatService)
    {
        _chatService = chatService;

        _chatHistory.AddSystemMessage("\"You are a helpful assistant. Always format responses using Markdown with proper headings, bullet points, and code blocks when needed.\"");
    }
    public async Task<string> GetResponseAsync(string userPrompt)
    {
        _chatHistory.AddUserMessage(userPrompt);

        var response = await _chatService.GetChatMessageContentAsync(_chatHistory);

        _chatHistory.AddAssistantMessage(response.Content ?? "");

        return response.Content ?? string.Empty;
    }
    public async IAsyncEnumerable<string> GetStreamingResponseAsync(string userPrompt)
    {
        _chatHistory.AddUserMessage(userPrompt);

        string fullResponse = "";

        await foreach (var chunk in _chatService.GetStreamingChatMessageContentsAsync(_chatHistory))
        {
            if (!string.IsNullOrEmpty(chunk.Content))
            {
                fullResponse += chunk.Content;
                yield return fullResponse; 
            }
        }

        _chatHistory.AddAssistantMessage(fullResponse);
    }
}