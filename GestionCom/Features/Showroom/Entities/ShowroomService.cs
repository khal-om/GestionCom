
using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomService : SoftDeletableEntity
{
    public string Slug { get; set; } = null!;
    public string TitleFr { get; set; } = null!;
    public string? ShortDescriptionFr { get; set; }
    public string? DescriptionFr { get; set; }
    public string? ImagePath { get; set; }
    public string? ButtonLabelFr { get; set; }
    public string? ButtonUrl { get; set; }
    public bool IsPublished { get; set; }
    public int SortOrder { get; set; }
}
