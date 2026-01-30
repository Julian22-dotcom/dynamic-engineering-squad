**Entities:**

1.  **AspNetUsers**
    * `Id`: NVARCHAR(450) (PK)
    * `UserName`: NVARCHAR(256)
    * `NormalizedUserName`: NVARCHAR(256)
    * `Email`: NVARCHAR(256)
    * `NormalizedEmail`: NVARCHAR(256)
    * `PasswordHash`: NVARCHAR(MAX)
    * `SecurityStamp`: NVARCHAR(MAX)
    * `ConcurrencyStamp`: NVARCHAR(MAX)
    * `PhoneNumber`: NVARCHAR(MAX) NULLABLE
    * `TwoFactorEnabled`: BIT
    * `LockoutEnd`: DATETIMEOFFSET
    * `LockoutEnabled`: BIT
    * `AccessFailedCount`: INT

2.  **AspNetRoles**
    * `Id`: NVARCHAR(450) (PK)
    * `Name`: NVARCHAR(256)
    * `Normalizedname`: NVARCHAR(256)
    * `ConcurrencyStamp`: NVARCHAR(MAX)

3.  **AspNetUserRoles** (Join table)
    * `UserId`: NVARCHAR(450) (PK, FK -> AspNetUsers.Id)
    * `RoleId`: NVARCHAR(450) (PK, FK -> AspNetRoles.Id)

4.  **Reports** 
    * `Id`: INT (PK, Identity)
    * `Description`: NVARCHAR(MAX)
    * `Status`: NVARCHAR(50) (Pending, Approved, Rejected)
    * `CreatedAt`: DATETIME2
    * `UserId`: NVARCHAR(450) (FK -> AspNetUsers.Id)
    * `Latitude`: DECIMAL(9, 6)
    * `Longitude`: DECIMAL(9, 6)
    * `ImageUrl`: NVARCHAR(450) (Link to Azure Blob)

5.  **UserPoints** 
    * `Id`: INT (PK, Identity)
    * `UserId`: NVARCHAR(450) (FK -> AspNetUsers.Id)
    * `CurrentPoints`: INT
    * `LifetimePoints`: INT
    * `LastUpdated`: DATETIME2

6.  **Leaderboards** 
    * `Id`: INT (PK, Identity)
    * `UserId`: NVARCHAR(450) (FK -> AspNetUsers.Id)
    * `TotalPoints`: INT
    * `LastUpdated`: DATETIME2