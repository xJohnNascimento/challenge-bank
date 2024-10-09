namespace StarkBank.Shared.Services
{
    public static class CpfService
    {
        public static string Generate()
        {
            var random = new Random();
            var soma = 0;

            var cpf = new int[11];
            for (var i = 0; i < 9; i++)
            {
                cpf[i] = random.Next(0, 10);
                soma += cpf[i] * (10 - i);
            }

            var remainder = soma % 11;
            cpf[9] = remainder < 2 ? 0 : 11 - remainder;

            soma = 0;
            for (var i = 0; i < 10; i++)
            {
                soma += cpf[i] * (11 - i);
            }
            remainder = soma % 11;
            cpf[10] = remainder < 2 ? 0 : 11 - remainder;

            return $"{cpf[0]}{cpf[1]}{cpf[2]}.{cpf[3]}{cpf[4]}{cpf[5]}.{cpf[6]}{cpf[7]}{cpf[8]}-{cpf[9]}{cpf[10]}";
        }
    }
}
