using AutoMapper;
using IS7024FinalProject.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IS7024FinalProject.Pages.API;

public class EventSearchModel
{
    private IAPIService _apiService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    // Inject the API service (ensure IApiService is registered in DI)
    public EventSearchModel(HttpClient httpClient, IConfiguration configuration, IMapper mapper)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _mapper = mapper;
        _apiService = new APIService(_httpClient, _configuration);
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/eventsearch",
            //[SwaggerOperation(
            //    EndpointSummaryAttribute = "List of Events",
            //    EndpointDescriptionAttribute = "Get a list of events by search criteria."
            //)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        async Task<Results<NotFound, Ok<List<GetEventResponse>>>> (string q) =>
            {
                return await HandleAsync(q);
            })
            .Produces<List<GetEventResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags("Config Bot Bot");
    }

    public async Task<Results<NotFound, Ok<List<GetEventResponse>>>> HandleAsync(string q)
    {
        var (events, eventMessage) = await _apiService.SearchEventsAsync(q);
        if (events is null || events.Count() == 0)
        {
            return TypedResults.NotFound();
        }

        var response = events.Select(e => _mapper.Map<GetEventResponse>(e)).ToList();
        return TypedResults.Ok(response);
    }

    //// Example: GET /API/ParkingSearch?q=...
    //public async Task<IActionResult> OnGetAsync([FromQuery(Name = "q")] string q)
    //{
    //    if (string.IsNullOrWhiteSpace(q))
    //    {
    //        return BadRequest(new { error = "Query parameter 'q' is required." });
    //    }

    //    try
    //    {
    //        var result = await _apiService.SearchEventsAsync(q);
    //        // Return raw result as JSON. Adjust mapping if your service returns a domain type.
    //        return new JsonResult(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        // Log exception as needed (not shown here).
    //        Response.StatusCode = 500;
    //        return new JsonResult(new { error = "An error occurred while searching parking.", detail = ex.Message });
    //    }
    //}
}