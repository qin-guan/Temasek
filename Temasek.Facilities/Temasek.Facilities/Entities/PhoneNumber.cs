using MemoryPack;

namespace Temasek.Facilities.Entities;

[MemoryPackable]
public partial record PhoneNumber(string CountryCode, string Number);
