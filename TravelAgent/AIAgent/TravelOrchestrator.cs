using System.Text;

namespace AIAgent
{
    public class TravelOrchestrator
    {
        private readonly TravelSupervisor _supervisor;
        private readonly ExtractionAgent _extract;
        private readonly FlightSearchAgent _Flightsearch;
        private readonly FlightSelectAgent _selectedFlight;
        private readonly HotelSearchAgent _hotelsearch;
        private readonly HotelSelectAgent _hotelselect;
        private readonly BookingAgent _booking;
        private readonly ItineraryAgent _itinerary;
        private readonly ConfirmationAgent _confirm;
        private readonly ClarificationAgent _clarify;
        public TravelOrchestrator(
            TravelSupervisor supervisor,
            ExtractionAgent extract,
            FlightSearchAgent search,
            FlightSelectAgent selectedFlight,
            HotelSearchAgent hotelsearch,
            HotelSelectAgent hotelselect,
            BookingAgent booking,
            ItineraryAgent itinerary,
            ConfirmationAgent confirm,
            ClarificationAgent clarify)
        {
            _supervisor = supervisor;
            _extract = extract;
            _Flightsearch = search;
            _hotelsearch = hotelsearch;
            _booking = booking;
            _itinerary = itinerary;
            _confirm = confirm;
            _clarify = clarify;
            _selectedFlight = selectedFlight;
            _hotelselect = hotelselect;
        }

        public async Task<AgentResponse> RunAsync(TravelContext ctx)
        {
            while (true)
            {
                ctx.Iteration++;

                if (ctx.Iteration > 25)
                    throw new Exception("Workflow exceeded safe iteration limit");

                //Step 1: Update state
                if(ctx.CurrentStage==TravelStage.InfoCollection || ctx.CurrentStage == TravelStage.Clarification)
                  await _extract.ExecuteAsync(ctx);

                // Step 2: Decide
                var decision = await _supervisor.DecideAsync(ctx);
                ctx.Logs.Add($"[{DateTime.UtcNow}] Decision: {decision.Action} | {decision.Reason}");
                
                // Step 3: Act
                switch (decision.Action)
                {
                    case SupervisorAction.Clarify:
                        await _clarify.ExecuteAsync(ctx);
                        if (!string.IsNullOrWhiteSpace(ctx.PendingQuestion))
                            return new AgentResponse
                            {
                                Message = ctx.PendingQuestion!,
                                NeedsUserInput = true,
                                IsFinal = false
                            };
                        break;

                    case SupervisorAction.SearchFlights:
                        await _Flightsearch.ExecuteAsync(ctx);
                        return new AgentResponse
                        {
                            Message = FormatFlightOptions(ctx.FlightOptions),
                            NeedsUserInput = true,
                            IsFinal = false
                        };

                    case SupervisorAction.SelectFlight:
                        await _selectedFlight.ExecuteAsync(ctx);
                        break;

                    case SupervisorAction.SearchHotels:
                        await _hotelsearch.ExecuteAsync(ctx);
                        return new AgentResponse
                        {
                            Message = FormatHotelOptions(ctx.HotelOptions),
                            NeedsUserInput = true,
                            IsFinal = false
                        };

                    case SupervisorAction.SelectHotel:
                        await _hotelselect.ExecuteAsync(ctx);
                        break;

                    case SupervisorAction.GenerateItinerary:
                        await _itinerary.ExecuteAsync(ctx);
                        break;

                    case SupervisorAction.Book:
                        await _booking.ExecuteAsync(ctx);
                        break;

                    case SupervisorAction.Confirm:
                        await _confirm.ExecuteAsync(ctx);
                        break;

                    case SupervisorAction.Finish:
                        return new AgentResponse
                        {
                            Message = BuildUserResponse(ctx),
                            IsFinal = true
                        };

                    default:
                        throw new Exception($"Invalid action: {decision.Action}");
                }
            }
        }

        private string BuildUserResponse(TravelContext ctx)
        {
            var sb = new System.Text.StringBuilder();

            // Booking Section (optional now)
            if (ctx.BookingConfirmation != null)
            {
                sb.AppendLine("Booking Confirmed!");
                sb.AppendLine();

                sb.AppendLine($"Booking ID: {ctx.BookingConfirmation.BookingId}");
                //sb.AppendLine($"Total Amount: {ctx.BookingConfirmation.TotalAmount}");
                sb.AppendLine();
            }

            // Trip Info
            if (!string.IsNullOrWhiteSpace(ctx.Source) && !string.IsNullOrWhiteSpace(ctx.Destination))
            {
                sb.AppendLine($"Trip: {ctx.Source} → {ctx.Destination}");
            }

            if (ctx.DepartureDate.HasValue)
            {
                sb.AppendLine($"Departure: {ctx.DepartureDate:dd MMM yyyy}");
            }

            sb.AppendLine();

            // =========================
            // Flight Section
            // =========================
            if (ctx.FlightOptions?.Any() == true)
            {
                sb.AppendLine("Flight Details:");

                var selectedFlight = ctx.SelectedFlight;

                if (selectedFlight != null)
                {
                    sb.AppendLine($"{selectedFlight.Airline} ({selectedFlight.FlightNumber})");
                    sb.AppendLine($"  Departure: {selectedFlight.DepartureTime:HH:mm}");
                    sb.AppendLine($"  Arrival: {selectedFlight.ArrivalTime:HH:mm}");
                    sb.AppendLine($"  Price: ₹{selectedFlight.Price:N0}");
                }
                else
                {
                    foreach (var flight in ctx.FlightOptions.Take(3))
                    {
                        sb.AppendLine($"{flight.Airline} ({flight.FlightNumber}) - ₹{flight.Price:N0}");
                    }

                    sb.AppendLine();
                    sb.AppendLine("Please select a flight (enter Flight Number)");
                }

                sb.AppendLine();
            }

            // =========================
            // Hotel Section
            // =========================
            if (ctx.HotelOptions?.Any() == true)
            {
                sb.AppendLine("Hotel Details:");

                var selectedHotel = ctx.SelectedHotel;

                if (selectedHotel != null)
                {
                    sb.AppendLine($"{selectedHotel.Name}");
                    sb.AppendLine($"  Location: {selectedHotel.Location}");
                    sb.AppendLine($"  Price/Night: ₹{selectedHotel.PricePerNight:N0}");
                    sb.AppendLine($"  Rating: {selectedHotel.Rating}");
                }
                else
                {
                    foreach (var hotel in ctx.HotelOptions.Take(3))
                    {
                        sb.AppendLine($"{hotel.Name} - ₹{hotel.PricePerNight:N0} ({hotel.Rating})");
                    }

                    sb.AppendLine();
                    sb.AppendLine("Please select a hotel (enter Hotel Name or Number)");
                }

                sb.AppendLine();
            }

            // =========================
            // Final Travel Plan
            // =========================
            if (ctx.SelectedFlight != null && ctx.SelectedHotel != null)
            {
                var total = ctx.SelectedFlight.Price + ctx.SelectedHotel.PricePerNight;

                sb.AppendLine("Travel Plan Summary:");
                sb.AppendLine($"Total Estimated Cost: ₹{total:N0}");
                sb.AppendLine();
                //sb.AppendLine("Proceed to payment?");
                sb.AppendLine();
            }

            // =========================
            // Itinerary
            // =========================
            if (ctx.Itinerary != null)
            {
                sb.AppendLine("Itinerary:");
                sb.AppendLine(FormatItinerary(ctx.Itinerary));
            }

            // Fallback
            if (sb.Length == 0)
            {
                return "Processing your request...";
            }

            return sb.ToString();
        }
        private string FormatItinerary(Itinerary? itinerary)
        {
            if (itinerary == null) return string.Empty;

            var sb = new System.Text.StringBuilder();

            // Summary
            sb.AppendLine(itinerary.Summary);
            sb.AppendLine();

            foreach (var day in itinerary.Days.OrderBy(d => d.Day))
            {
                sb.AppendLine($"Day {day.Day}: {day.Theme}");

                foreach (var act in day.Activities)
                {
                    sb.AppendLine($"  - {act.Time}: {act.Title}");
                    sb.AppendLine($"    {act.Description}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
        private string FormatFlightOptions(List<FlightOption> flights)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Available Flights:");
            sb.AppendLine();

            foreach (var f in flights)
            {
                sb.AppendLine($" {f.Airline} ({f.FlightNumber})");
                sb.AppendLine($"  Departure: {f.DepartureTime:HH:mm}");
                sb.AppendLine($"  Arrival: {f.ArrivalTime:HH:mm}");
                sb.AppendLine($"  Price: ₹{f.Price}");
                sb.AppendLine();
            }
            sb.AppendLine("Please enter the Flight Number to select your flight:");
            return sb.ToString();
        }
        private string FormatHotelOptions(List<HotelOption> hotels)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Available Hotels:");
            sb.AppendLine();

            foreach (var h in hotels)
            {
                sb.AppendLine($" {h.Name}");
                sb.AppendLine($"  Location: {h.Location}");
                sb.AppendLine($"  Price/Night: ₹{h.PricePerNight}");
                sb.AppendLine($"  Rating: ⭐ {h.Rating}");
                sb.AppendLine();
            }

            sb.AppendLine("Please enter the Hotel Name to select your hotel:");
            return sb.ToString();
        }
    }
}
