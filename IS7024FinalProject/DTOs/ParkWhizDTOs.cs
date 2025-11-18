using System.Text.Json.Serialization;

namespace IS7024FinalProject.DTOs
{
    // --- DTOs for ParkWhiz API Response (based on parkwhiz_quote.json) ---

    // DTO for the specific "gallery" size URL
    public class GallerySize
    {
        [JsonPropertyName("URL")]
        public string Url { get; set; } = string.Empty;
    }

    // DTO for all image sizes
    public class Sizes
    {
        [JsonPropertyName("gallery")]
        public GallerySize? Gallery { get; set; }
    }

    // DTO for a single photo entry
    public class Photo
    {
        [JsonPropertyName("sizes")]
        public Sizes? Sizes { get; set; }
    }

    // DTO for the nested location details inside "_embedded"
    public class PwLocation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("address1")]
        public string Address1 { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("photos")]
        public List<Photo> Photos { get; set; } = new();
    }

    // DTO for the "_embedded" container
    public class ParkWhizEmbedded
    {
        [JsonPropertyName("pw:location")]
        public PwLocation? Location { get; set; }
    }

    public class ParkWhizQuote
    {
        [JsonPropertyName("location_id")]
        public string LocationId { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("distance")]
        public ParkWhizDistance Distance { get; set; } = new();

        [JsonPropertyName("purchase_options")]
        public List<PurchaseOption> PurchaseOptions { get; set; } = new();

        [JsonPropertyName("_embedded")]
        public ParkWhizEmbedded Embedded { get; set; } = new();

        // --- Calculated Properties for Easy Access and Display ---

        public string DisplayLocationName => Embedded.Location?.Name ?? string.Empty;

        public string DisplayStreetAddress
        {
            get
            {
                if (Embedded.Location == null) return string.Empty;

                var parts = new List<string>
                {
                    Embedded.Location.Address1,
                    Embedded.Location.City,
                    Embedded.Location.State
                };

                parts.RemoveAll(string.IsNullOrWhiteSpace);
                return string.Join(", ", parts);
            }
        }

        public string DisplayImageUrl =>
            Embedded.Location?.Photos.FirstOrDefault()?.Sizes?.Gallery?.Url
            ?? "https://placehold.co/400x300/e9ecef/495057?text=Parking+Image+N%2FA";

        public decimal MinPrice =>
            PurchaseOptions.Count > 0
                ? PurchaseOptions.Min(p => decimal.TryParse(p.Price.USD, out var price) ? price : decimal.MaxValue)
                : 0;
    }

    public class ParkWhizDistance
    {
        [JsonPropertyName("straight_line")]
        public ParkWhizStraightLine StraightLine { get; set; } = new();
    }

    public class ParkWhizStraightLine
    {
        [JsonPropertyName("feet")]
        public int Feet { get; set; }
    }

    public class PurchaseOption
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.Empty;

        [JsonPropertyName("price")]
        public ParkWhizPrice Price { get; set; } = new();
    }

    public class ParkWhizPrice
    {
        [JsonPropertyName("USD")]
        public string USD { get; set; } = string.Empty;
    }
}
