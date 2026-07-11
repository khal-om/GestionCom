namespace GestionCom.DTOs.Client
{
    public class ClientDetailDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Profession { get; set; }
        public string? MatriculeFiscale { get; set; }

        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Portable { get; set; }
        public string? Fax { get; set; }

        public string? Adresse { get; set; }
        public string? CodePostal { get; set; }
        public string? Ville { get; set; }
        public string? Pays { get; set; }

        public string? Banque { get; set; }
        public string? RIB { get; set; }

        public string? Note { get; set; }

        public decimal TotalSpent { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal Debt { get; set; }

        public List<ClientLedgerItemDto> Ledger { get; set; } = new();
        public List<ClientPaymentItemDto> Payments { get; set; } = new();
        public List<ClientVenteItemDto> Ventes { get; set; } = new();

    }
}
