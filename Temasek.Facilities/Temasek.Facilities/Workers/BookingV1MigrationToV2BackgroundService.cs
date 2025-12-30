using Google.Apis.Calendar.v3;
using MemoryPack;
using Microsoft.Extensions.Options;
using Temasek.Facilities.Entities;
using Temasek.Facilities.Entities.Booking;
using Temasek.Facilities.Extensions;
using Temasek.Facilities.Options;

namespace Temasek.Facilities.Workers;

public partial class BookingV1MigrationToV2BackgroundService(
    [FromKeyedServices("bookings")] CalendarService cal,
    ILogger<BookingV1MigrationToV2BackgroundService> logger,
    IOptions<AppOptions> appOptions,
    IOptions<GoogleOptions> options
) : BackgroundService
{
    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Service not running as option is not enabled in App Options : {OptionName}"
    )]
    private partial void LogServiceNotEnabled(string optionName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!appOptions.Value.BookingV1MigrationToV2BackgroundServiceEnabled)
        {
            LogServiceNotEnabled(nameof(AppOptions.BookingV1MigrationToV2BackgroundServiceEnabled));
            return;
        }

        var res = cal.Events.ListAllAsync(options.Value.CalendarId, ct: stoppingToken);

        var items = res.Select(i => new { Event = i, Data = i.ExtendedProperties.Shared["Data"] })
            .Select(d => new { d.Event, Data = Convert.FromBase64String(d.Data) })
            .Select(d => new
            {
                d.Event,
#pragma warning disable CS0618
                Data = MemoryPackSerializer.Deserialize<BookingV1>(d.Data),
#pragma warning restore CS0618
            });

        await foreach (var i in items)
        {
            var facilityId = i.Data?.FacilityName switch
            {
                "Eiger" => Facility.Eiger,
                "Temasek Square" => Facility.TemasekSquare,
                "Ballinger" => Facility.Ballinger,
                "MPH" => Facility.Mph,
                "Mess" => Facility.Mess,
                "Futsal A" => Facility.FutsalA,
                "Futsal B" => Facility.FutsalB,
                "Audit" => Facility.Audit,
                "Conference Room" => Facility.ConferenceRoom,
                "Kingon" => Facility.Kingon,
                "2SIR RTS" => Facility.TwoSirRts,
                "Lions Square" => Facility.LionsSquare,
                "Lions Cove" => Facility.LionsCove,
                "Lions Den" => Facility.LionsDen,
                "Lions Pride" => Facility.LionsPride,
                "Lions Heart" => Facility.LionsHeart,
                _ => throw new InvalidOperationException("Unknown facility name"),
            };

            var v2 = new BookingV2
            {
                Id = new Guid(i.Data.Id.ToString()),

                StartDateTime = new DateTimeOffset(
                    i.Data.StartDateTime?.DateTime
                        ?? throw new Exception($"No start date time for {i.Data.Id.ToString()}")
                ),

                EndDateTime = new DateTimeOffset(
                    i.Data.EndDateTime?.DateTime
                        ?? throw new Exception($"No end date time for {i.Data.Id.ToString()}")
                ),

                Conduct =
                    i.Data.Conduct
                    ?? throw new Exception($"No conduct name for {i.Data.Id.ToString()}"),

                Description = i.Data.Description ?? "",

                PocName =
                    i.Data.PocName?.ToUpperInvariant()
                    ?? throw new Exception($"No PocName for {i.Data.Id.ToString()}"),

                PocPhone = new PhoneNumber(
                    "65",
                    new string(i.Data.PocPhone?.Trim().TakeLast(8).ToArray())
                ),

                BookedBy = new BookingV2BookedByEitherUserPhoneOrId
                {
                    Phone = new PhoneNumber(
                        "65",
                        new string(i.Data.UserPhone?.Trim().TakeLast(8).ToArray())
                    ),
                    UserId = null,
                },

                FacilityId = facilityId.Id,
            };
        }
    }
}
