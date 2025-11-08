using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web; // Needed for HttpUtility.UrlEncode

namespace IS7024FinalProject.Pages
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
    
    // --- Razor Page Model ---
    public class EventSearchModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration; // Inject configuration service
        private readonly string _seatGeekClientId;

        public EventSearchModel(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            
            // Read the Client ID from the configuration (e.g., appsettings.json or environment variable)
            _seatGeekClientId = _configuration["SeatGeek:ClientId"] ?? 
                                throw new InvalidOperationException("SeatGeek:ClientId is not configured. Please set this value in appsettings.json or environment variables.");
        }

        [BindProperty(SupportsGet = true)]
        public string Query { get; set; } = string.Empty;

        public List<Event> Events { get; set; } = new List<Event>();
        
        public bool SearchPerformed { get; set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Query))
            {
                SearchPerformed = false;
                return;
            }

            SearchPerformed = true;
            
            // 1. Construct the API URL using the configured Client ID
            var encodedQuery = HttpUtility.UrlEncode(Query);
            var apiUrl = $"https://api.seatgeek.com/2/events?q={encodedQuery}&client_id={_seatGeekClientId}";

            try
            {
                // 2. Fetch the data
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // 3. Read and deserialize the JSON content
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    
                    var seatGeekResponse = JsonSerializer.Deserialize<SeatGeekResponse>(jsonContent, options);
                    
                    if (seatGeekResponse?.Events != null)
                    {
                        Events = seatGeekResponse.Events;
                    }
                    else
                    {
                        // Handle case where API succeeds but returns no events or malformed response
                        Events = new List<Event>(); 
                    }
                }
                else
                {
                    // Handle API error status codes
                    ModelState.AddModelError(string.Empty, $"SeatGeek API returned an error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Handle general exceptions (e.g., network issues)
                ModelState.AddModelError(string.Empty, $"An error occurred while fetching events: {ex.Message}");
            }
        }
    }
}