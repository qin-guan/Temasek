using SqlSugar;

namespace Temasek.Facilities.Models;

public class Booking
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }
    public string? Conduct { get; set; }
    public string? Description { get; set; }
    public string? PocName { get; set; }
    public string? PocPhone { get; set; }
    public DateTimeOffset? StartDateTime { get; set; }
    public DateTimeOffset? EndDateTime { get; set; }

    public string? UserPhone { get; set; }

    public Guid FacilityId { get; set; }

    [Navigate(NavigateType.ManyToOne, nameof(FacilityId), nameof(Facility.Id))]
    public Facility? FacilityName { get; set; }
}
