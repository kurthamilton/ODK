using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class LogsAdd : Migration
    {
        // The Logs table is written to by the Serilog MSSqlServer sink (see AddLogging in Program.cs),
        // not mapped as an EF entity - so it's created here with raw SQL rather than via the model. The
        // schema matches the sink's default ColumnOptions (Serilog.Sinks.MSSqlServer 10.0.0); the column
        // names must match exactly or the sink's INSERT fails.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[Logs]', N'U') IS NULL
BEGIN
    CREATE TABLE [Logs] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Message] nvarchar(max) NULL,
        [MessageTemplate] nvarchar(max) NULL,
        [Level] nvarchar(16) NULL,
        [TimeStamp] datetime NULL,
        [Exception] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS [Logs];");
        }
    }
}
