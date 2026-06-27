
namespace AIAgent
{
    public class TravelDecision
    {
        public SupervisorAction Action { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
    public enum SupervisorAction
    {
        Extract,
        Clarify,
        SearchFlights,
        SelectFlight,
        SearchHotels,
        SelectHotel,
        GenerateItinerary,
        Book,
        Confirm,
        Finish
    }
}
