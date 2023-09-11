namespace DecouplingLegacyUsingMicroservice.Features;

public static class DataEndpointMapper
{

    public static DataDTO? ToDataDTO(this Data? data)
    {
        if (data == null)
            return null;
        else
            return new DataDTO
            {
                id = data.Id,
                title = data.Title,
                description = data.Description
            };
    }

}