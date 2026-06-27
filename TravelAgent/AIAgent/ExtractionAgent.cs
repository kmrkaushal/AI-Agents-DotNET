using Microsoft.SemanticKernel;
using System.Text.Json;

namespace AIAgent
{
    public class ExtractionAgent
    {
        private readonly Kernel _kernel;
        public ExtractionAgent(Kernel kernel)
        {
            _kernel=kernel;
        }
        public async Task ExecuteAsync(TravelContext ctx)
        {
            var conversation = FormatConversation(ctx.ConversationHistory);

            var prompt = $"""
                            You are a strict information extraction system.

                            Original Request:
                            {ctx.OriginalRequest}

                            Recent Conversation:
                            {conversation}

                            Latest User Input:
                            {ctx.LatestUserInput}

                            IMPORTANT:
                            - Preserve previously extracted values
                            - Only update if new info is provided
                            - Do not remove valid existing fields
                            - All dates must be in ISO 8601 format (YYYY-MM-DD)    
                            - Budget must be a NUMBER (no text, no currency symbols)
                            - Travelers.Adults and Travelers.Children must be NUMBERS

                            Return ONLY JSON:
                            - Source
                            - Destination
                            - DepartureDate
                            - ReturnDate
                            - Travelers (Adults, Children)
                            - Budget
                            - Preferences (FlightClass, HotelType)
                            """;

            var response = await _kernel.InvokePromptAsync(prompt);

            var json = response.ToString();

            try
            {
                var extracted = JsonSerializer.Deserialize<TravelContext>(json);
                Merge(ctx, extracted);
            }
            catch
            {
                ctx.Logs.Add("Extraction failed");
            }

            ctx.Logs.Add("Extraction done");
        }
        private void Merge(TravelContext target, TravelContext? source)
        {
            if (source == null) return;
            target.Source ??= source.Source;
            target.Destination ??= source.Destination;
            target.DepartureDate ??= source.DepartureDate;
            target.ReturnDate ??= source.ReturnDate;

            if (source.Travelers.Adults > 0)
                target.Travelers.Adults = source.Travelers.Adults;

            if (source.Travelers.Children > 0)
                target.Travelers.Children = source.Travelers.Children;

            target.Budget ??= source.Budget;
        }
        private string FormatConversation(List<ChatMessage> history, int maxTurns = 6)
        {
            var recent = history
                .TakeLast(maxTurns * 2); // user + agent pairs

            return string.Join("\n", recent.Select(m =>
                $"{m.Role.ToUpper()}: {m.Content}"));
        }
    }

}
