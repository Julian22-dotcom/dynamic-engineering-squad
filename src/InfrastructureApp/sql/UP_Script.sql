--UP SCRIPT
--USE THIS AFTER THE DOWN SCRIPT HAS BEEN RUN

-- asp.net identity stuff
CREATE TABLE [AspNetRoles] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(256) NULL,
    [NormalizedName] NVARCHAR(256) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL
);

CREATE TABLE [AspNetUsers] (
    [Id] NVARCHAR(450) NOT NULL PRIMARY KEY,
    [UserName] NVARCHAR(256) NULL,
    [NormalizedUserName] NVARCHAR(256) NULL,
    [Email] NVARCHAR(256) NULL,
    [NormalizedEmail] NVARCHAR(256) NULL,
    [PasswordHash] NVARCHAR(MAX) NULL,
    [SecurityStamp] NVARCHAR(MAX) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    [PhoneNumber] NVARCHAR(MAX) NULL,
    [TwoFactorEnabled] BIT NOT NULL,
    [LockoutEnd] DATETIMEOFFSET NULL,
    [LockoutEnabled] BIT NOT NULL,
    [AccessFailedCount] INT NOT NULL
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] NVARCHAR(450) NOT NULL,
    [RoleId] NVARCHAR(450) NOT NULL,
    PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

-- reporting
CREATE TABLE [Reports] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Description] NVARCHAR(MAX) NOT NULL,
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Open',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UserId] NVARCHAR(450) NOT NULL,
    [Latitude] DECIMAL(9,6) NOT NULL,
    [Longitude] DECIMAL(9,6) NOT NULL,
    [ImageUrl] NVARCHAR(450) NULL,
    CONSTRAINT [FK_Reports_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
);

-- gamification
CREATE TABLE [UserPoints] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] NVARCHAR(450) NOT NULL,
    [CurrentPoints] INT NOT NULL DEFAULT 0,
    [LifetimePoints] INT NOT NULL DEFAULT 0,
    [LastUpdated] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_UserPoints_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Leaderboards] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] NVARCHAR(450) NOT NULL,
    [TotalPoints] INT NOT NULL DEFAULT 0,
    [LastUpdated] DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [FK_Leaderboards_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

--DATA SEEDING

-- roles
INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES 
('role-guid-admin', 'Admin', 'ADMIN', NEWID()),
('role-guid-moderator', 'Moderator', 'MODERATOR', NEWID()),
('role-guid-user', 'User', 'USER', NEWID());

-- test user (password = Password123!)
-- Using a fixed GUID for the user to link other tables
DECLARE @UserId NVARCHAR(450) = 'user-guid-001';

INSERT INTO [AspNetUsers] (
    [Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], 
    [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [TwoFactorEnabled], 
    [LockoutEnabled], [AccessFailedCount]
)
VALUES (
    @UserId, 
    'palter', 'PALTER', 
    'p.alter@example.com', 'P.ALTER@EXAMPLE.COM', 
    'AQAAAAEAACcQAAAAEBL6w...YourActualHashHere...', -- totally legit asp.net hash
    NEWID(), NEWID(), 0, 1, 0
);

-- assign 'user' role to user
INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
VALUES (@UserId, 'role-guid-user');

-- init reports
INSERT INTO [Reports] ([Description], [Status], [CreatedAt], [UserId], [Latitude], [Longitude], [ImageUrl])
VALUES 
('Large pothole obstructing the bike lane on 4th St.', 'Unresolved', GETDATE(), @UserId, 45.1158, -122.8974, 'https://placehold.co/600x400?text=Pothole+1'),
('Faded crosswalk markings near the elementary school.', 'Repaired', GETDATE(), @UserId, 45.1162, -122.8980, 'https://placehold.co/600x400?text=Crosswalk'),
('Small pothole near the elementary school.', 'In Repair', GETDATE(), @UserId, 45.1162, -122.8980, 'https://placehold.co/600x400?text=Pothole+2');

-- user points
INSERT INTO [UserPoints] ([UserId], [CurrentPoints], [LifetimePoints], [LastUpdated])
VALUES (@UserId, 150, 200, GETDATE());

-- leaderboard
INSERT INTO [Leaderboards] ([UserId], [TotalPoints], [LastUpdated])
VALUES (@UserId, 200, GETDATE());