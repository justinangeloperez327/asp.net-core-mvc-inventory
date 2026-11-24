# Inventory MVC
ASP.NET Core 10 MVC, EF Core, SQLite.

## Setup
Install .NET 10 SDK. Generate a password hash with `dotnet run -- --hash-password`. Set InventoryUsers__Admin__Username and InventoryUsers__Admin__PasswordHash environment variables before first startup. Run `dotnet run` and open http://localhost:5187. Configuration accounts are imported only when the users table is empty. Thereafter use the Users screen to manage accounts. Do not commit credentials.

Admin manages users, master data and approvals; Operator records inventory and purchases; Viewer reads inventory. Changing an account invalidates its existing sessions. Audit records capture actor, operation, entity and key for asynchronous database writes; passwords are not recorded.

Create products, suppliers and warehouses, then record receipts, issues and transfers. Purchase orders are single-line with full receipt. Valuation uses current product price. Back up inventory.db before upgrades.

## Tests
`dotnet test Tests/Inventory.Tests.csproj`. GitHub Actions builds and tests main on push.

Production deployment still requires formal migrations, HTTPS, backups, persistent data-protection keys, login throttling and security review.

## Code formatting

Run from the repository root with the .NET 10 SDK:

```sh
dotnet format InventoryMvc.csproj
dotnet format Tests/Inventory.Tests.csproj
```

CI verifies whitespace formatting for both projects. Roslyn recommended analyzers and code-style diagnostics run during builds. Razor markup is governed by EditorConfig and the IDE Razor formatter, not by dotnet format.
