using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarkBank.Domain.Interfaces.Application.ProcessInvoicePayment
{
    public interface ITransferService
    {
        public List<Transfer> CreateTransfer(long amount, User project);
    }
}
