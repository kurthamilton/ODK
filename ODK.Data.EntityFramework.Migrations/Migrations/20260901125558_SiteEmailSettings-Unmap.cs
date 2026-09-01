using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteEmailSettingsUnmap : Migration
    {
        /* No schema change. The site's email settings are read from configuration now, so the model has
           stopped mapping the table, and a model the migrations do not account for is refused at the point
           they are applied - so the change is recorded here to keep the deploy that ships it applicable.

           The table is dropped by a later migration, which cannot come any sooner: a migration runs a minute
           ahead of the code it ships with, so a table dropped here would go while the build that still
           selects it is serving, which takes every email down rather than one admin form. */

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
