using MemoryPack;

namespace Temasek.Calendarr.Models;

[MemoryPackable]
public partial class BdeComdSourceCalendarEventMetadataV1 : BdeComdSourceCalendarEventMetadata
{
    public override Version Version { get; set; } = new(1, 0);

    public required DateTimeOffset? LastBdeComdEventSync { get; set; }
}
