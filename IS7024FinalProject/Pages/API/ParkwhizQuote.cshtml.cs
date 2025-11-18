namespace IS7024FinalProject.Pages.API;

public class ParkwhizQuoteModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        //https://developer.parkwhiz.com/v4/#price-quotes-and-locations

        var client = new HttpClient();
        const string requestUrl =
           "https://api.parkwhiz.com/v4/quotes" +
           "?q=coordinates:39.103100,-84.512000" +
           "&start_time=2025-11-04T12:00:28-06:00" +
           "&end_time=2025-11-04T23:00:44-06:00" +
           "&returns=offstreet_bookable";

        HttpResponseMessage response;
        //Catching Http
        try
        {
            response = await _httpClient.GetAsync(requestUrl);
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, $"Error connecting to Parkwhiz API: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Error fetching data from Parkwhiz API.");
        }

        var json = await response.Content.ReadAsStringAsync();

        // Example: deserialize into a strongly typed model if available
        // var quotes = JsonSerializer.Deserialize<QuoteResponse>(json);

        // For now, return raw JSON
        return Content(json, "application/json");
    }
}
}

