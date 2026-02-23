using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using NpgsqlTypes;

namespace RentalForge.Api.Data.Entities;

/// <summary>
/// MPAA film rating system. Maps to PostgreSQL mpaa_rating enum.
/// </summary>
public enum MpaaRating
{
    [PgName("G")]
    [EnumMember(Value = "G")]
    [JsonStringEnumMemberName("G")]
    G,

    [PgName("PG")]
    [EnumMember(Value = "PG")]
    [JsonStringEnumMemberName("PG")]
    PG,

    [PgName("PG-13")]
    [EnumMember(Value = "PG-13")]
    [JsonStringEnumMemberName("PG-13")]
    Pg13,

    [PgName("R")]
    [EnumMember(Value = "R")]
    [JsonStringEnumMemberName("R")]
    R,

    [PgName("NC-17")]
    [EnumMember(Value = "NC-17")]
    [JsonStringEnumMemberName("NC-17")]
    Nc17
}
