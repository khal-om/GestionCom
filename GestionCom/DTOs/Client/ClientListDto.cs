namespace GestionCom.DTOs.Client
{
    public class ClientListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Ville { get; set; }
        public string? Telephone { get; set; }

        public decimal Debt { get; set; }
    }
}
