
namespace AIAgent
{
    public class ConfirmationAgent
    {
        public Task ExecuteAsync(TravelContext ctx)
        {
            // In real system:
            // - Email service
            // - SMS API
            // - WhatsApp API

            var message = $"""
                CONFIRMATION MESSAGE:

                Your trip is booked!
                Booking Ref: {ctx.BookingConfirmation.BookingId}
                """;

           //Console.WriteLine(message);

            ctx.NotificationStatus = NotificationStatus.Sent;
            ctx.Logs.Add("User notified");

            return Task.CompletedTask;
        }
    }
}
