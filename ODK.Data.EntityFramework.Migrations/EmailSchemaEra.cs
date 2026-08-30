namespace ODK.Data.EntityFramework.Migrations;

/// <summary>
/// The shape of the Emails table at the point a migration was written, which decides the columns that
/// migration inserts into and the column it deletes by.
/// </summary>
/// <remarks>
/// A migration keeps its era forever. That is the shape the table had when it first ran, and the shape it
/// must still write into when a database is built from the migrations alone - so a newer era is a new member
/// here, never an edit to an existing one.
/// </remarks>
internal enum EmailSchemaEra
{
    None = 0,

    /// <summary>
    /// Keyed on EmailTypeId, before the EmailRecipientTypeId column existed.
    /// </summary>
    TypeIdKey,

    /// <summary>
    /// Keyed on EmailTypeId, with EmailRecipientTypeId.
    /// </summary>
    TypeIdKeyWithRecipientType,

    /// <summary>
    /// Keyed on Id, with EmailRecipientTypeId.
    /// </summary>
    IdKey,

    /// <summary>
    /// Keyed on Id, with EmailRecipientTypeId, the body in BodyHtml and the group flag in IsGroupEmail.
    /// </summary>
    IdKeyBodyHtml
}
