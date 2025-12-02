namespace GymAPI.Models
{
    public class PaymentEmailRequest
    {
        public string ToEmail { get; set; } = "";
        public string MemberName { get; set; } = "";
        public decimal Amount { get; set; }
        public string PaymentUrl { get; set; } = "";
        public string QrPngBase64 { get; set; } = "";
    }
}
