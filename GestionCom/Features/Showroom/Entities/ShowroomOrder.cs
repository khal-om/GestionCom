using GestionCom.Entities;
namespace GestionCom.Features.Showroom.Entities;
public class ShowroomOrder : BaseEntity
{
 public string OrderNumber { get; set; } = null!;
 public string CustomerName { get; set; } = null!;
 public string Email { get; set; } = null!;
 public string Phone { get; set; } = null!;
 public string Address { get; set; } = null!;
 public string PostalCode { get; set; } = null!;
 public string City { get; set; } = null!;
 public string Country { get; set; } = null!;
 public string Status { get; set; } = "Nouvelle";
 public string? AdminNote { get; set; }
 public decimal TotalTnd { get; set; }
 public ICollection<ShowroomOrderItem> Items { get; set; } = new List<ShowroomOrderItem>();
}