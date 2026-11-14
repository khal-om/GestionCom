using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomProductOption : BaseEntity
{
    public string OptionType { get; set; } = null!;
    public string ValueFr { get; set; } = null!;
    public string NormalizedValue { get; set; } = null!;
}