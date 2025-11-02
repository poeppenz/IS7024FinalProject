using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IS7024FinalProject.Pages.API
{
    public class ParkwhizVenueModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParkwhizVenueModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public List<VenueElement> Venues { get; private set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var requestUrl = "https://api.parkwhiz.com/v4/venues?&q=coordinates:39.103100,-84.512000";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var json = await client.GetStringAsync(requestUrl);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // The schema indicates the response is an array of VenueElement
                var venues = JsonSerializer.Deserialize<List<VenueElement>>(json, options);
                if (venues != null)
                {
                    Venues = venues;
                }

                return Page();
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Unable to reach Parkwhiz API.");
            }
            catch (JsonException)
            {
                return StatusCode(500, "Invalid JSON returned from Parkwhiz API.");
            }
            catch (Exception)
            {
                return StatusCode(500, "Error fetching data from Parkwhiz API.");
            }
        }
    }

    // DTOs generated from the provided schema (minimal fields used in the table)
    public class VenueElement
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address1")]
        public string? Address1 { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("distance")]
        public Distance? Distance { get; set; }
    }

    public class Distance
    {
        [JsonPropertyName("meters")]
        public double Meters { get; set; }

        [JsonPropertyName("miles")]
        public double Miles { get; set; }
    }
}