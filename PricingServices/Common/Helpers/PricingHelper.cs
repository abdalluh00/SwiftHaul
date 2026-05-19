namespace PricingServices.Common.Helpers
{
   

    public class PricingHelper
    {
        private readonly IConfiguration _config;

        // Saudi cities grouped by distance zone
        private static readonly Dictionary<string, int> CityZones = new()
    {
        { "Riyadh", 1 },
        { "Dammam", 1 },
        { "Khobar", 1 },
        { "Jeddah", 2 },
        { "Mecca", 2 },
        { "Medina", 2 },
        { "Tabuk", 3 },
        { "Abha", 3 }
    };

        public PricingHelper(IConfiguration config)
        {
            _config = config;
        }

        public (double total, double multiplier) Calculate(
            string pickupCity,
            string deliveryCity,
            double weightKg)
        {
            var rules = _config.GetSection("PricingRules");
            var basePricePerKg = double.Parse(rules["BasePricePerKg"]!);

            var multiplier = GetMultiplier(pickupCity, deliveryCity, rules);
            var total = Math.Round(weightKg * basePricePerKg * multiplier, 2);

            return (total, multiplier);
        }

        public double GetBasePricePerKg()
        {
            return double.Parse(_config.GetSection("PricingRules")["BasePricePerKg"]!);
        }

        private static double GetMultiplier(
            string pickup,
            string delivery,
            IConfigurationSection rules)
        {
            // Same city
            if (pickup == delivery)
                return double.Parse(rules["SameCityMultiplier"]!);

            // Get zones
            CityZones.TryGetValue(pickup, out var pickupZone);
            CityZones.TryGetValue(delivery, out var deliveryZone);

            var zoneDiff = Math.Abs(pickupZone - deliveryZone);

            return zoneDiff switch
            {
                0 => double.Parse(rules["NearCityMultiplier"]!),
                1 => double.Parse(rules["NearCityMultiplier"]!),
                _ => double.Parse(rules["FarCityMultiplier"]!)
            };
        }
    }

    public static class InvoiceNumberHelper
    {
        public static string Generate()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = Random.Shared.Next(10000, 99999);
            return $"INV-{date}-{random}";
        }
    }
}
