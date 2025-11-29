using Swashbuckle.AspNetCore.Annotations;

namespace IS7024FinalProject.Pages.API;

public static class GetParkingSearch
{
    // Call this from Program.cs to register the endpoint
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/getparkingsearch",
                [SwaggerOperation(
                    Summary = "List of Parking Spots",
                    Description = "Get a list of parking spots from our sources by Latitude and Longitude."
                )]
        [SwaggerResponse(StatusCodes.Status200OK)]
        [SwaggerResponse(StatusCodes.Status400BadRequest)]
        [SwaggerResponse(StatusCodes.Status404NotFound)]
        [SwaggerResponse(StatusCodes.Status500InternalServerError)]
        async (IS7024FinalProject.Services.IAPIService apiService, ILoggerFactory loggerFactory, [FromQuery(Name = "lat")] double? lat, [FromQuery(Name = "lon")] double? lon) =>
                {
                    var logger = loggerFactory.CreateLogger("GetParkingSearch");

                    if (!lat.HasValue || !lon.HasValue)
                    {
                        return Results.BadRequest();
                    }

                    try
                    {
                        // Use the correct return type from IAPIService: ParkWhizQuote
                        (IEnumerable<ParkWhizQuote>? parkingSpots, string? parkingMessage) = await apiService.SearchParkingAsync(lat.Value, lon.Value, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(3));

                        if (parkingSpots is null || !parkingSpots.Any())
                        {
                            return Results.NotFound();
                        }

                        return Results.Ok(parkingSpots.ToList());
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while searching parking spots for query '{Lat}' and '{Lon}'", lat, lon);
                        return Results.Problem(detail: "An internal error occurred while processing the request.", statusCode: 500);
                    }
                })
            .Produces<List<ParkWhizQuote>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithTags("Parking");
    }
}