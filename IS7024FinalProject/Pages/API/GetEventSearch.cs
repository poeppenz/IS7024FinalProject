using Swashbuckle.AspNetCore.Annotations;

namespace IS7024FinalProject.Pages.API
{
    public static class GetEventSearch
    {
        // Call this from Program.cs to register the endpoint
        public static void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/geteventsearch",
                    [SwaggerOperation(
                        Summary = "List of Events",
                        Description = "Get a list of events from our sources."
                    )]
            [SwaggerResponse(StatusCodes.Status200OK)]
            [SwaggerResponse(StatusCodes.Status400BadRequest)]
            [SwaggerResponse(StatusCodes.Status404NotFound)]
            [SwaggerResponse(StatusCodes.Status500InternalServerError)]
            async (IS7024FinalProject.Services.IAPIService apiService, ILoggerFactory loggerFactory, [FromQuery(Name = "q")] string? q) =>
                    {
                        var logger = loggerFactory.CreateLogger("GetEventSearch");

                        if (string.IsNullOrWhiteSpace(q))
                        {
                            return Results.BadRequest();
                        }

                        try
                        {
                            (IEnumerable<Event>? events, string? eventMessage) = await apiService.SearchEventsAsync(q);

                            if (events is null || !events.Any())
                            {
                                return Results.NotFound();
                            }

                            return Results.Ok(events.ToList());
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "An error occurred while searching events for query '{Query}'", q);
                            return Results.Problem(detail: "An internal error occurred while processing the request.", statusCode: 500);
                        }
                    })
                .Produces<List<Event>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithTags("Events");
        }
    }
}