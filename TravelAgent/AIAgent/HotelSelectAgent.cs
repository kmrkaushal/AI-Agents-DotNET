
namespace AIAgent
{
    public class HotelSelectAgent
    {
        public Task ExecuteAsync(TravelContext ctx)
        {
            ctx.CurrentStage = TravelStage.HotelSelect;
            var selected = GetSelectedHotel(ctx.LatestUserInput, ctx.HotelOptions);

            if (selected == null)
            {
                ctx.Logs.Add("Invalid hotel name. Please try again.");
            }
            ctx.SelectedHotel = selected;
            ctx.Logs.Add("Hotel Selected");

            return Task.CompletedTask;
        }
        public HotelOption? GetSelectedHotel(string Name, List<HotelOption> hotels)
        {
            return hotels.FirstOrDefault(f =>
                f.Name.Contains(Name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
