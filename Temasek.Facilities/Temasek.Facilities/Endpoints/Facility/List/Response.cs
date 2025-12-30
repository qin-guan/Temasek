namespace Temasek.Facilities.Endpoints.Facility.List;

public class Response
{
    public IEnumerable<FacilityItem> Facilities { get; set; } = [];
}

public class FacilityItem
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Group { get; set; }
    public required IEnumerable<string> Scope { get; set; }
}
