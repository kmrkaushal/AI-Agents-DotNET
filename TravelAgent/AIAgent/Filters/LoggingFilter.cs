using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace AIAgent.Filters
{
    public class LoggingFilter : IFunctionInvocationFilter
    {
        private readonly ILogger<LoggingFilter> _logger;

        public LoggingFilter(ILogger<LoggingFilter> logger)
        {
            _logger = logger;
        }
        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            Console.WriteLine("FILTER HIT");
            _logger.LogInformation("Calling function: {Function}", context.Function.Name);

            await next(context);

            _logger.LogInformation("Function result: {Result}", context.Result);
        }
    }
}
