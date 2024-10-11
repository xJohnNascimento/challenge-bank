using StarkBank.Domain.Interfaces.Shared;

namespace StarkBank.Shared.Services
{
    public class ClientGeneratorService : IClientGeneratorService
    {
        private static readonly Random Random = new();

        #region Names
        private static readonly List<string> FirstNames =
        [
            "John",
            "Arya",
            "Sophia",
            "Michael",
            "Emily",
            "James",
            "Olivia",
            "Daniel",
            "Emma",
            "Liam",
            "Charlotte",
            "Benjamin",
            "Lucas",
            "Amelia",
            "Alexander",
            "Isabella",
            "Mason",
            "Mia",
            "Ethan",
            "Harper",
            "Ava",
            "Noah",
            "Lily",
            "Matthew",
            "Grace",
            "David",
            "Hannah",
            "Oliver",
            "Chloe",
            "Jack",
            "Zoe",
            "Isaac",
            "Ella",
            "Elijah",
            "Madison",
            "Gabriel",
            "Scarlett",
            "Henry",
            "Abigail",
            "Logan",
            "Samuel",
            "Victoria",
            "Sebastian",
            "Sofia",
            "Mila",
            "Jackson",
            "Layla",
            "Levi",
            "Penelope",
            "Owen",
            "Aria",
            "Dylan",
            "Avery",
            "Joseph",
            "Ella",
            "Eleanor",
            "Ryan",
            "Riley",
            "Caleb",
            "Aurora",
            "Wyatt",
            "Brooklyn",
            "Nathan",
            "Savannah",
            "Carter",
            "Samantha",
            "Asher",
            "Aubrey",
            "Hunter",
            "Addison",
            "Julian",
            "Ellie",
            "Lincoln",
            "Stella",
            "Isaiah",
            "Nora",
            "Charles",
            "Hazel",
            "Aaron",
            "Lucy",
            "Ezra",
            "Paisley",
            "Thomas",
            "Luna",
            "Christian",
            "Zara",
            "Connor",
            "Bella",
            "Andrew",
            "Leah",
            "Joshua",
            "Willow",
            "Eli",
            "Nova",
            "Christopher",
            "Violet",
            "Dominic",
            "Aurora",
            "Adam",
            "Alice"
        ];

        private static readonly List<string> LastNames =
        [
            "Stark",
            "Smith",
            "Johnson",
            "Williams",
            "Brown",
            "Jones",
            "Garcia",
            "Miller",
            "Davis",
            "Martinez",
            "Rodriguez",
            "Martins",
            "Hernandez",
            "Lopez",
            "Gonzalez",
            "Wilson",
            "Anderson",
            "Thomas",
            "Taylor",
            "Moore",
            "Jackson",
            "Martin",
            "Lee",
            "Perez",
            "Thompson",
            "White",
            "Harris",
            "Sanchez",
            "Clark",
            "Ramirez",
            "Lewis",
            "Robinson",
            "Walker",
            "Young",
            "Allen",
            "King",
            "Wright",
            "Scott",
            "Torres",
            "Nguyen",
            "Hill",
            "Flores",
            "Green",
            "Adams",
            "Nelson",
            "Baker",
            "Hall",
            "Rivera",
            "Campbell",
            "Mitchell",
            "Carter",
            "Roberts",
            "Gomez",
            "Phillips",
            "Evans",
            "Turner",
            "Diaz",
            "Parker",
            "Cruz",
            "Edwards",
            "Collins",
            "Reyes",
            "Stewart",
            "Morris",
            "Morales",
            "Murphy",
            "Cook",
            "Rogers",
            "Morgan",
            "Peterson",
            "Cooper",
            "Reed",
            "Bailey",
            "Bell",
            "Gomez",
            "Kelly",
            "Howard",
            "Ward",
            "Cox",
            "Diaz",
            "Richardson",
            "Wood",
            "Watson",
            "Brooks",
            "Bennett",
            "Gray",
            "James",
            "Mendoza",
            "Hughes",
            "Castillo",
            "Sanders",
            "Ross",
            "Henderson",
            "Coleman",
            "Jenkins",
            "Perry",
            "Powell",
            "Long",
            "Patterson",
            "Hughes"
        ];

        #endregion

        public string GenerateName()
        {
            var firstName = FirstNames[Random.Next(FirstNames.Count)];
            var lastName = LastNames[Random.Next(LastNames.Count)];

            return $"{firstName} {lastName}";
        }

        public string GenerateCpf()
        {
            var soma = 0;

            var cpf = new int[11];
            for (var i = 0; i < 9; i++)
            {
                cpf[i] = Random.Next(0, 10);
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
