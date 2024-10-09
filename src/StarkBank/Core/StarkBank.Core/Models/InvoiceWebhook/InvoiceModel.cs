namespace StarkBank.Core.Models.InvoiceWebhook
{
    public class InvoiceModel
    {
        public int? Amount { get; set; }
        public string? Brcode { get; set; }
        public DateTime? Created { get; set; }
        public List<DescriptionModel>? Descriptions { get; set; }
        public int? DiscountAmount { get; set; }
        public List<DiscountModel>? Discounts { get; set; }
        public DateTime? Due { get; set; }
        public int? Expiration { get; set; }
        public int? Fee { get; set; }
        public double? Fine { get; set; }
        public int? FineAmount { get; set; }
        public string? Id { get; set; }
        public double? Interest { get; set; }
        public int? InterestAmount { get; set; }
        public string? Link { get; set; }
        public string? Name { get; set; }
        public int? NominalAmount { get; set; }
        public string? Pdf { get; set; }
        public List<string>? Rules { get; set; }
        public List<string>? Splits { get; set; }
        public string? Status { get; set; }
        public List<string>? Tags { get; set; }
        public string? TaxId { get; set; }
        public List<string>? TransactionIds { get; set; }
        public DateTime? Updated { get; set; }
    }
}
