
using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomProduct : SoftDeletableEntity
{
    public int BrandId { get; set; }
    public ShowroomBrand Brand { get; set; } = null!;

    public int? RangeId { get; set; }
    public ShowroomRange? Range { get; set; }

    public string Slug { get; set; } = null!;
    public string NameFr { get; set; } = null!;
    public string? CategoryFr { get; set; }
    public string? HairNeedFr { get; set; }
    public string? ShortDescriptionFr { get; set; }
    public string? DescriptionFr { get; set; }
    public string? HairTypeFr { get; set; }
    public string? HowToUseFr { get; set; }
    public string? IngredientsFr { get; set; }

    public string? Size { get; set; }
    public decimal PriceTnd { get; set; }
    public string? AvailabilityFr { get; set; }

    public bool IsAvailable { get; set; } = true;
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public int SortOrder { get; set; }

    public ICollection<ShowroomProductImage> Images { get; set; }
        = new List<ShowroomProductImage>();

    public ICollection<ShowroomRoutineProduct> RoutineLinks { get; set; }
        = new List<ShowroomRoutineProduct>();
}
