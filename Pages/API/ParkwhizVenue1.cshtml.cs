using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace IS7024FinalProject.Pages.API
{
    public class ParkwhizVenue1Model : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParkwhizVenue1Model(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Accept coordinates via query string, e.g. /API/ParkwhizVenue1?Lat=39.1031&Lon=-84.5120
        [BindProperty(SupportsGet = true)]
        public double? Lat { get; set; }

        [BindProperty(SupportsGet = true)]
        public double? Lon { get; set; }

        // Exposed to the view
        public List<VenueElement> Venues { get; private set; } = new();

        // The final request URL used (helpful for debugging)
        public string RequestUrl { get; private set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            // default coordinates if none provided
            var lat = Lat ?? 39.103100;
            var lon = Lon ?? -84.512000;

            // Build the Parkwhiz "q" value as "coordinates:lat,lon"
            var qValue = $"coordinates:{lat:F6},{lon:F6}";

            // Build request URL using QueryHelpers to ensure proper encoding
            var baseUrl = "https://api.parkwhiz.com/v4/venues";
            var query = new Dictionary<string, string?>()
            {
                ["q"] = qValue
            };

            RequestUrl = QueryHelpers.AddQueryString(baseUrl, query);

            try
            {
                var client = _httpClientFactory.CreateClient();
                var json = await client.GetStringAsync(RequestUrl);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Deserialize to the schema-defined array of venues
                var venues = await client.GetFromJsonAsync<List<VenueElement>>(RequestUrl, options);

                Venues = venues ?? new List<VenueElement>();


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

    // DTOs (same as schema)
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