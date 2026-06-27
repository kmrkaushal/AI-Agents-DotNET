using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AIAgent
{
    public class ItineraryAgent
    {
        private readonly Kernel _kernel;

        public ItineraryAgent(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task ExecuteAsync(TravelContext ctx)
        {
            ctx.CurrentStage = TravelStage.ItineraryGeneration;

            // Guard clauses
            if (!ctx.DepartureDate.HasValue)
                throw new Exception("Departure date missing");

            var days = ctx.ReturnDate.HasValue
                ? (ctx.ReturnDate.Value - ctx.DepartureDate.Value).Days
                : 3;

            if (days <= 0) days = 3;

            var jsonSchema = """
                            {
                              "Summary": "string",
                              "Days": [
                                {
                                  "Day": number,
                                  "Theme": "string",
                                  "Activities": [
                                    {
                                      "Time": "Morning | Afternoon | Evening",
                                      "Title": "string",
                                      "Description": "string"
                                    }
                                  ]
                                }
                              ]
                            }
                            """;

            var prompt = $"""
                            You are a professional travel planner.

                            Generate a DETAILED travel itinerary in STRICT JSON format.

                            RULES:
                            - Return ONLY valid JSON
                            - Do NOT include markdown or explanation
                            - Must strictly follow the schema
                            - Generate FULL itinerary for ALL days
                            - Each day must have at least 3–5 activities
                            - Activities must be realistic and location-specific

                            SCHEMA:
                            {jsonSchema}

                            Trip Details:
                            - Source: {ctx.Source}
                            - Destination: {ctx.Destination}
                            - DepartureDate: {ctx.DepartureDate:yyyy-MM-dd}
                            - Duration: {days} days

                            QUALITY REQUIREMENTS:
                            - Include famous attractions of the destination
                            - Include local food experiences
                            - Include travel-friendly sequencing (group nearby places)
                            - Avoid generic phrases like "explore city"
                            """;

            var settings = new OpenAIPromptExecutionSettings
            {
                Temperature = 0.2
            };

            var arguments = new KernelArguments(settings);

            try
            {
                var result = await _kernel.InvokePromptAsync(prompt, arguments);

                var content = result.ToString()
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                var itinerary = JsonSerializer.Deserialize<Itinerary>(content);

                if (itinerary == null)
                    throw new Exception("Invalid itinerary format");

                ctx.Itinerary = itinerary;

                ctx.Logs.Add($"[{DateTime.UtcNow}] Itinerary generated ({days} days)");
            }
            catch (Exception ex)
            {
                ctx.Logs.Add($"[{DateTime.UtcNow}] Itinerary generation failed: {ex.Message}");
                throw;
            }
        }
    }
}