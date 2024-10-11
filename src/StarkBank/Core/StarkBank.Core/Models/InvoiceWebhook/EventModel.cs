namespace StarkBank.Domain.Models.InvoiceWebhook
{
    public class EventModel
    {
        public DateTime? Created { get; set; }
        public string? Id { get; set; }
        public LogModel? Log { get; set; }
        public string? Subscription { get; set; }
        public string? WorkspaceId { get; set; }
    }
}
