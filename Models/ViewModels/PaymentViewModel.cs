namespace TripGenius.Models.ViewModels
{
    public class PaymentViewModel
    {
        public int Id { get; set; }

        public string TransactionId { get; set; }

        public string CustomerName { get; set; }

        public string TripName { get; set; }

        public decimal Amount { get; set; }

        public string Method { get; set; }

        public DateTime DateTime { get; set; }

        public string Status { get; set; }
    }
}