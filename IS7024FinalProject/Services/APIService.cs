using Humanizer;
using IS7024FinalProject.DTOs;
using System.Globalization;
using System.Text.Json;
using System.Web;

namespace IS7024FinalProject.Services
{
    public interface IAPIService
    {
        Task<(Event, string)> EventDetailsAsync(string EventId, CancellationToken ct = default);
        Task<(List<Event>, string)> SearchEventsAsync(string query,  CancellationToken ct = default);

        Task<(List<ParkWhizQuote>, string)> SearchParkingAsync(double lat, double lon, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);

        Task<(List<ParkWhizQuote>, string)> SearchParkingAsync(string query, CancellationToken ct = default);
    }

    public class APIService : IAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _parkWhizApiKey;
        private readonly string _seatgeekApiKey;

        public APIService()
        {}

        public APIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _parkWhizApiKey = configuration["ParkWhiz:ApiKey"] ?? "";
            _seatgeekApiKey = configuration["SeatGeek:ClientId"] ?? throw new InvalidOperationException("SeatGeek:ClientId is not configured. Please set this value in appsettings.json or environment variables.");
        }

        public async Task<(List<Event>, string)> SearchEventsAsync(string query, CancellationToken ct = default)
        {
            // Ensure the query is not empty
            if (string.IsNullOrWhiteSpace(query))
            {
                return (new List<Event>(), "Query string is empty.");
            }

            var apiUrl = $"https://api.seatgeek.com/2/events?q={HttpUtility.UrlEncode(query)}&client_id={_seatgeekApiKey}";

            try
            {
                var response = await _httpClient.GetAsync(apiUrl, ct);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync(ct);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var seatGeekResponse = JsonSerializer.Deserialize<SeatGeekResponse>(jsonContent, options);
                    return (seatGeekResponse?.Events ?? new List<Event>(), string.Empty);
                }
                else
                {
                    return (new List<Event>(), $"Failed to retrieve events (Status: {response.StatusCode})");
                }
            }
            catch (Exception ex)
            {
                return (new List<Event>(), $"An error occurred while searching for events: {ex.Message}");
            }
        }

        public async Task<(Event, string)> EventDetailsAsync(string EventId, CancellationToken ct = default)
        {
            // --- 1. Fetch Event Details (Required to get Lat/Lon) ---
            try
            {
                var eventApiUrl = $"https://api.seatgeek.com/2/events/{EventId}?client_id={_seatgeekApiKey}";
                var response = await _httpClient.GetAsync(eventApiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var eventDetails = JsonSerializer.Deserialize<Event>(jsonContent, options);
                    if (eventDetails != null)
                    {

                        // ADDITIONAL VALIDATION: Ensure location data is available
                        if (eventDetails.Venue?.Location == null)
                        {
                            return (new Event(), "Event details loaded, but missing venue location data (Lat/Lon). Cannot search for parking.");
                        }
                        else
                        {
                            return (eventDetails, string.Empty);
                        }
                    }
                    else
                    {
                        return (new Event(), "Could not deserialize SeatGeek event details.");
                    }
                }
                else
                {
                    return (new Event(), $"Could not load event details from SeatGeek (Status: {response.StatusCode}).");
                }
            }
            catch (Exception ex)
            {
                return (new Event(), $"An error occurred fetching event details: {ex.Message}");
            }
        }

        public async Task<(List<ParkWhizQuote>, string)> SearchParkingAsync(double lat, double lon, DateTime startUtc, DateTime endUtc, CancellationToken ct = default)
        {
            // Ensure UTC formatted strings with trailing Z
            var start = startUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            var end = endUtc.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            var url = $"http://api.parkwhiz.com/v4/quotes?q=coordinates:{lat},{lon}&start_time={HttpUtility.UrlEncode(start)}&end_time={HttpUtility.UrlEncode(end)}&returns=offstreet_bookable";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(_parkWhizApiKey))
                {
                    request.Headers.Add("X-Api-Key", _parkWhizApiKey);
                }

                using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return (new List<ParkWhizQuote>(), $"Failed to retrieve parking quotes (Status: {response.StatusCode})");
                }

                var json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var quotes = JsonSerializer.Deserialize<List<ParkWhizQuote>>(json, options) ?? new List<ParkWhizQuote>();

                var filtered = quotes.Where(q => q.PurchaseOptions != null && q.PurchaseOptions.Count > 0)
                                     .OrderBy(q => q.MinPrice)
                                     .ToList();

                return (filtered, string.Empty);
            }
            catch (Exception ex)
            {
                // Swallow exceptions; return empty list so callers can show friendly UI messages.
                return (new List<ParkWhizQuote>(), $"An error occurred fetching parking quotes: {ex.Message}");
            }
        }

        public async Task<(List<ParkWhizQuote>, string)> SearchParkingAsync(string query, CancellationToken ct)
        {
            // Ensure UTC formatted strings with trailing Z
            var start = DateTime.UtcNow.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            var end = DateTime.UtcNow.AddHours(4).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

            var url = $"http://api.parkwhiz.com/v4/quotes?q={query}&start_time={HttpUtility.UrlEncode(start)}&end_time={HttpUtility.UrlEncode(end)}&returns=offstreet_bookable";

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                if (!string.IsNullOrEmpty(_parkWhizApiKey))
                {
                    request.Headers.Add("X-Api-Key", _parkWhizApiKey);
                }

                using var response = await _httpClient.SendAsync(request, ct).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    return (new List<ParkWhizQuote>(), $"Failed to retrieve parking quotes (Status: {response.StatusCode})");
                }

                var json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var quotes = JsonSerializer.Deserialize<List<ParkWhizQuote>>(json, options) ?? new List<ParkWhizQuote>();

                var filtered = quotes.Where(q => q.PurchaseOptions != null && q.PurchaseOptions.Count > 0)
                                     .OrderBy(q => q.MinPrice)
                                     .ToList();

                return (filtered, string.Empty);
            }
            catch (Exception ex)
            {
                // Swallow exceptions; return empty list so callers can show friendly UI messages.
                return (new List<ParkWhizQuote>(), $"An error occurred fetching parking quotes: {ex.Message}");
            }

        }
    }
}