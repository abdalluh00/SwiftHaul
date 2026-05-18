namespace ShipmentService.Common.Helpers
{
    public static class TrackingNumberHelper
    {
        public static string Generate()
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = Random.Shared.Next(10000, 99999);
            return $"SH-{date}-{random}";
        }
    }
}
