using System.Text.Json.Serialization;

namespace IS7024FinalProject.Models;

public class GetEventResponse
{
    [JsonPropertyName("location_id")]
    public string LocationId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("distance")]
    public ParkWhizDistance Distance { get; set; } = new ParkWhizDistance();

    [JsonPropertyName("purchase_options")]
    public List<PurchaseOption> PurchaseOptions { get; set; } = new List<PurchaseOption>();
}
