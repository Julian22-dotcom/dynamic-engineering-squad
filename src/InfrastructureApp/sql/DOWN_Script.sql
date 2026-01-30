-- Drop child/dependent tables first
DROP TABLE IF EXISTS [Leaderboards];
DROP TABLE IF EXISTS [UserPoints];
DROP TABLE IF EXISTS [Reports];
DROP TABLE IF EXISTS [AspNetUserRoles];

-- Drop parent/identity tables last
DROP TABLE IF EXISTS [AspNetUsers];
DROP TABLE IF EXISTS [AspNetRoles];