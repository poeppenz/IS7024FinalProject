using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Web; // Needed for HttpUtility.UrlEncode
using IS7024FinalProject.DTOs; // New: Reference the shared DTOs

namespace IS7024FinalProject.Pages
{
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

        // Uses the shared Event DTO
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
                    
                    // Uses the shared SeatGeekResponse DTO
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