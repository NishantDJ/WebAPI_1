IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Employees] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Department] nvarchar(max) NOT NULL,
    [Salary] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Employees] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251218210259_InitialCreate', N'10.0.1');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Employees] ADD [DateOfJoining] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Employees] ADD [Email] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [Employees] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251229193323_UpdateEmployee', N'10.0.1');

COMMIT;
GO

