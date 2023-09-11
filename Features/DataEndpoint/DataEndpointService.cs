using DecouplingLegacyUsingMicroservice.Models;

namespace DecouplingLegacyUsingMicroservice.Features;

public class DataEndpointService
{
    public bool SendData(Data data)
    {
        DataDTO? d = data.ToDataDTO();
       // DO STUFF          DataEndpointMapper.ToDataDTO(data)
        return true;
    }
}
