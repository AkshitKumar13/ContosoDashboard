using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Data;

public static class DocumentSchemaInitializer
{
    public static void EnsureCreated(ApplicationDbContext context)
    {
        context.Database.ExecuteSqlRaw(@"
IF OBJECT_ID(N'[dbo].[Documents]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Documents] (
        [DocumentId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Documents] PRIMARY KEY,
        [Title] nvarchar(255) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [Category] nvarchar(100) NOT NULL,
        [Tags] nvarchar(1000) NULL,
        [OriginalFileName] nvarchar(255) NOT NULL,
        [FilePath] nvarchar(500) NOT NULL,
        [FileSize] bigint NOT NULL,
        [ContentType] nvarchar(255) NOT NULL,
        [UploadedDate] datetime2 NOT NULL,
        [UpdatedDate] datetime2 NOT NULL,
        [UploadedByUserId] int NOT NULL,
        [ProjectId] int NULL,
        [TaskId] int NULL,
        CONSTRAINT [FK_Documents_Users_UploadedByUserId] FOREIGN KEY ([UploadedByUserId]) REFERENCES [dbo].[Users] ([UserId]),
        CONSTRAINT [FK_Documents_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([ProjectId]),
        CONSTRAINT [FK_Documents_Tasks_TaskId] FOREIGN KEY ([TaskId]) REFERENCES [dbo].[Tasks] ([TaskId])
    );
END;

IF OBJECT_ID(N'[dbo].[DocumentShares]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DocumentShares] (
        [DocumentShareId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_DocumentShares] PRIMARY KEY,
        [DocumentId] int NOT NULL,
        [SharedWithUserId] int NULL,
        [SharedWithDepartment] nvarchar(100) NULL,
        [SharedByUserId] int NOT NULL,
        [SharedDate] datetime2 NOT NULL,
        CONSTRAINT [FK_DocumentShares_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([DocumentId]) ON DELETE CASCADE,
        CONSTRAINT [FK_DocumentShares_Users_SharedWithUserId] FOREIGN KEY ([SharedWithUserId]) REFERENCES [dbo].[Users] ([UserId]),
        CONSTRAINT [FK_DocumentShares_Users_SharedByUserId] FOREIGN KEY ([SharedByUserId]) REFERENCES [dbo].[Users] ([UserId])
    );
END;

IF OBJECT_ID(N'[dbo].[DocumentActivities]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[DocumentActivities] (
        [DocumentActivityId] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_DocumentActivities] PRIMARY KEY,
        [DocumentId] int NULL,
        [ActorUserId] int NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [Details] nvarchar(2000) NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [FK_DocumentActivities_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([DocumentId]) ON DELETE SET NULL,
        CONSTRAINT [FK_DocumentActivities_Users_ActorUserId] FOREIGN KEY ([ActorUserId]) REFERENCES [dbo].[Users] ([UserId])
    );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_FilePath' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
    CREATE UNIQUE INDEX [IX_Documents_FilePath] ON [dbo].[Documents] ([FilePath]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_UploadedByUserId' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
    CREATE INDEX [IX_Documents_UploadedByUserId] ON [dbo].[Documents] ([UploadedByUserId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_ProjectId' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
    CREATE INDEX [IX_Documents_ProjectId] ON [dbo].[Documents] ([ProjectId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_TaskId' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
    CREATE INDEX [IX_Documents_TaskId] ON [dbo].[Documents] ([TaskId]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_Category' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
    CREATE INDEX [IX_Documents_Category] ON [dbo].[Documents] ([Category]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_UploadedDate' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
    CREATE INDEX [IX_Documents_UploadedDate] ON [dbo].[Documents] ([UploadedDate]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Documents_ContentType' AND object_id = OBJECT_ID(N'[dbo].[Documents]'))
    CREATE INDEX [IX_Documents_ContentType] ON [dbo].[Documents] ([ContentType]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentActivities_CreatedDate' AND object_id = OBJECT_ID(N'[dbo].[DocumentActivities]'))
    CREATE INDEX [IX_DocumentActivities_CreatedDate] ON [dbo].[DocumentActivities] ([CreatedDate]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DocumentActivities_Action_CreatedDate' AND object_id = OBJECT_ID(N'[dbo].[DocumentActivities]'))
    CREATE INDEX [IX_DocumentActivities_Action_CreatedDate] ON [dbo].[DocumentActivities] ([Action], [CreatedDate]);
");
    }
}
