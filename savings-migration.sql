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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406123037_InitSqlServer'
)
BEGIN
    CREATE TABLE [events] (
        [_id] uniqueidentifier NOT NULL,
        [handler_id] uniqueidentifier NOT NULL,
        [type] nvarchar(100) NOT NULL,
        [status] nvarchar(100) NULL,
        [timestamp] datetimeoffset NOT NULL,
        [username] nvarchar(100) NOT NULL,
        [host_user] nvarchar(100) NULL,
        [revitbuild] nvarchar(100) NOT NULL,
        [revit] nvarchar(100) NOT NULL,
        [cancelled] bit NULL,
        [cancellable] bit NULL,
        [docid] int NOT NULL,
        [doctype] nvarchar(100) NULL,
        [doctemplate] nvarchar(100) NULL,
        [docname] nvarchar(250) NULL,
        [docpath] nvarchar(1024) NULL,
        [projectname] nvarchar(250) NULL,
        [projectnum] nvarchar(100) NULL,
        [args] nvarchar(max) NULL,
        CONSTRAINT [PK_events] PRIMARY KEY ([_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406123037_InitSqlServer'
)
BEGIN
    CREATE TABLE [scripts] (
        [_id] uniqueidentifier NOT NULL,
        [sessionid] uniqueidentifier NOT NULL,
        [timestamp] datetimeoffset NOT NULL,
        [username] nvarchar(100) NOT NULL,
        [host_user] nvarchar(100) NULL,
        [revitbuild] nvarchar(100) NOT NULL,
        [revit] nvarchar(100) NOT NULL,
        [pyrevit] nvarchar(100) NOT NULL,
        [clone] nvarchar(100) NOT NULL,
        [debug] bit NOT NULL,
        [config] bit NOT NULL,
        [from_gui] bit NOT NULL,
        [exec_id] nvarchar(100) NOT NULL,
        [exec_timestamp] datetimeoffset NOT NULL,
        [commandbundle] nvarchar(250) NOT NULL,
        [commandextension] nvarchar(250) NOT NULL,
        [commandname] nvarchar(250) NOT NULL,
        [commanduniquename] nvarchar(500) NOT NULL,
        [docname] nvarchar(250) NULL,
        [docpath] nvarchar(1024) NULL,
        [resultcode] int NOT NULL,
        [scriptpath] nvarchar(1024) NOT NULL,
        [message] nvarchar(max) NOT NULL,
        [type] nvarchar(100) NOT NULL,
        [version] nvarchar(100) NOT NULL,
        [syspath] nvarchar(max) NULL,
        [configs] nvarchar(max) NULL,
        [commandresults] nvarchar(max) NULL,
        CONSTRAINT [PK_scripts] PRIMARY KEY ([_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260406123037_InitSqlServer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260406123037_InitSqlServer', N'10.0.11');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911112708_AddSavings'
)
BEGIN
    CREATE TABLE [savings] (
        [_id] uniqueidentifier NOT NULL,
        [queue_key] nvarchar(100) NOT NULL,
        [kind] nvarchar(20) NOT NULL,
        [source_id] bigint NOT NULL,
        [occurred_at] datetimeoffset NOT NULL,
        [label] nvarchar(250) NOT NULL,
        [minutes] float NOT NULL,
        [recorded_at] datetimeoffset NOT NULL,
        [hostname] nvarchar(100) NULL,
        [username] nvarchar(100) NULL,
        CONSTRAINT [PK_savings] PRIMARY KEY ([_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911112708_AddSavings'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_savings_entry] ON [savings] ([queue_key], [kind], [source_id], [occurred_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911112708_AddSavings'
)
BEGIN
    CREATE INDEX [IX_savings_queue_key] ON [savings] ([queue_key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911112708_AddSavings'
)
BEGIN
    CREATE INDEX [IX_savings_occurred_at] ON [savings] ([occurred_at]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260911112708_AddSavings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260911112708_AddSavings', N'10.0.11');
END;

COMMIT;
GO

