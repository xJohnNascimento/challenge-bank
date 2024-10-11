using System.Reflection;
using System.Text.RegularExpressions;

namespace StarkBank.Shared.Tests.Services
{
    public class ClientGeneratorServiceTest
    {
        private readonly ClientGeneratorService _service = new();

        #region GenerateName Tests

        [Fact]
        public void GenerateName_ShouldReturnNonEmptyString()
        {
            var name = _service.GenerateName();

            Assert.False(string.IsNullOrWhiteSpace(name));
        }

        [Fact]
        public void GenerateName_ShouldContainSpace()
        {
            var name = _service.GenerateName();

            Assert.Contains(" ", name);
        }

        [Fact]
        public void GenerateName_ShouldContainValidFirstAndLastNames()
        {
            var name = _service.GenerateName();

            var parts = name.Split(' ');
            Assert.Equal(2, parts.Length);

            var firstNamesField = typeof(ClientGeneratorService).GetField("FirstNames", BindingFlags.NonPublic | BindingFlags.Static);
            var lastNamesField = typeof(ClientGeneratorService).GetField("LastNames", BindingFlags.NonPublic | BindingFlags.Static);

            var firstNames = firstNamesField!.GetValue(null) as List<string>;
            var lastNames = lastNamesField!.GetValue(null) as List<string>;

            Assert.Contains(parts[0], firstNames!);
            Assert.Contains(parts[1], lastNames!);
        }

        [Fact]
        public void GenerateName_ShouldReturnDifferentNames()
        {
            var name1 = _service.GenerateName();
            var name2 = _service.GenerateName();

            Assert.NotEqual(name1, name2);
        }

        #endregion

        #region GenerateCpf Tests

        [Fact]
        public void GenerateCpf_ShouldReturnValidFormat()
        {
            var cpf = _service.GenerateCpf();

            var regex = new Regex(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$");
            Assert.Matches(regex, cpf);
        }

        [Fact]
        public void GenerateCpf_ShouldReturn11Digits()
        {
            var cpf = _service.GenerateCpf();

            var digitsOnly = cpf.Replace(".", "").Replace("-", "");
            Assert.Equal(11, digitsOnly.Length);
        }

        [Fact]
        public void GenerateCpf_ShouldReturnDifferentCpfs()
        {
            var cpf1 = _service.GenerateCpf();
            var cpf2 = _service.GenerateCpf();

            Assert.NotEqual(cpf1, cpf2);
        }

        #endregion
    }
}
