using MemoryPack;

namespace Temasek.Facilities.Entities.Booking;

[MemoryPackable]
public partial class BookingV2 : IBookingBase
{
    public Guid Id { get; set; }
    public DateTimeOffset StartDateTime { get; set; }
    public DateTimeOffset EndDateTime { get; set; }
    public string Conduct { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PocName { get; set; } = null!;
    public PhoneNumber PocPhone { get; set; } = null!;

    public string FacilityId { get; set; } = null!;

    public BookingV2BookedByEitherUserPhoneOrId BookedBy { get; set; } = null!;
}

/// <summary>
/// Due to backward compatibility with <see cref="BookingV1"/>, we retain the <see cref="Phone"/> property only for legacy events. All new events should tag user to Clerk.
/// </summary>
[MemoryPackable]
public partial record BookingV2BookedByEitherUserPhoneOrId
{
    public PhoneNumber? Phone { get; set; }
    public string? UserId { get; set; }
}
