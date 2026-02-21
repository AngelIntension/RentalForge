namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Represents a film actor. Related to films through the FilmActor join table.
/// </summary>
public class Actor
{
    public int ActorId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime LastUpdate { get; set; }

    public ICollection<FilmActor> FilmActors { get; set; } = [];
}
