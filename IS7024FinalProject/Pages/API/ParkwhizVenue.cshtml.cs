using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;

namespace IS7024FinalProject.Pages.API
{
    public class ParkwhizModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParkwhizModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            //https://developer.parkwhiz.com/v4/#price-quotes-and-locations

            var client = _httpClientFactory.CreateClient();
            var requestUrl = "https://api.parkwhiz.com/v4/venues?&q=coordinates:39.103100,-84.512000";

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
}
