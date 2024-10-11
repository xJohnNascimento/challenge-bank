using StarkBank.Domain.Interfaces.Application.ProcessInvoicePayment;

namespace StarkBank.ProcessInvoicePayment.Services
{
    public class TransferService : ITransferService
    {
        public List<Transfer> CreateTransfer(long amount, User project)
        {
            var transfers = Transfer.Create(
                [
                    new Transfer(
                        amount: amount,
                        bankCode: "20018183",
                        branchCode: "0001",
                        accountNumber: "6341320293482496",
                        taxID: "20.018.183/0001-80",
                        name: "Stark Bank S.A."
                    )
                ],
                user: project
            );

            return transfers;
        }
    }
}
