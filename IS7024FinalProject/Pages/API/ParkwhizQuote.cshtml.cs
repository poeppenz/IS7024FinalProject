namespace IS7024FinalProject.Pages.API;

public class ParkwhizQuoteModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        //https://developer.parkwhiz.com/v4/#price-quotes-and-locations

        var client = new HttpClient();
        var requestUrl = "https://api.parkwhiz.com/v4/quotes?&q=coordinates:39.103100,-84.512000&start_time=2025-11-04T12:00:28-06:00&end_time=2025-11-04T23:00:44-06:00&returns=offstreet_bookable";
        //                https://api.parkwhiz.com/v4/quotes/?q=coordinates:41.881943,-87.630976&start_time=2015-11-22T16:35:28-06:00&end_time=2015-11-22T19:35:44-06:00&returns=offstreet_bookable

        var response = await client.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Error fetching data from Parkwhiz API.");
        }

        var json = await response.Content.ReadAsStringAsync();
        //ViewData["ParkwhizJson"] = json;

        // Optionally, deserialize the JSON if you want to work with the data in C#
        // var quotes = JsonSerializer.Deserialize<YourQuoteType>(json);

        // For now, just return the raw JSON as content
        return Content(json, "application/json");
    }
}
