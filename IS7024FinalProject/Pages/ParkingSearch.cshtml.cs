using System.Text.Json;
using System.Globalization;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IS7024FinalProject.DTOs;

namespace IS7024FinalProject.Pages
{
    // Razor Page Model for Parking Search
    public class ParkingSearchModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _seatGeekClientId;
        private readonly string _parkWhizApiKey;

        public ParkingSearchModel(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _seatGeekClientId = _configuration["SeatGeek:ClientId"]
                                ?? throw new InvalidOperationException("SeatGeek:ClientId is not configured.");
            _parkWhizApiKey = _configuration["ParkWhiz:ApiKey"] ?? string.Empty;
        }

        // Parameters passed from EventSearch.cshtml
        [BindProperty(SupportsGet = true)]
        public long EventId { get; set; }

        // Data to display
        public Event EventDetails { get; private set; } = new Event();
        public List<ParkWhizQuote> ParkingQuotes { get; private set; } = new();
        public bool ParkingSearchPerformed { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;

        public async Task OnGetAsync()
        {
            if (EventId == 0)
            {
                ErrorMessage = "Missing required event ID.";
                return;
            }

            // Fetch Event Details
            if (!await TryLoadEventDetailsAsync())
            {
                return;
            }

            // Fetch Parking Quotes
            ParkingSearchPerformed = true;
            await TryLoadParkingQuotesAsync();
        }

        private async Task<bool> TryLoadEventDetailsAsync()
        {
            try
            {
                var eventApiUrl = $"https://api.seatgeek.com/2/events/{EventId}?client_id={_seatGeekClientId}";
                var response = await _httpClient.GetAsync(eventApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Could not load event details from SeatGeek (Status: {response.StatusCode}).";
                    return false;
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var eventDetails = JsonSerializer.Deserialize<Event>(jsonContent, options);

                if (eventDetails == null)
                {
                    ErrorMessage = "Could not deserialize SeatGeek event details.";
                    return false;
                }

                if (eventDetails.Venue?.Location == null)
                {
                    ErrorMessage = "Event details loaded, but missing venue location data (Lat/Lon). Cannot search for parking.";
                    return false;
                }

                EventDetails = eventDetails;
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred fetching event details: {ex.Message}";
                return false;
            }
        }

        private async Task TryLoadParkingQuotesAsync()
        {
            var lat = EventDetails.Venue.Location.Lat;
            var lon = EventDetails.Venue.Location.Lon;

            var parkingStartTimeUtc = EventDetails.DatetimeUtc.AddHours(-1);
            var parkingEndTimeUtc = EventDetails.EndDatetimeUtc?.AddHours(2)
                                    ?? parkingStartTimeUtc.AddHours(4);

            var startTimeFormatted = parkingStartTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            var endTimeFormatted = parkingEndTimeUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            var parkWhizUrl = $"https://api.parkwhiz.com/v4/quotes?q=coordinates:{lat},{lon}" +
                              $"&start_time={Uri.EscapeDataString(startTimeFormatted)}" +
                              $"&end_time={Uri.EscapeDataString(endTimeFormatted)}" +
                              $"&returns=offstreet_bookable";

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, parkWhizUrl);

                if (!string.IsNullOrEmpty(_parkWhizApiKey))
                {
                    request.Headers.Add("X-Api-Key", _parkWhizApiKey);
                }

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Could not load parking quotes from ParkWhiz (Status: {response.StatusCode}).";
                    return;
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var quotes = JsonSerializer.Deserialize<List<ParkWhizQuote>>(jsonContent, options);

                if (quotes != null)
                {
                    ParkingQuotes = quotes
                        .Where(q => q.PurchaseOptions.Count > 0)
                        .OrderBy(q => q.MinPrice)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred fetching parking quotes: {ex.Message}";
            }
        }
    }
}
