using System.Text.Json;
using System.Web;
using System.Globalization;
using IS7024FinalProject.DTOs; // Now includes both SeatGeek and ParkWhiz DTOs

namespace IS7024FinalProject.Pages;

// --- Razor Page Model ---
public class ParkingSearchModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _seatGeekClientId;
    private readonly string _parkWhizApiKey; // ParkWhiz uses API Key

    public ParkingSearchModel(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Get API Keys from configuration
        _seatGeekClientId = _configuration["SeatGeek:ClientId"] ?? 
                            throw new InvalidOperationException("SeatGeek:ClientId is not configured.");
        
        // MODIFIED: Make ParkWhiz API key retrieval optional based on your requirement. 
        // It will be an empty string if not found, and we will conditionally add the header later.
        _parkWhizApiKey = _configuration["ParkWhiz:ApiKey"] ?? string.Empty;
    }

    // Parameters passed from EventSearch.cshtml
    [BindProperty(SupportsGet = true)]
    public long EventId { get; set; }
    
    // REMOVED: Lat and Lon properties are no longer needed as query parameters.

    // Data to display - Uses the shared Event DTO
    public Event EventDetails { get; set; } = new Event();
    // Uses the shared ParkWhizQuote DTO
    public List<ParkWhizQuote> ParkingQuotes { get; set; } = new List<ParkWhizQuote>();
    public bool ParkingSearchPerformed { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;


    public async Task OnGetAsync()
    {
        // UPDATED VALIDATION: Check only for EventId
        if (EventId == 0)
        {
            ErrorMessage = "Missing required event ID.";
            return;
        }

        // --- 1. Fetch Event Details (Required to get Lat/Lon) ---
        try
        {
            var eventApiUrl = $"https://api.seatgeek.com/2/events/{EventId}?client_id={_seatGeekClientId}";
            var response = await _httpClient.GetAsync(eventApiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                var eventDetails = JsonSerializer.Deserialize<Event>(jsonContent, options);
                if (eventDetails != null)
                {
                    EventDetails = eventDetails;
                    
                    // ADDITIONAL VALIDATION: Ensure location data is available
                    if (EventDetails.Venue?.Location == null)
                    {
                        ErrorMessage = "Event details loaded, but missing venue location data (Lat/Lon). Cannot search for parking.";
                        return;
                    }
                }
                else
                {
                    ErrorMessage = "Could not deserialize SeatGeek event details.";
                    return;
                }
            }
            else
            {
                ErrorMessage = $"Could not load event details from SeatGeek (Status: {response.StatusCode}).";
                return;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred fetching event details: {ex.Message}";
            return;
        }

        // --- 2. Fetch Parking Quotes from ParkWhiz ---
        ParkingSearchPerformed = true;

        // NEW: Get coordinates from the fetched Event Details
        var lat = EventDetails.Venue.Location.Lat;
        var lon = EventDetails.Venue.Location.Lon;
        
        // Use UTC times from the Event DTO for reliable calculation and ParkWhiz API compatibility.
        // Start 1 hour before the event's UTC start time to allow time to arrive and park.
        var parkingStartTimeUtc = EventDetails.DatetimeUtc.AddHours(-1);
        
        // Determine the end time for parking.
        DateTime parkingEndTimeUtc;
        if (EventDetails.EndDatetimeUtc.HasValue)
        {
            // If the event end time is provided, use it and add 2 hours for time to exit the lot.
            parkingEndTimeUtc = EventDetails.EndDatetimeUtc.Value.AddHours(2);
        }
        else
        {
            // Default to a 4-hour parking window if the end time isn't specified (1hr before + 3hrs during/after event).
            parkingEndTimeUtc = parkingStartTimeUtc.AddHours(4);
        }
        
        // ParkWhiz API requires ISO 8601 UTC format, ending in 'Z' (Zulu time).
        var startTimeFormatted = parkingStartTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        var endTimeFormatted = parkingEndTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        
        // Construct the ParkWhiz URL using the coordinates retrieved from EventDetails
        var parkWhizUrl = $"http://api.parkwhiz.com/v4/quotes?q=coordinates:{lat},{lon}&start_time={HttpUtility.UrlEncode(startTimeFormatted)}&end_time={HttpUtility.UrlEncode(endTimeFormatted)}&returns=offstreet_bookable";
        
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, parkWhizUrl);

            // CONDITIONALLY ADD HEADER: Only add the X-Api-Key if the key is available.
            if (!string.IsNullOrEmpty(_parkWhizApiKey))
            {
                request.Headers.Add("X-Api-Key", _parkWhizApiKey);
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                
                // ParkWhiz returns an array of quotes
                var quotes = JsonSerializer.Deserialize<List<ParkWhizQuote>>(jsonContent, options);
                
                if (quotes != null)
                {
                    // Filter out quotes with no available purchase options (or price will be 0)
                    ParkingQuotes = quotes.Where(q => q.PurchaseOptions.Count > 0)
                                          .OrderBy(q => q.MinPrice) // Sort by cheapest
                                          .ToList();
                }
            }
            else
            {
                // Log the error but don't prevent page load if event details worked
                ErrorMessage = $"Could not load parking quotes from ParkWhiz (Status: {response.StatusCode}).";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred fetching parking quotes: {ex.Message}";
        }
    }
}