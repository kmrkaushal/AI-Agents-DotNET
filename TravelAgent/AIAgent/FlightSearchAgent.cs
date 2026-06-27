using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AIAgent
{
    public class FlightSearchAgent
    {
        private readonly Kernel _kernel;

        public FlightSearchAgent(Kernel kernel)
        {
            _kernel = kernel;
        }
        public async Task ExecuteAsync(TravelContext ctx)
        {
            ctx.CurrentStage = TravelStage.FlightSearch;

            // Guard clauses
            if (string.IsNullOrWhiteSpace(ctx.Source))
                throw new Exception("Source is missing");

            if (string.IsNullOrWhiteSpace(ctx.Destination))
                throw new Exception("Destination is missing");

            if (!ctx.DepartureDate.HasValue)
                throw new Exception("Departure date is missing");
                var prompt = """
                                You are a flight search agent.

                                Your task:
                                - Call the function Travel.search_flights with the provided input.

                                STRICT RULES:
                                - Do NOT generate flight data manually
                                - Preserve previously extracted values
                                - Only update fields if new information is provided
                                - Do not remove valid existing fields
                                - All dates must be in ISO 8601 format (YYYY-MM-DD)
                                - Price must be a NUMBER (no text, no currency symbols)
                                - MUST call the tool
                                - Return ONLY the tool response
                                - The response MUST be a collection (array) of flight objects

                                OUTPUT FORMAT (STRICT):
                                [
                                  {
                                    "FlightNumber": "string",
                                    "Airline": "string",
                                    "DepartureTime": "string",
                                    "ArrivalTime": "string",
                                    "Price": number
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
                    throw new Exception("Empty flight search result");

                // Deserialize properly
                var flights = JsonSerializer.Deserialize<List<FlightOption>>(json);

                if (flights == null || !flights.Any())
                    throw new Exception("No flights returned");

                ctx.FlightOptions = flights;

                ctx.Logs.Add($"[{DateTime.UtcNow}] Flights fetched: {flights.Count}");
            }
            catch (Exception ex)
            {
                ctx.Logs.Add($"[{DateTime.UtcNow}] Flight search failed: {ex.Message}");
                throw;
            }
        }
    }
}