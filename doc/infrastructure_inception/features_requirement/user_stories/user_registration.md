# ID 1

## Title

*As a user, I want to create an account, so that I can begin to use the application.*

## Description

This user story is about creating an account so that the user can log in and begin using the app. They should be able to enter their username, email, password, and password confirmation for future logging in.
 
### Details:

THe user should be able to enter the following data:
1. *Username* - A username that the user will be referred to by in the application, i.e. "RoadWarrior2026" or similar. This cannot be empty or nulled.
2. *Email* - This is used for logging in and receiving information for the user. This cannot be empty or nulled.
3. *Password* - Their password to be hashed by the ASP.NET Identity algorithm. Cannot be empty or nulled.
4. *Password Confirmation* - A field which verifies that the user entered the correct password they wantd to be entered. This cannot be empty or nulled.

Registration should be fast and easy with a simple form field and a submit button. This should be done using a POST action, with them being redirected to the homepage after registration so they can begin logging in.

#### Implementation Details

1. Use ASP.NET Identity NUGET package to implement identity.

## Acceptance Criteria

Given the user wants to register,
When the user clicks the register button,
Then they are redirected to a page with the registration form.

Given the user is on the registration page,
When the user enters their information,
And clicks submit,
Then they are redirected to the homepage.

Given the user is on the registration page,
When the user leaves a field blank,
And clicks submit,
Then the blank field shows an error.

Given the user has submitted a registration,
When the application receives the POST action,
Then the application saves their data to the database.

## Assumptions/Preconditions
None

## Dependencies
Initial Application Setup

## Effort Points
4

## Owner

## Git Feature Branch
`feature-registration`

## Tasks
1. Set up Identity ASP.NET NUGET package
2. Use EF Core Migrations for Identity ASP.NET to Database