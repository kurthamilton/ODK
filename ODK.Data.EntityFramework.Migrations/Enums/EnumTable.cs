namespace ODK.Data.EntityFramework.Migrations.Enums;

/// <summary>
/// The lookup table that mirrors an enum, so other tables can foreign key to its values.
/// </summary>
public sealed record EnumTable
{
    public string IdColumnName { get; init; } = "Id";

    public required string Name { get; init; }
}
