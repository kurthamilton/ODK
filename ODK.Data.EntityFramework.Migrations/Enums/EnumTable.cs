namespace ODK.Data.EntityFramework.Migrations.Enums;

/// <summary>
/// The lookup table that mirrors an enum, so other tables can foreign key to its values.
/// </summary>
public sealed record EnumTable
{
    /// <summary>
    /// The primary key column, holding the enum's numeric value. Named separately rather than
    /// derived by singularising <see cref="Name"/>, which only works for tables whose plural is
    /// a trailing "s".
    /// </summary>
    public required string IdColumnName { get; init; }

    public required string Name { get; init; }
}
