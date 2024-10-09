using StarkBank.Shared;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public async void Test1()
        {
            var cpf = CpfService.Generate();
            var name = NameService.Generate();
        }
    }
}