using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

/// <summary>Editable global showroom content such as contact details, social links and footer text.</summary>
public class ShowroomSiteSetting : BaseEntity
{
    public string Key { get; set; } = null!;
    public string? ValueFr { get; set; }
    public string? ValueAr { get; set; }
}
