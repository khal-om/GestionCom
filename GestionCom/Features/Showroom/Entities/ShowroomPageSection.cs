using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

/// <summary>Editable content for a designed section of the public showroom.</summary>
public class ShowroomPageSection : BaseEntity
{
    public string PageKey { get; set; } = null!;
    public string SectionKey { get; set; } = null!;
    public string SectionType { get; set; } = null!;
    public string? TitleFr { get; set; }
    public string? TitleAr { get; set; }
    public string? SubtitleFr { get; set; }
    public string? SubtitleAr { get; set; }
    public string? BodyFr { get; set; }
    public string? BodyAr { get; set; }
    public string? ImagePath { get; set; }
    public string? MobileImagePath { get; set; }
    public string? ButtonLabelFr { get; set; }
    public string? ButtonLabelAr { get; set; }
    public string? ButtonUrl { get; set; }
    public string? SecondaryButtonLabelFr { get; set; }
    public string? SecondaryButtonLabelAr { get; set; }
    public string? SecondaryButtonUrl { get; set; }
    public string? StyleVariant { get; set; }
    public bool IsVisible { get; set; } = true;
    public int SortOrder { get; set; }
}
