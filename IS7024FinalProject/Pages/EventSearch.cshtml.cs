namespace IS7024FinalProject.Pages;

// --- Razor Page Model ---
public class EventSearchModel : PageModel
{
    private IAPIService _apiService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public EventSearchModel(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiService = new APIService(_httpClient, _configuration);
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

        var (events, errorMessage) = await _apiService.SearchEventsAsync(Query);

        Events = events;

        if (!string.IsNullOrEmpty(errorMessage))
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}