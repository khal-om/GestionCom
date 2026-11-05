
using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomRoutine : SoftDeletableEntity
{
    public string Slug { get; set; } = null!;
    public string TitleFr { get; set; } = null!;
    public string? DescriptionFr { get; set; }
    public string? HairNeedFr { get; set; }
    public string? ImagePath { get; set; }
    public bool IsPublished { get; set; }
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }

    public ICollection<ShowroomRoutineProduct> Products { get; set; }
        = new List<ShowroomRoutineProduct>();
}
