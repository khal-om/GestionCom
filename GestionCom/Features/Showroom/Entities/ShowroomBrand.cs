
using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomBrand : SoftDeletableEntity
{
    public string Slug { get; set; } = null!;
    public string NameFr { get; set; } = null!;
    public string? TaglineFr { get; set; }
    public string? DescriptionFr { get; set; }
    public string? LogoImagePath { get; set; }
    public string? HeroImagePath { get; set; }
    public string? AccentColor { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }

    public ICollection<ShowroomRange> Ranges { get; set; }
        = new List<ShowroomRange>();

    public ICollection<ShowroomProduct> Products { get; set; }
        = new List<ShowroomProduct>();
}
