namespace IS7024FinalProject.Pages.API;

public class SeatgeekEventModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly string _seatGeekClientId;

    public SeatgeekEventModel(IConfiguration configuration)
    {
        _configuration = configuration;
        _seatGeekClientId = _configuration["SeatGeek:ClientId"] ??
            throw new InvalidOperationException("SeatGeek:ClientId is not configured. Please set this value in appsettings.json or environment variables.");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var client = new HttpClient();
        var requestUrl = $"https://api.seatgeek.com/2/events?q=cincinnati&client_id={_seatGeekClientId}";

        var response = await client.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Error fetching data from Seatgeek API.");
        }

        var json = await response.Content.ReadAsStringAsync();
        return Content(json, "application/json");
    }
}