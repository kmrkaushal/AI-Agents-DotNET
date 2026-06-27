using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace AIAgent
{
    public class TravelPlugin
    {
        private static readonly Random _rand = new();

        // =========================
        // Flight Search
        // =========================
        [KernelFunction("search_flights")]
        public string SearchFlights(
            string source,
            string destination,
            string departure_date,
            int adults,
            int children)
        {
            var date = DateTime.Parse(departure_date);

            var flights = new List<FlightOption>
            {
                CreateFlight("IndiGo", "6E-123", date, 6, 2, 4500),
                CreateFlight("Air India", "AI-202", date, 9, 2.5, 5200),
                CreateFlight("Vistara", "UK-455", date, 12, 2, 6100),
                CreateFlight("SpiceJet", "SG-890", date, 15, 2.5, 4800),
                CreateFlight("Akasa Air", "QP-101", date, 18, 2, 5300)
            };

            // Adjust price based on passengers
            var totalPassengers = adults + children;
            foreach (var f in flights)
            {
                f.Price *= totalPassengers;
            }

            return JsonSerializer.Serialize(flights);
        }

        private FlightOption CreateFlight(
            string airline,
            string flightNumber,
            DateTime date,
            int departureHour,
            double durationHours,
            decimal basePrice)
        {
            var dep = date.AddHours(departureHour);
            var arr = dep.AddHours(durationHours);

            // Add slight randomness
            var price = basePrice + _rand.Next(0, 1000);

            return new FlightOption
            {
                Airline = airline,
                FlightNumber = flightNumber,
                DepartureTime = dep,
                ArrivalTime = arr,
                Price = price
            };
        }

        // =========================
        // Hotel Search
        // =========================
        [KernelFunction("search_hotels")]
        public string SearchHotels(string destination, int nights)
        {
            var hotels = new List<HotelOption>
            {
                CreateHotel("Budget Inn", 2000, 3.8),
                CreateHotel("Comfort Stay", 3500, 4.2),
                CreateHotel("Grand Palace", 7000, 4.7),
                CreateHotel("City Lodge", 2800, 4.0),
                CreateHotel("Luxury Suites", 9000, 4.8)
            };

            // Multiply by nights
            foreach (var h in hotels)
            {
                h.PricePerNight *= nights;
            }

            return JsonSerializer.Serialize(hotels);
        }

        private HotelOption CreateHotel(string name, decimal basePrice, double rating)
        {
            var price = basePrice + _rand.Next(0, 1500);

            return new HotelOption
            {
                Name = name,
                PricePerNight = price,
                Rating = rating,
                Location = "City Center"
            };
        }

        [KernelFunction("book_flight")]
        public string BookFlight(
            string airline,
            string flightNumber,
            decimal price)
        {
            var confirmation = new BookingConfirmation
            {
                BookingId = Guid.NewGuid().ToString(),
                TotalAmount = price
            };

            return JsonSerializer.Serialize(confirmation);
        }
    }
}