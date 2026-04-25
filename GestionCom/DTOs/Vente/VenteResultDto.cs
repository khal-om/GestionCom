namespace GestionCom.DTOs.Vente
{
    public class VenteResultDto
    {
        public int VenteId { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
