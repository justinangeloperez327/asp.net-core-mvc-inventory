# Inventory MVC
ASP.NET Core 10 MVC, EF Core and SQLite.

## Run
Install .NET 10 SDK. Configure at least one account before running. No default password is supplied.

Use PowerShell to generate an ASP.NET Identity-compatible password hash with the included command:
```powershell
dotnet run -- --hash-password
```
Set environment variables InventoryUsers__Admin__Username and InventoryUsers__Admin__PasswordHash to your chosen username and generated hash. Operator and Viewer accounts use the same pattern with their respective role names.
Run dotnet run and open http://localhost:5187.

Admin manages master records and approves purchases; Operator records stock and purchases; Viewer reads reports and inventory. Accounts are configuration-based, not a full user administration system.

Create a product, supplier and warehouse; record stock receipts, issues or transfers. Purchase orders are single-line and support draft, approval, receipt and cancellation. Existing unallocated stock is assigned to Main on upgrade. Back up inventory.db before upgrading. CSV valuation uses current product unit price, not FIFO or weighted-average costing.

This is a small inventory MVP. Production deployment requires formal migrations, HTTPS, backups, persistent data-protection keys, account lifecycle management, security review and integration testing.
