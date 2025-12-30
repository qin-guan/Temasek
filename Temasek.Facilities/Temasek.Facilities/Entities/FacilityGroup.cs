namespace Temasek.Facilities.Entities;

public record FacilityGroup
{
    public static readonly FacilityGroup Outdoor = new("Outdoor");
    public static readonly FacilityGroup Indoor = new("Indoor");
    public static readonly FacilityGroup Conference = new("Conference");
    public static readonly FacilityGroup Futsal = new("Futsal");
    public static readonly FacilityGroup Others = new("Others");
    public static readonly FacilityGroup TwoSir = new("2SIR");
    public static readonly FacilityGroup FiveSir = new("5SIR");
    public static readonly List<FacilityGroup> All =
    [
        Outdoor,
        Indoor,
        Conference,
        Futsal,
        Others,
        TwoSir,
        FiveSir,
    ];

    public string Value { get; set; }

    private FacilityGroup(string value)
    {
        Value = value;
    }
}
