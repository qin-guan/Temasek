namespace Temasek.Facilities.Entities;

public record FacilityScope
{
    public static readonly FacilityScope TwoSir = new("2SIR");
    public static readonly FacilityScope FiveSir = new("5SIR");
    public static readonly FacilityScope BdeHq = new("3SIB");
    public static readonly List<FacilityScope> All = [TwoSir, FiveSir, BdeHq];
    public string Value { get; set; }

    private FacilityScope(string value)
    {
        Value = value;
    }
}
