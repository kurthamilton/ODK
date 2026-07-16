-- =============================================================================
-- Baseline bootstrap for EF Core migrations.
--
-- Run this ONCE against any database that ALREADY contains the ODK schema
-- (production, staging, and existing local dev DBs restored from a prod backup).
--
-- It records the initial migration (20260716081923_InitialCreate) as already
-- applied WITHOUT running its CREATE TABLE statements, so subsequent
-- `dotnet ef database update` / migration-bundle runs apply only NEW migrations
-- and never try to recreate existing tables.
--
-- Do NOT run this against a brand-new / empty database. There, apply migrations
-- normally (dotnet ef database update) so InitialCreate builds the schema.
-- =============================================================================

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260716081923_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260716081923_InitialCreate', N'10.0.9');
END;
GO
