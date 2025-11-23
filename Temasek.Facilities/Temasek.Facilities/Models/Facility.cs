using SqlSugar;

namespace Temasek.Facilities.Models;

public class Facility
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Group { get; set; }

    [SugarColumn(IsJson = true)]
    public List<string>? Scope { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(Booking.FacilityId))]
    public List<Booking>? Bookings { get; set; }
}
