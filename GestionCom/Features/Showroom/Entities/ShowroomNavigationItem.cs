
using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomNavigationItem : BaseEntity
{
    public string Key { get; set; } = null!;
    public string LabelFr { get; set; } = null!;
    public string Url { get; set; } = null!;
    public bool IsVisible { get; set; } = true;
    public bool OpenInNewTab { get; set; }
    public int SortOrder { get; set; }
}
