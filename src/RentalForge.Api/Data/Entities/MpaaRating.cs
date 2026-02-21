using System.Runtime.Serialization;

namespace RentalForge.Api.Data.Entities;

/// <summary>
/// MPAA film rating system. Maps to PostgreSQL mpaa_rating enum.
/// </summary>
public enum MpaaRating
{
    [EnumMember(Value = "G")]
    G,

    [EnumMember(Value = "PG")]
    PG,

    [EnumMember(Value = "PG-13")]
    Pg13,

    [EnumMember(Value = "R")]
    R,

    [EnumMember(Value = "NC-17")]
    Nc17
}
