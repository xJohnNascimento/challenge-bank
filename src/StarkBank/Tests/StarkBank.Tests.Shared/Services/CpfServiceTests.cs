using StarkBank.Shared.Services;
using System.Text.RegularExpressions;

namespace StarkBank.Tests.Shared.Services
{
    public class CpfServiceTests
    {
        [Fact]
        public void Generate_ShouldReturnValidCpfFormat()
        {
            var cpf = CpfService.Generate();

            var regex = new Regex(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
            Assert.Matches(regex, cpf);
        }

        [Fact]
        public void Generate_ShouldReturn11DigitsInCpf()
        {
            var cpf = CpfService.Generate();

            var digitsOnly = cpf.Replace(".", "").Replace("-", "");

            Assert.Equal(11, digitsOnly.Length);
        }

        [Fact]
        public void Generate_ShouldReturnDifferentCpfsEachTime()
        {
            var cpf1 = CpfService.Generate();
            var cpf2 = CpfService.Generate();

            Assert.NotEqual(cpf1, cpf2);
        }
    }
}
