using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AIAgent
{
    public class HotelSearchAgent
    {
        private readonly Kernel _kernel;

        public HotelSearchAgent(Kernel kernel)
        {
            _kernel = kernel;
        }
        public async Task ExecuteAsync(TravelContext ctx)
        {
            ctx.CurrentStage = TravelStage.HotelSearch;

            // Guard clauses
            if (string.IsNullOrWhiteSpace(ctx.Destination))
                throw new Exception("Destination is missing");

                var prompt = """
                                You are a hotel search agent.

                                Your task:
                                - Call the function Travel.search_hotels with the provided input.

                                STRICT RULES:
                                - Do NOT generate hotel data manually
                                - Preserve previously extracted values
                                - Only update fields if new information is provided
                                - Do not remove valid existing fields
                                - PricePerNight must be a NUMBER (no text, no currency symbols)
                                - MUST call the tool
                                - Return ONLY the tool response
                                - The response MUST be a collection (array) of hotels objects

                                OUTPUT FORMAT (STRICT):
                                [
                                  {
                                    "Name": "string",
                                    "PricePerNight": number,
                                    "Rating": number,
                                    "Location": "string"
                                  }
                                ]
                                """;

            var settings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0
            };

            var arguments = new KernelArguments(settings);

            try
            {
                var result = await _kernel.InvokePromptAsync(prompt, arguments);

                var json = result.ToString();

                if (string.IsNullOrWhiteSpace(json))
                    throw new Exception("Empty hotel search result");

                // Deserialize properly
                var hotels = JsonSerializer.Deserialize<List<HotelOption>>(json);

                if (hotels == null || !hotels.Any())
                    throw new Exception("No hotel returned");

                ctx.HotelOptions = hotels;

                ctx.Logs.Add($"[{DateTime.UtcNow}] Hotels fetched: {hotels.Count}");
            }
            catch (Exception ex)
            {
                ctx.Logs.Add($"[{DateTime.UtcNow}] Hotel search failed: {ex.Message}");
                throw;
            }
        }
    }
}