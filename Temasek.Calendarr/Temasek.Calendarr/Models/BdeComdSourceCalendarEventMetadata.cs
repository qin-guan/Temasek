using System;

namespace Temasek.Calendarr.Models;

public abstract class BdeComdSourceCalendarEventMetadata
{
    public const string Key = nameof(BdeComdSourceCalendarEventMetadata);
    public abstract Version Version { get; set; }
}
