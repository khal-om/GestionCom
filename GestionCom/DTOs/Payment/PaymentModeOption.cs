namespace GestionCom.DTOs.Payment
{
    public static class PaymentModeOption
    {
        public const string Especes = "Espèces";
        public const string Virement = "Virement";
        public const string Cheque = "Chèque";
        public const string Traite = "Traite";

        public static readonly string[] All =
        [
            Especes,
            Virement,
            Cheque,
            Traite
        ];
    }
}
