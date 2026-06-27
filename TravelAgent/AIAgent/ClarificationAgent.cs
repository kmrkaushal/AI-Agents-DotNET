using Microsoft.SemanticKernel;
using Microsoft.VisualBasic;
namespace AIAgent
{
    public class ClarificationAgent
    {
        private readonly Kernel _kernel;
        public ClarificationAgent(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task ExecuteAsync(TravelContext ctx)
        {
            ctx.CurrentStage = TravelStage.Clarification;
            var conversation = FormatConversation(ctx.ConversationHistory);
            var prompt = $"""
                            You are a professional travel assistant.

                            Your job is to determine whether you need more information to proceed with a travel-related request, and if so, ask a clear and concise question.

                            ---

                            ### CONTEXT

                            Original Request:
                            {ctx.OriginalRequest}

                            Recent Conversation:
                            {conversation}

                            Missing Fields:
                            {string.Join(", ", ctx.MissingFields)}

                            ---
                            ### INSTRUCTIONS
                            1. If the user's intent is NOT related to travel:
                               - Respond with:
                                 "I'm sorry, I can only assist with travel-related requests."

                            2. If the intent IS travel-related:

                               - If the user indicates they have no more information (e.g., "no further info", "that's all", "next"):

                                 - If Missing Fields is NOT empty:
                                   - Ask ONE concise question covering the most critical missing fields

                                 - If Missing Fields is empty:
                                   - Respond with:
                                     "Thanks, I have all the details needed."

                               - Otherwise:

                                 - If Missing Fields is empty:
                                   - Respond with:
                                     "Thanks, I have all the details needed."

                                 - If Missing Fields is NOT empty:
                                   - Ask ONE clear and professional question
                                   - Combine multiple missing fields into a single question where possible
                                   - Do NOT repeat already provided information
                            ---
                            ### OUTPUT RULES
                            - Do NOT provide explanations
                            - Output only one response (either a question OR the fixed message)
                            """;
            var response = await _kernel.InvokePromptAsync(prompt);
            ctx.PendingQuestion = response.ToString();
            ctx.Logs.Add("Clarification done");
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
