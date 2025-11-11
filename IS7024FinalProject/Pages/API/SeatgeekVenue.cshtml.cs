namespace IS7024FinalProject.Pages.API;

public class SeatgeekVenueModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly string _seatGeekClientId;

    public SeatgeekVenueModel(IConfiguration configuration)
    {
        _configuration = configuration;
        _seatGeekClientId = _configuration["SeatGeek:ClientId"] ??
            throw new InvalidOperationException("SeatGeek:ClientId is not configured. Please set this value in appsettings.json or environment variables.");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        //https://publicapi.dev/seat-geek-api

        var client = new HttpClient();
        var requestUrl = $"https://api.seatgeek.com/2/venues?city=cincinnati&client_id={_seatGeekClientId}";

        var response = await client.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Error fetching data from Seatgeek API.");
        }

        var json = await response.Content.ReadAsStringAsync();
        //ViewData["SeatgeekJson"] = json;

        // Optionally, deserialize the JSON if you want to work with the data in C#
        // var quotes = JsonSerializer.Deserialize<YourQuoteType>(json);

        // For now, just return the raw JSON as content
        return Content(json, "application/json");
    }
}
