using GestionCom.Entities;
namespace GestionCom.Features.Showroom.Entities;
public class ShowroomOrderItem : BaseEntity
{
 public int ShowroomOrderId { get; set; }
 public ShowroomOrder Order { get; set; } = null!;
 public int? ShowroomProductId { get; set; }
 public ShowroomProduct? Product { get; set; }
 public string ProductNameFr { get; set; } = null!;
 public string BrandNameFr { get; set; } = null!;
 public string? Size { get; set; }
 public decimal UnitPriceTnd { get; set; }
 public int Quantity { get; set; }
}