
namespace AIAgent
{
    public class FlightSelectAgent
    {
        public Task ExecuteAsync(TravelContext ctx)
        {
            ctx.CurrentStage = TravelStage.FlightSelect;
            var selected = GetSelectedFlight(ctx.LatestUserInput, ctx.FlightOptions);

            if (selected == null)
            {
                ctx.Logs.Add("Invalid flight number. Please try again.");
            }
            ctx.SelectedFlight = selected;
            ctx.Logs.Add("Flight Selected");

            return Task.CompletedTask;
        }
        public FlightOption? GetSelectedFlight(string flightNumber, List<FlightOption> flights)
        {
            return flights.FirstOrDefault(f =>
                f.FlightNumber.Contains(flightNumber, StringComparison.OrdinalIgnoreCase));
        }
    }
}
