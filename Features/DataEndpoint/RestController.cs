using Microsoft.AspNetCore.Authorization;

namespace DecouplingLegacyUsingMicroservice.Features;

[Authorize]
public static class RestController
{
    public static void SetupRestEndpoint(this WebApplication app)
    {
        app.MapPost("/sendRestData", (Data data, DataEndpointService dataService) =>
        {
            if (dataService.SendData(data))
                return Results.Ok($"Data was sent using Rest");
            else
                return Results.BadRequest($"Data could not be sent using Rest");
        });
    }
}