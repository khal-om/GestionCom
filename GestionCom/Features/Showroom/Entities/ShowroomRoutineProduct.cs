using GestionCom.Entities;

namespace GestionCom.Features.Showroom.Entities;

public class ShowroomRoutineProduct : BaseEntity
{
    public int RoutineId { get; set; }
    public ShowroomRoutine Routine { get; set; } = null!;
    public int ProductId { get; set; }
    public ShowroomProduct Product { get; set; } = null!;
    public int SortOrder { get; set; }
    public string? StepLabelFr { get; set; }
    public string? StepLabelAr { get; set; }
}
