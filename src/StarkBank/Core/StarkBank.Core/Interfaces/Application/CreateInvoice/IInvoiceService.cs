namespace StarkBank.Domain.Interfaces.Application.CreateInvoice
{
    public interface IInvoiceService
    {
        public List<Invoice>? Create(List<Invoice> invoices, User user);
    }
}
