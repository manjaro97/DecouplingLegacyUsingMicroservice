namespace DecouplingLegacyUsingMicroservice.Models;

public record Data
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
}
