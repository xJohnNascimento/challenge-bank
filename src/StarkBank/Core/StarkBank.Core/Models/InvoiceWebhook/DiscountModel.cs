﻿namespace StarkBank.Domain.Models.InvoiceWebhook
{
    public class DiscountModel
    {
        public DateTime? Due { get; set; }
        public double? Percentage { get; set; }
    }
}
