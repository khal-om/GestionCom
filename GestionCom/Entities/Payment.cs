namespace GestionCom.Entities
{
    public class Payment : SoftDeletableEntity
    {
            public int ClientId { get; set; }
            public Client Client { get; set; } = null!;

            // Optional link to a specific vente this payment applies to
            public int? VenteId { get; set; }
            public Vente? Vente { get; set; }

            public decimal Amount { get; set; }

            public string PaymentMode { get; set; } = null!;

            public string? PaymentNumber { get; set; }
            public DateTime? PaymentDate { get; set; }
            public DateTime? DueDate { get; set; }
            public bool IsReceived { get; set; } = true;
            public int? CalendarEventId { get; set; }

            public string? Note { get; set; }
        }
    }
