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
        public string Name { get; set; } = string.Empty; // Location name

        [JsonPropertyName("address1")]
        public string Address1 { get; set; } = string.Empty; // Street address
        
        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("photos")]
        public List<Photo> Photos { get; set; } = new List<Photo>(); // List of images
    }

    // DTO for the "_embedded" container
    public class ParkWhizEmbedded
    {
        [JsonPropertyName("pw:location")]
        public PwLocation? Location { get; set; } // Nullable, as it might be missing
    }

    public class ParkWhizQuote
    {
        [JsonPropertyName("location_id")]
        public string LocationId { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("distance")]
        public ParkWhizDistance Distance { get; set; } = new ParkWhizDistance();

        [JsonPropertyName("purchase_options")]
        public List<PurchaseOption> PurchaseOptions { get; set; } = new List<PurchaseOption>();

        // New field to capture the nested embedded data
        [JsonPropertyName("_embedded")]
        public ParkWhizEmbedded Embedded { get; set; } = new ParkWhizEmbedded();

        // --- Calculated Properties for Easy Access and Display ---
        
        public string DisplayLocationName => Embedded.Location?.Name ?? string.Empty;
        
        // Construct the full address from the nested fields
        public string DisplayStreetAddress
        {
            get
            {
                if (Embedded.Location == null) return string.Empty;

                // Concatenate Address1, City, and State for a complete address string
                var parts = new List<string> { Embedded.Location.Address1, Embedded.Location.City };
                
                // Filter out empty parts before joining
                parts.RemoveAll(string.IsNullOrWhiteSpace);
                
                return string.Join(", ", parts);
            }
        }

        // NEW: Property to grab the URL for the first photo's "gallery" size
        public string DisplayImageUrl
        {
            get
            {
                // Try to get the URL from the first photo
                var imageUrl = Embedded.Location?.Photos.FirstOrDefault()?.Sizes?.Gallery?.Url;

                // Return the URL or a styled placeholder if no image is available
                // Using a standard placeholder image URL as a string fallback (400x300 for card)
                return imageUrl ?? "https://placehold.co/400x300/e9ecef/495057?text=Parking+Image+N%2FA";
            }
        }

        // MinPrice logic remains the same
        public decimal MinPrice => PurchaseOptions.Count > 0 ? 
            PurchaseOptions.Min(p => decimal.TryParse(p.Price.USD, out var price) ? price : decimal.MaxValue) : 0;
    }

    public class ParkWhizDistance
    {
        [JsonPropertyName("straight_line")]
        public ParkWhizStraightLine StraightLine { get; set; } = new ParkWhizStraightLine();
    }

    public class ParkWhizStraightLine
    {
        [JsonPropertyName("feet")]
        public int Feet { get; set; }
    }

    public class PurchaseOption
    {
        [JsonPropertyName("Id")]
        public Guid Id { get; set; } = Guid.Empty;

        [JsonPropertyName("price")]
        public ParkWhizPrice Price { get; set; } = new ParkWhizPrice();
    }

    public class ParkWhizPrice
    {
        [JsonPropertyName("USD")]
        public string USD { get; set; } = string.Empty;
    }
}