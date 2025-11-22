namespace IS7024FinalProject.Pages;

// --- Razor Page Model ---
public class ParkingSearchModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private IAPIService _apiService;

    public ParkingSearchModel(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiService = new APIService(_httpClient, _configuration);
    }

    // Parameters passed from EventSearch.cshtml
    [BindProperty(SupportsGet = true)]
    public long EventId { get; set; }

    public Event EventDetails { get; set; } = new Event();

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

        // Returns a ValueTupletuple with event details and any error message
        var (eventDetails, eventError) = await _apiService.EventDetailsAsync(EventId.ToString());

        EventDetails = eventDetails;

        if (!string.IsNullOrEmpty(eventError))
        {
            ErrorMessage = eventError;
        }

        // --- 2. Fetch Parking Quotes from ParkWhiz ---
        if (EventDetails is not null)
        {
            ParkingSearchPerformed = true;

            // Get coordinates from the fetched Event Details
            var lat = EventDetails?.Venue?.Location?.Lat ?? 0.0;
            var lon = EventDetails?.Venue?.Location?.Lon ?? 0.0;

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

            var (quotes, parkingError) = await _apiService.SearchParkingAsync(lat, lon, parkingStartTimeUtc, parkingEndTimeUtc);
            ParkingQuotes = quotes;

            if (!string.IsNullOrEmpty(parkingError))
            {
                ErrorMessage = parkingError;
            }
        }
    }
}