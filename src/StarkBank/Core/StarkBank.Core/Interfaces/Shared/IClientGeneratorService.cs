namespace StarkBank.Domain.Interfaces.Shared
{
    public interface IClientGeneratorService
    {
        public string GenerateName();
        public string GenerateCpf();
    }
}
