namespace StarkBank.Tests.Shared.Services
{
    public class NameServiceTests
    {
        [Fact]
        public void Generate_ShouldReturnValidFullNameFormat()
        {
            var fullName = NameService.Generate();

            Assert.Contains(" ", fullName);

            var nameParts = fullName.Split(' ');
            Assert.Equal(2, nameParts.Length);

            Assert.False(string.IsNullOrEmpty(nameParts[0]));
            Assert.False(string.IsNullOrEmpty(nameParts[1]));
        }

        [Fact]
        public void Generate_ShouldReturnDifferentNamesEachTime()
        {
            var name1 = NameService.Generate();
            var name2 = NameService.Generate();

            Assert.NotEqual(name1, name2);
        }
    }
}
