using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization; // Required for JsonPropertyName
using System.Globalization;

namespace IS7024FinalProject.Pages;

public class EventSummary
{
    public string? ArtistName { get; set; }
    public string? Genre { get; set; }
    public string? Country { get; set; }
    public string? Popularity { get; set; }
    public string? Venue { get; set; }
    public string? City { get; set; }

    // Date is returned as a string (date-time format) in the API response
    public DateTimeOffset? Date { get; set; }

    public string? Url { get; set; }


    public string FormattedDate => Date?.ToString("ddd, MMM dd, yyyy h:mm tt", CultureInfo.InvariantCulture) ?? "TBD";
}


public class Where2PlayModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Where2PlayModel> _logger;

    private const string ApiBaseUrl = "https://where2play-e8eafmaxcvbuhgcr.eastus2-01.azurewebsites.net/api/Values";

    public List<EventSummary> Events { get; set; } = new List<EventSummary>();

    [TempData]
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string City { get; set; } = "Cincinnati"; // Default to 'Cincinnati'

    public Where2PlayModel(HttpClient httpClient, ILogger<Where2PlayModel> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(City))
        {
            ErrorMessage = "Please enter a city to search for events.";
            return;
        }

        var requestUrl = $"{ApiBaseUrl}/search?city={Uri.EscapeDataString(City)}";
        _logger.LogInformation("Attempting to fetch data from: {RequestUrl}", requestUrl);

        try
        {
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var events = JsonSerializer.Deserialize<List<EventSummary>>(jsonContent, options);

                if (events != null)
                {
                    Events = events;
                    _logger.LogInformation("Successfully fetched {Count} events for city: {City}", Events.Count, City);
                }
                else
                {
                    ErrorMessage = $"Failed to parse API response for city: {City}. The city might not be supported or the data format is unexpected.";
                    _logger.LogError("Deserialization failed for response: {JsonContent}", jsonContent);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                // Handle 400 Bad Request, as mentioned in the Swagger file for missing city parameter
                ErrorMessage = $"Bad Request. The API requires a valid 'city' parameter. (Status: 400)";
            }
            else
            {
                // Handle other API errors
                ErrorMessage = $"API call failed with status code: {(int)response.StatusCode} {response.ReasonPhrase}.";
                _logger.LogError("API call failed: {StatusCode} for URL {RequestUrl}", response.StatusCode, requestUrl);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An unexpected error occurred: {ex.Message}";
            _logger.LogError(ex, "Exception thrown during API request for city: {City}", City);
        }
    }
}
