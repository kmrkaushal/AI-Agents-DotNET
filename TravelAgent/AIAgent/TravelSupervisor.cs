using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
namespace AIAgent
{
    public class TravelSupervisor
    {
        private readonly IChatCompletionService _chat;

        private static readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        public TravelSupervisor(Kernel kernel)
        {
            _chat = kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<TravelDecision> DecideAsync(TravelContext ctx)
        {
            UpdateMissingFields(ctx);

            // 1.DETERMINISTIC LAYER (fast + reliable)
            var deterministic = TryDeterministicDecision(ctx);
            if (deterministic != null)
                return deterministic;

            // 2. If request already structured → skip LLM
            if (IsStructuredRequest(ctx))
            {
                return new TravelDecision
                {
                    Action = SupervisorAction.SearchFlights,
                    Reason = "Structured request detected"
                };
            }

            // 3. LLM FALLBACK (only when needed)
            return await GetLlmDecision(ctx);
        }

        // =========================
        // Deterministic Engine
        // =========================
        private TravelDecision? TryDeterministicDecision(TravelContext ctx)
        {
            if (ctx.MissingFields.Any())
            {
                return Decision(SupervisorAction.Clarify, "Missing required fields");
            }

            if (ctx.FlightOptions == null || !ctx.FlightOptions.Any())
            {
                return Decision(SupervisorAction.SearchFlights, "Flights not fetched");
            }
            if (string.IsNullOrEmpty(ctx.SelectedFlight.FlightNumber))
            {
                return Decision(SupervisorAction.SelectFlight, "Flight not selected");
            }
            if (ctx.HotelOptions == null || !ctx.HotelOptions.Any())
            {
                return Decision(SupervisorAction.SearchHotels, "Hotels not fetched");
            }
            if (string.IsNullOrEmpty(ctx.SelectedHotel.Name))
            {
                return Decision(SupervisorAction.SelectHotel, "Hotel not selected");
            }
            if (ctx.Itinerary == null)
            {
                return Decision(SupervisorAction.GenerateItinerary, "Itinerary missing");
            }

            if (ctx.BookingConfirmation == null)
            {
                return Decision(SupervisorAction.Book, "Booking not completed");
            }

            if (ctx.NotificationStatus != NotificationStatus.Sent)
            {
                return Decision(SupervisorAction.Confirm, "User not notified");
            }

            return Decision(SupervisorAction.Finish, "Workflow complete");
        }

        // =========================
        // LLM Fallback Layer
        // =========================
        private async Task<TravelDecision> GetLlmDecision(TravelContext ctx)
        {
            var history = BuildPrompt(ctx);

            for (int attempt = 1; attempt <= 2; attempt++) // reduced retries
            {
                var response = await _chat.GetChatMessageContentAsync(history);

                try
                {
                    var decision = ParseDecision(response.Content!);
                    ValidateDecision(decision);
                    return decision;
                }
                catch
                {
                    history.AddUserMessage("""
                        Invalid response.

                        Return STRICT JSON:
                        {
                          "Action": "Extract | Clarify | SearchFlights | GenerateItinerary | Book | Confirm | Finish",
                          "Reason": "short"
                        }
                        """);
                }
            }

            // Fallback safety (never fail system)
            return Decision(SupervisorAction.Clarify, "Fallback after LLM failure");
        }

        // =========================
        // Helpers
        // =========================
        private TravelDecision Decision(SupervisorAction action, string reason)
        {
            return new TravelDecision
            {
                Action = action,
                Reason = reason
            };
        }

        private bool IsStructuredRequest(TravelContext ctx)
        {
            return !string.IsNullOrWhiteSpace(ctx.Source)
                   && !string.IsNullOrWhiteSpace(ctx.Destination)
                   && ctx.DepartureDate.HasValue;
        }

        private void UpdateMissingFields(TravelContext context)
        {
            context.MissingFields.Clear();

            if (string.IsNullOrWhiteSpace(context.Source))
                context.MissingFields.Add("Source");

            if (string.IsNullOrWhiteSpace(context.Destination))
                context.MissingFields.Add("Destination");

            if (!context.DepartureDate.HasValue)
                context.MissingFields.Add("DepartureDate");

            if (!context.ReturnDate.HasValue)
                context.MissingFields.Add("ReturnDate");

            if (context.Travelers.Adults == null || context.Travelers.Adults == 0)
                context.MissingFields.Add("Travelers");

            if (context.Budget == null || context.Budget == 0)
                context.MissingFields.Add("Budget");
        }

        private ChatHistory BuildPrompt(TravelContext ctx)
        {
            var history = new ChatHistory();
            var conversation = FormatConversation(ctx.ConversationHistory);

            history.AddSystemMessage("""
                                    You are a travel supervisor agent.

                                    Decide the NEXT BEST ACTION.

                                    Actions:
                                    - Extract
                                    - Clarify
                                    - SearchFlights
                                    - GenerateItinerary
                                    - Book
                                    - Confirm
                                    - Finish

                                    Rules:
                                    - Choose ONE action
                                    - Prefer logical progression
                                    - Do not skip steps
                                    - Do not repeat completed steps

                                    Return STRICT JSON:
                                    {
                                      "Action": "...",
                                      "Reason": "short"
                                    }
                                    """);

            history.AddUserMessage($$"""
                                    Original Request:
                                    {{ctx.OriginalRequest}}

                                    Conversation:
                                    {{conversation}}

                                    State:
                                    - Source: {{ctx.Source}}
                                    - Destination: {{ctx.Destination}}
                                    - DepartureDate: {{ctx.DepartureDate}}
                                    - ReturnDate: {{ctx.ReturnDate}}
                                    - Adults: {{ctx.Travelers.Adults}}
                                    - Children: {{ctx.Travelers.Children}}

                                    - FlightsCount: {{ctx.FlightOptions?.Count ?? 0}}
                                    - BookingDone: {{ctx.BookingConfirmation != null}}
                                    - ItineraryReady: {{ctx.Itinerary != null}}
                                    - NotificationSent: {{ctx.NotificationStatus}}

                                    Stage: {{ctx.CurrentStage}}
                                    Iteration: {{ctx.Iteration}}
                                    """);

            return history;
        }

        private TravelDecision ParseDecision(string content)
        {
            var cleaned = content
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            return JsonSerializer.Deserialize<TravelDecision>(cleaned, _jsonOptions)
                   ?? throw new Exception("Failed to parse decision");
        }

        private void ValidateDecision(TravelDecision decision)
        {
            if (!Enum.IsDefined(typeof(SupervisorAction), decision.Action))
                throw new Exception($"Invalid action: {decision.Action}");
        }

        private string FormatConversation(List<ChatMessage> history, int maxTurns = 6)
        {
            var recent = history.TakeLast(maxTurns * 2);

            return string.Join("\n", recent.Select(m =>
                $"{m.Role.ToUpper()}: {m.Content}"));
        }
    }
}