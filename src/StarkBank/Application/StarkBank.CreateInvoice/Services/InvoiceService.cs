using StarkBank.Domain.Interfaces.Application.CreateInvoice;
using System.Diagnostics.CodeAnalysis;

namespace StarkBank.CreateInvoice.Services
{
    [ExcludeFromCodeCoverage]
    public class InvoiceService : IInvoiceService
    {
        public List<Invoice>? Create(List<Invoice> invoices, User user)
        {
            return Invoice.Create(invoices, user: user);
        }
    }
}
