namespace IS7024FinalProject.Pages.API;

public class ParkingSearchModel : PageModel
{
    private IAPIService _apiService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    // Inject the API service (ensure IApiService is registered in DI)
    public ParkingSearchModel(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiService = new APIService(_httpClient, _configuration);
    }

    // Example: GET /API/ParkingSearch?q=...
    public async Task<IActionResult> OnGetAsync([FromQuery(Name = "q")] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "Query parameter 'q' is required." });
        }

        try
        {
            var result = await _apiService.SearchParkingAsync(q);
            // Return raw result as JSON. Adjust mapping if your service returns a domain type.
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            // Log exception as needed (not shown here).
            Response.StatusCode = 500;
            return new JsonResult(new { error = "An error occurred while searching parking.", detail = ex.Message });
        }
    }
}
