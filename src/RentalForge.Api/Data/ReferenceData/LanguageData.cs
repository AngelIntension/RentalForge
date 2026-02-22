using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Data.ReferenceData;

public static class LanguageData
{
    private static readonly DateTime LastUpdate = new(2006, 2, 15, 10, 2, 19, DateTimeKind.Utc);

    public static Language[] GetAll() =>
    [
        new() { LanguageId = 1, Name = "English             ", LastUpdate = LastUpdate },
        new() { LanguageId = 2, Name = "Italian             ", LastUpdate = LastUpdate },
        new() { LanguageId = 3, Name = "Japanese            ", LastUpdate = LastUpdate },
        new() { LanguageId = 4, Name = "Mandarin            ", LastUpdate = LastUpdate },
        new() { LanguageId = 5, Name = "French              ", LastUpdate = LastUpdate },
        new() { LanguageId = 6, Name = "German              ", LastUpdate = LastUpdate },
    ];
}
