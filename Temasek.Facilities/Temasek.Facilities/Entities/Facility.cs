namespace Temasek.Facilities.Entities;

public record Facility
{
    public static readonly Facility Eiger = new(
        "eiger",
        "Eiger",
        FacilityGroup.Outdoor,
        FacilityScope.All
    );

    public static readonly Facility TemasekSquare = new(
        "temasek-square",
        "Temasek Square",
        FacilityGroup.Outdoor,
        FacilityScope.All
    );

    public static readonly Facility Ballinger = new(
        "ballinger",
        "Ballinger",
        FacilityGroup.Outdoor,
        FacilityScope.All
    );

    public static readonly Facility Mph = new(
        "mph",
        "MPH",
        FacilityGroup.Indoor,
        FacilityScope.All
    );

    public static readonly Facility Mess = new(
        "mess",
        "Mess",
        FacilityGroup.Indoor,
        FacilityScope.All
    );

    public static readonly Facility FutsalA = new(
        "futsal-a",
        "Futsal A",
        FacilityGroup.Futsal,
        FacilityScope.All
    );

    public static readonly Facility FutsalB = new(
        "futsal-b",
        "Futsal B",
        FacilityGroup.Futsal,
        FacilityScope.All
    );

    public static readonly Facility Audit = new(
        "audit",
        "Audit",
        FacilityGroup.Conference,
        FacilityScope.All
    );

    public static readonly Facility ConferenceRoom = new(
        "conference-room",
        "Conference Room",
        FacilityGroup.Conference,
        FacilityScope.All
    );

    public static readonly Facility Kingon = new(
        "kingon",
        "Kingon",
        FacilityGroup.Others,
        FacilityScope.All
    );

    public static readonly Facility TwoSirRts = new(
        "2sir-rts",
        "2SIR RTS",
        FacilityGroup.TwoSir,
        [FacilityScope.BdeHq, FacilityScope.TwoSir]
    );

    public static readonly Facility LionsSquare = new(
        "lions-square",
        "Lions Square",
        FacilityGroup.TwoSir,
        [FacilityScope.BdeHq, FacilityScope.TwoSir]
    );

    public static readonly Facility LionsCove = new(
        "lions-cove",
        "Lions Cove",
        FacilityGroup.TwoSir,
        [FacilityScope.BdeHq, FacilityScope.TwoSir]
    );

    public static readonly Facility LionsDen = new(
        "lions-den",
        "Lions Den",
        FacilityGroup.TwoSir,
        [FacilityScope.BdeHq, FacilityScope.TwoSir]
    );

    public static readonly Facility LionsPride = new(
        "lions-pride",
        "Lions Pride",
        FacilityGroup.TwoSir,
        [FacilityScope.BdeHq, FacilityScope.TwoSir]
    );

    public static readonly Facility LionsHeart = new(
        "lions-heart",
        "Lions Heart",
        FacilityGroup.TwoSir,
        [FacilityScope.BdeHq, FacilityScope.TwoSir]
    );

    public static readonly List<Facility> All =
    [
        Eiger,
        TemasekSquare,
        Ballinger,
        Mph,
        Mess,
        FutsalA,
        FutsalB,
        Audit,
        ConferenceRoom,
        Kingon,
        TwoSirRts,
        LionsSquare,
        LionsCove,
        LionsDen,
        LionsPride,
        LionsHeart,
    ];

    public string Id { get; set; }
    public string Name { get; set; }
    public FacilityGroup Group { get; set; }
    public List<FacilityScope> Scope { get; set; }

    private Facility(string id, string name, FacilityGroup group, List<FacilityScope> scope)
    {
        Id = id;
        Name = name;
        Group = group;
        Scope = scope;
    }

    public Facility(string id)
    {
        var item =
            All.FirstOrDefault(v => v.Id == id)
            ?? throw new ArgumentException($"Invalid facility id: {id}");

        Id = item.Id;
        Name = item.Name;
        Group = item.Group;
        Scope = item.Scope;
    }
}
