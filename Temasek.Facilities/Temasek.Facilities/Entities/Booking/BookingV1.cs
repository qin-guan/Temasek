using MemoryPack;

namespace Temasek.Facilities.Entities.Booking;

[MemoryPackable]
[Obsolete("This should only be used for the purpose of data migration to BookingV2.")]
public partial class BookingV1
{
    public int Row { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset? StartDateTime { get; set; }
    public DateTimeOffset? EndDateTime { get; set; }
    public string? Conduct { get; set; }
    public string? Description { get; set; }
    public string? PocName { get; set; }
    public string? PocPhone { get; set; }

    public string? FacilityName { get; set; }
    public string? UserPhone { get; set; }

    public string GetId()
    {
        return Id.ToString();
    }
}
