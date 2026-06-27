using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;


namespace AIAgent
{
    public class BookingAgent
    {
        private readonly Kernel _kernel;

        public BookingAgent(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task ExecuteAsync(TravelContext ctx)
        {
            ctx.CurrentStage = TravelStage.Booking;

            if (ctx.FlightOptions == null || !ctx.FlightOptions.Any())
                throw new Exception("No flight options available for booking");

            var flightsJson = JsonSerializer.Serialize(ctx.FlightOptions, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var prompt = """
                    You are a flight booking agent.

                    Your task:
                    1. Select the BEST flight from the provided options using STRICT criteria:
                       - Primary: Lowest price
                       - Secondary: Earliest departure time among lowest-priced options

                    2. Call the function Travel.book_flight with the selected flight.

                    STRICT RULES:
                    - Do NOT generate booking confirmation yourself
                    - MUST call the tool
                    - Return ONLY the tool response
                    - Selection MUST be from the provided list (no fabrication)

                    INPUT FLIGHTS (JSON ARRAY):
                    {flightsJson}

                    TOOL INPUT REQUIREMENT:
                    Pass the EXACT selected flight object to Travel.book_flight.

                    EXPECTED TOOL RESPONSE (STRICT):
                    {"BookingId": "string",
                      "AddedAt": "ISO 8601 datetime",
                      "TotalAmount": number
                    }
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

                var bookingResult = result.GetValue<string>();

                if (string.IsNullOrWhiteSpace(bookingResult))
                    throw new Exception("Empty booking response");

                var confirmation = JsonSerializer.Deserialize<BookingConfirmation>(bookingResult);

                if (confirmation == null)
                    throw new Exception("Invalid booking response format");

                ctx.BookingConfirmation = confirmation;

                ctx.Logs.Add($"[{DateTime.UtcNow}] Booking completed: {confirmation.BookingId}");
            }
            catch (Exception ex)
            {
                ctx.Logs.Add($"[{DateTime.UtcNow}] Booking failed: {ex.Message}");
                throw;
            }
        }
    }
}