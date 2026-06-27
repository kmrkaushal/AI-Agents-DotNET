using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

    public class HotelPlugin
    {
     
        [KernelFunction("search_hotels")]
        public string SearchHotels(string destination, int nights)
        {
            var hotels = new List<HotelOption>
            {
                CreateHotel("Budget Inn", "2000", 3.8),
                CreateHotel("Comfort Stay", "3500", 4.2),
                CreateHotel("Grand Palace", "7000", 4.7),
                CreateHotel("City Lodge", "2800", 4.0),
                CreateHotel("Luxury Suites", "9000", 4.8)
            };

            // Multiply by nights
            foreach (var h in hotels)
            {
                h.TotalAmount =(Convert.ToDouble(h.PricePerNight)* nights);
            }

            return JsonSerializer.Serialize(hotels);
        }

        private HotelOption CreateHotel(string name, string basePrice, double rating)
        {
            return new HotelOption
            {
                Name = name,
                PricePerNight = basePrice,
                Rating = rating,
            };
        }
    }

    public class HotelOption
    {
        public string Name { get; set; } = "";
        public string PricePerNight { get; set; }
        public double Rating { get; set; }
    public double TotalAmount { get; set; }
}