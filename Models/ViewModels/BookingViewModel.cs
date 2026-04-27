namespace TripGenius.Models.ViewModels
{
    public class BookingViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string TripName { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime BookingDate { get; set; }
    }
}