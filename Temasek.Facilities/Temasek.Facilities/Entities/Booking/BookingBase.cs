using MemoryPack;

namespace Temasek.Facilities.Entities.Booking;

[MemoryPackable]
[MemoryPackUnion(0, typeof(BookingV2))]
public partial interface IBookingBase;
