namespace RentalForge.Api.Data.Entities;

/// <summary>
/// Join table linking films to actors. Composite PK on (ActorId, FilmId).
/// </summary>
public class FilmActor
{
    public int ActorId { get; set; }
    public int FilmId { get; set; }
    public DateTime LastUpdate { get; set; }

    public Actor Actor { get; set; } = null!;
    public Film Film { get; set; } = null!;
}
