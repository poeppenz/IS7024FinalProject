using System.Text.Json.Serialization;

namespace IS7024FinalProject.DTOs
{
    // --- DTOs for SeatGeek API Response ---
    // These models define the structure of the JSON data we expect from the API.

    public class SeatGeekResponse
    {
        [JsonPropertyName("events")]
        public List<Event> Events { get; set; } = new List<Event>();
    }

    public class Performer
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string? Image { get; set; }

        // Helper property to safely return the image URL or a placeholder
        public string DisplayImageUrl => string.IsNullOrEmpty(Image) 
            ? "https://placehold.co/400x150/e9ecef/495057?text=No+Image" 
            : Image;
    }

    public class Location
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }

    public class Venue
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name_v2")]
        public string NameV2 { get; set; } = string.Empty;

        [JsonPropertyName("display_location")]
        public string DisplayLocation { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public Location Location { get; set; } = new Location();
    }

    public class Event
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("datetime_local")]
        public DateTime DatetimeLocal { get; set; }

        [JsonPropertyName("datetime_utc")]
        public DateTime DatetimeUtc { get; set; }

        [JsonPropertyName("enddatetime_utc")]
        public DateTime? EndDatetimeUtc { get; set; }

        [JsonPropertyName("venue")]
        public Venue Venue { get; set; } = new Venue();

        [JsonPropertyName("performers")]
        public List<Performer> Performers { get; set; } = new List<Performer>();

        // Helper to generate a summary of the main performers/teams
        public string GetPerformersSummary()
        {
            var names = Performers.Select(p => p.Name).Distinct().Take(3).ToList();
            if (names.Count == 0) return "Various Performers";

            if (names.Count > 2)
            {
                return $"{names[0]} vs. {names[1]}";
            }
            return string.Join(" and ", names);
        }
        
        // Helper to get the image URL of the primary performer (the first one in the list)
        public string GetPrimaryPerformerImageUrl()
        {
            // If the list is null or empty, use a generic placeholder
            if (Performers == null || Performers.Count == 0)
            {
                return "https://placehold.co/400x150/e9ecef/495057?text=No+Image";
            }
            // Use the DisplayImageUrl property from the first performer
            return Performers[0].DisplayImageUrl;
        }
    }
}