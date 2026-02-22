using RentalForge.Api.Data.Entities;

namespace RentalForge.Api.Data.ReferenceData;

public static class CategoryData
{
    private static readonly DateTime LastUpdate = new(2006, 2, 15, 9, 46, 27, DateTimeKind.Utc);

    public static Category[] GetAll() =>
    [
        new() { CategoryId = 1, Name = "Action", LastUpdate = LastUpdate },
        new() { CategoryId = 2, Name = "Animation", LastUpdate = LastUpdate },
        new() { CategoryId = 3, Name = "Children", LastUpdate = LastUpdate },
        new() { CategoryId = 4, Name = "Classics", LastUpdate = LastUpdate },
        new() { CategoryId = 5, Name = "Comedy", LastUpdate = LastUpdate },
        new() { CategoryId = 6, Name = "Documentary", LastUpdate = LastUpdate },
        new() { CategoryId = 7, Name = "Drama", LastUpdate = LastUpdate },
        new() { CategoryId = 8, Name = "Family", LastUpdate = LastUpdate },
        new() { CategoryId = 9, Name = "Foreign", LastUpdate = LastUpdate },
        new() { CategoryId = 10, Name = "Games", LastUpdate = LastUpdate },
        new() { CategoryId = 11, Name = "Horror", LastUpdate = LastUpdate },
        new() { CategoryId = 12, Name = "Music", LastUpdate = LastUpdate },
        new() { CategoryId = 13, Name = "New", LastUpdate = LastUpdate },
        new() { CategoryId = 14, Name = "Sci-Fi", LastUpdate = LastUpdate },
        new() { CategoryId = 15, Name = "Sports", LastUpdate = LastUpdate },
        new() { CategoryId = 16, Name = "Travel", LastUpdate = LastUpdate },
    ];
}
