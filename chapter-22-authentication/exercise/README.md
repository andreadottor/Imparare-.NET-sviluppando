# TicketManager authorization exercise

The `ManageTickets` policy requires the `TicketManager` role. Existing Blazor controls, ticket editing routes, the delete event handler, and Minimal API write endpoints continue to use that same policy. Ticket reading remains public, and account management requires authentication without requiring a role.

## Development setup

Run the following commands from this exercise directory. Configure the existing SQL Server connection string and apply both contexts' migrations before starting the application. The Identity migration already includes the role tables; no new migration is needed for this exercise.

- Set the connection string locally: `dotnet user-secrets set "ConnectionStrings:Ticketing" "<your SQL Server connection string>" --project Ticketing.Web`
- Apply Identity migrations: `dotnet ef database update --project Ticketing.Web --startup-project Ticketing.Web --context ApplicationDbContext`
- Apply ticket migrations: `dotnet ef database update --project Ticketing.Application --startup-project Ticketing.Web --context TicketingDbContext`
- Store the promotion email in User Secrets: `dotnet user-secrets set "IdentityDevelopment:TicketManagerEmail" "<registered user's email>" --project Ticketing.Web`

Replace the placeholders with local values. Do not put the email or credentials in tracked appsettings files.

Start the application in `Development`. The initializer creates `TicketManager` only if missing. If the configured user exists, it assigns the role only if the user is not already a member. Failed Identity operations are reported rather than ignored. Without a configured email, only the role is created; an unknown email produces a warning and does not create an account.

If the user has not registered yet, register and confirm the account using the existing flow, then restart the application to assign the role. Sign out and sign back in after promotion so the authentication cookie includes the role. Initialization never runs outside `Development`. Repeated startup does not duplicate the role or membership. Changing the configured email does not revoke previously assigned roles.

## Verification

| User | Read tickets (UI and GET API) | Manage own account | Create, edit, delete tickets |
| --- | --- | --- | --- |
| Anonymous | Allowed | Sign-in required | Denied |
| Authenticated, no roles | Allowed | Allowed | Denied |
| Authenticated, TicketManager | Allowed | Allowed | Allowed |

Check `/tickets` and `GET /api/tickets` without signing in. For an authenticated user without roles, verify account access, hidden write controls, denied `/tickets/new` and edit routes, and HTTP 403 from valid `POST /api/tickets` and `DELETE /api/tickets/{code}` requests. The existing Blazor delete handler also authorizes `ManageTickets` before invoking the ticket service; hiding a button is not the security boundary.

Repeat with the promoted user after a fresh sign-in: create, edit and delete through Blazor, and create/delete through the existing API. Restart twice to verify the same role and membership remain without duplicate-creation errors. Finally, run outside `Development` to verify that startup does not create roles or promote users.
