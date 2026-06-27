
namespace AIAgent
{
    public class TravelContext
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public string OriginalRequest { get; set; } = string.Empty;
        public string LatestUserInput { get; set; } = string.Empty;
        public string? Source { get; set; }
        public string? Destination { get; set; }
        public DateTime? DepartureDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public Travelers Travelers { get; set; } = new();
        public decimal? Budget { get; set; }
        public Preferences Preferences { get; set; } = new();

        // Agent outputs
        public FlightOption SelectedFlight { get; set; } = new();
        public HotelOption SelectedHotel { get; set; } = new();
        public List<FlightOption> FlightOptions { get; set; } = new();
        public List<HotelOption> HotelOptions { get; set; } = new();
        public BookingConfirmation? BookingConfirmation { get; set; }
        public NotificationStatus NotificationStatus { get; set; } = NotificationStatus.NotSent;
        public Itinerary? Itinerary { get; set; }

        // Workflow / orchestration
        public TravelStage CurrentStage { get; set; } = TravelStage.InfoCollection;

        public string? PendingQuestion { get; set; }

        public List<string> MissingFields { get; set; } = new();

        public int Iteration { get; set; } = 0;

        public List<string> Logs { get; set; } = new();

        public bool IsComplete =>
            !string.IsNullOrWhiteSpace(Source) &&
            !string.IsNullOrWhiteSpace(Destination) &&
            DepartureDate.HasValue &&
            Travelers.Adults > 0;

        // Validation hook (optional advanced)
        public IEnumerable<string> Validate()
        {
            if (ReturnDate.HasValue && DepartureDate.HasValue &&
                ReturnDate < DepartureDate)
            {
                yield return "ReturnDate cannot be earlier than DepartureDate.";
            }

            if (Travelers.Adults == 0)
            {
                yield return "At least one adult traveler is required.";
            }
        }
        public List<ChatMessage> ConversationHistory { get; set; } = new();
    }
    public class FlightOption
    {
        public string Airline { get; set; } = "";
        public string FlightNumber { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
    }

    public class HotelOption
    {
        public string Name { get; set; } = "";
        public decimal PricePerNight { get; set; }
        public double Rating { get; set; }
        public string Location { get; set; } = "";
    }
    public class BookingConfirmation
    {
        public string BookingId { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
    }
    public class Itinerary
    {
        public string Summary { get; set; } = string.Empty;
        public List<DayPlan> Days { get; set; } = new();
    }

    public class DayPlan
    {
        public int Day { get; set; }
        public string Theme { get; set; } = string.Empty;
        public List<Activity> Activities { get; set; } = new();
    }

    public class Activity
    {
        public string Time { get; set; } = string.Empty; // Morning/Afternoon/Evening
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
    public class Travelers
    {
        public int? Adults { get; set; }
        public int? Children { get; set; }
    }
    public class Preferences
    {
        public FlightClass? FlightClass { get; set; }
        public HotelType? HotelType { get; set; }
    }
    public enum FlightClass
    {
        Economy,
        PremiumEconomy,
        Business,
        First
    }
    public enum HotelType
    {
        Budget,
        ThreeStar,
        FourStar,
        FiveStar,
        Luxury
    }
    public enum TravelStage
    {
        InfoCollection,
        Clarification,
        FlightSearch,
        HotelSearch,
        FlightSelect,
        HotelSelect,
        ItineraryGeneration,
        Booking,
        Completed
    }
    public enum NotificationStatus
    {
        NotSent,
        Pending,
        Sent,
        Failed
    }
    public class ChatMessage
    {
        public string Role { get; set; } = ""; // "user" | "agent" | "system"
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
    public class AgentResponse
    {
        public string Message { get; set; } = "";
        public bool IsFinal { get; set; }
        public bool NeedsUserInput { get; set; }
    }
}
