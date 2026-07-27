namespace GestionCom.Features.Savings
{
    public static class Reactions
    {
        public static readonly string[] Zero =
        [
            "Today the cigarette won.",
        "The tobacco company sends its thanks.",
        "Tomorrow we fight again.",
        "0€. Bold strategy.",
        "Coffee 1 - 0 Wallet."
        ];

        public static readonly string[] Small = // 0.01 - 4.99
        [
            "Every euro counts.",
        "Small damage to the cigarette industry.",
        "Not impressive... but acceptable.",
        "A tiny paper cut on Big Tobacco.",
        "Baby steps. Literally, baby steps."
        ];

        public static readonly string[] Medium = // 5 - 9.99
        [
            "Solid. The vending machine misses you.",
        "Your lungs noticed that one.",
        "The coffee shop is asking questions.",
        "That's a decent dent."
        ];

        public static readonly string[] Large = // 10+
        [
            "The tobacco companies are having a bad day.",
        "Your lungs just received a salary increase.",
        "Another victory against cigarettes.",
        "Absolute unit of a savings day.",
        "Someone's getting promoted at the Bank of Not Smoking."
        ];

        public static string Pick(decimal amount, string? lastShown)
        {
            var pool = amount switch
            {
                0m => Zero,
                < 5m => Small,
                < 10m => Medium,
                _ => Large
            };

            var choices = pool.Where(q => q != lastShown).ToArray();
            if (choices.Length == 0) choices = pool;

            return choices[Random.Shared.Next(choices.Length)];
        }
    }
}
