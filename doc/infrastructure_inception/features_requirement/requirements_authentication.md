# Requirements Workup: Feature - "User Authentication & Authorization"

## Elicitation
Goal: Determine how users create accounts, log in, and receive permissions so we can secure the application and enable gamification features.

1. What information is required to create an account?
   - A: We need a unique identity to track points and "reputation."
   - Solution: Users must provide a unique Username, a valid Email address, and a Password.

2. Do we need different types of users?
   - A: Yes. We need regular users to report issues, but we also need people to ban bad actors or verify reports.
   - Solution: We will implement a Role-based system: "Standard User" (default), "Moderator" (can verify/flag), and "Administrator" (can ban users/manage roles).

3. Can non-logged-in users see the map?
   - A: Yes, the goal is community awareness. The feed and map should be public.
   - Solution: "Read-only" access is allowed for guests. "Write" access (submitting, voting, commenting) requires a valid session.

4. How do we handle passwords?
   - A: Storing plain text passwords is a security vulnerability.
   - Solution: We will utilize ASP.NET Core Identity to handle password hashing, salting, and session management automatically.

## Analysis
Constraints & Bounds:
1.  **Unique Constraints:** Emails and Usernames must be unique across the system.
2.  **Session Security:** Users must be automatically logged out after a period of inactivity (e.g., 30 minutes) or upon closing the browser, unless "Remember Me" is checked.
3.  **Role Hierarchy:**
    * *Guest:* View Map/Feed.
    * *User:* Submit Report, Upvote, Comment.
    * *Moderator:* Change Report Status, Hide Comments.
    * *Admin:* Ban User, Promote/Demote User Roles.

**Conflicting Behaviors:**
* *Conflict:* We want users to sign up quickly to report potholes immediately (on the road), but we need secure, complex passwords to prevent hacking.
* *Resolution:* We will enforce standard complexity (Upper, Lower, Number, Special Char) but keep the registration form short (minimal fields). We will delay "Profile Setup" (Bio/Avatar) until *after* registration.

**Missing Items Discovered:**
* The original needs list mentioned "Ability to flag/report/ban users," but we didn't define *how* an Admin finds a user to ban. We need a "User Management" view for Administrators.

## Design and Modeling

**Entities:**

1.  **ApplicationUser** (Extends ASP.NET IdentityUser)
    * `Id`: GUID (PK)
    * `UserName`: STRING
    * `Email`: STRING
    * `PasswordHash`: STRING
    * `ReputationScore`: INT (Default 0)
    * `ProfilePictureUrl`: STRING (Nullable)
    * `Bio`: STRING (Nullable)
    * `CreatedAt`: DATETIME

2.  **ApplicationRole** (Extends ASP.NET IdentityRole)
    * `Id`: GUID (PK)
    * `Name`: STRING (e.g., "Admin", "Moderator")

3.  **UserRoles** (Join Table)
    * `UserId`: GUID (FK)
    * `RoleId`: GUID (FK)

**Relationships:**
* **User to Role:** Many-to-One (User can  only have one role, but a role can have many users)
* **User to Report:** One-to-Many (One User submits many Reports).

## Analysis of the Design

1.  **Does it support the requirements?**
    * *Yes.* The `ApplicationUser` entity supports the "Leaderboard" feature via the `ReputationScore` field. The Role system allows us to protect specific routes (like the Ban User function) using `[Authorize(Roles = "Admin")]` attributes.

2.  **Can it be done correctly/easily?**
    * *Yes.* ASP.NET Core MVC has built-in scaffolding for Identity. We can generate the Login/Register views automatically and then style them to match our "Dynamic Engineering Squad" theme.