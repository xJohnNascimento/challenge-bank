namespace StarkBank.Core.Models.InvoiceWebhook
{
    public class LogModel
    {
        public DateTime? Created { get; set; }
        public List<string>? Errors { get; set; }
        public string? Id { get; set; }
        public Invoice? Invoice { get; set; }
        public string? Type { get; set; }
    }
}
