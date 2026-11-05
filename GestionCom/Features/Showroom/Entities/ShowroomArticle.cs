
using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomArticle : SoftDeletableEntity
{
    public string Slug { get; set; } = null!;
    public string TitleFr { get; set; } = null!;
    public string? ExcerptFr { get; set; }
    public string? BodyFr { get; set; }
    public string? CoverImagePath { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}
