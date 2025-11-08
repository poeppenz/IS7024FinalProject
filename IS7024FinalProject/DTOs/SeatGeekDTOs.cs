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

        // NEW: UTC start time for accurate API calls
        [JsonPropertyName("datetime_utc")]
        public DateTime DatetimeUtc { get; set; }

        // NEW: Optional UTC end time for more accurate parking exit time
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
    }
}