
using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomRange : SoftDeletableEntity
{
    public int BrandId { get; set; }
    public ShowroomBrand Brand { get; set; } = null!;

    public string Slug { get; set; } = null!;
    public string NameFr { get; set; } = null!;
    public string? DescriptionFr { get; set; }
    public string? ImagePath { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }

    public ICollection<ShowroomProduct> Products { get; set; }
        = new List<ShowroomProduct>();
}
