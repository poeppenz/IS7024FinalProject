namespace IS7024FinalProject.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IAPIService _apiService;

    // Inject the API Service
    public IndexModel(ILogger<IndexModel> logger, IAPIService apiService)
    {
        _logger = logger;
        _apiService = apiService;
    }

    public void OnGet()
    {
        // Standard page load logic
    }

    // Handler for the Autocomplete functionality (called via AJAX/fetch)
    public async Task<JsonResult> OnGetAutocomplete(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return new JsonResult(new List<Event>());
        }

        // 1. Call the main search method
        var (allEvents, errorMessage) = await _apiService.SearchEventsAsync(term);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            // Log the error but return an empty list for the UI
            _logger.LogError("API Error during Autocomplete for query '{Term}': {Message}", term, errorMessage);
            return new JsonResult(new List<Event>());
        }

        // 2. Apply the hard limit (5) for autocomplete results 
        var topEvents = allEvents.Take(5).ToList();

        // 3. Return the results as JSON
        // We project the list to an anonymous type containing all fields required by the client JS
        var result = topEvents.Select(e => new
        {
            id = e.Id,
            title = e.Title,
            venue = e.Venue.NameV2,
            date = e.DatetimeLocal.ToString("MMM dd, yyyy"),
            location = e.Venue.DisplayLocation
        });

        return new JsonResult(result);
    }
}