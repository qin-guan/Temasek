namespace Temasek.Clerk;

public static class Scope
{
    public const string Hq3Sib = "HQ 3 SIB";
    public const string TwoSir = "2 SIR";
    public const string FiveSir = "5 SIR";

    public static readonly List<string> All = new()
  {
    Hq3Sib,
    TwoSir,
    FiveSir
  };
}
