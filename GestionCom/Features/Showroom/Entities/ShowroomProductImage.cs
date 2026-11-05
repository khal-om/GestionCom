using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomProductImage : BaseEntity
{
    public int ProductId { get; set; }
    public ShowroomProduct Product { get; set; } = null!;
    public string ImagePath { get; set; } = null!;
    public string? AltTextFr { get; set; }
    public string? AltTextAr { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
