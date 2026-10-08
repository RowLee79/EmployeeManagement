# Employee Management — refactored source

ASP.NET Core MVC on .NET 10 with SQL Server, EF Core, ASP.NET Core Identity, Serilog and QuestPDF. The main solution contains the Domain, Application, Infrastructure and Web projects plus automated tests. The existing MVC screens and uploaded employee photos are retained.

## Validation status

Source/configuration/package checks were completed during refactoring. **The application has not been compiled or run in the editing environment:** no .NET SDK or SQL Server is installed and SDK downloads are blocked. New xUnit tests are included but have not been executed. Run the checks below before using this version against your working database. See `docs/VALIDATION.md` for the exact remaining checks.

## Start on Windows with SQL Server Express

1. Extract the ZIP. Open a terminal in the `EmployeeManagement` folder. Install a .NET 10 SDK and make your SQL Server instance available.
2. Configure the connection and first administrator in user secrets. Replace the sample connection with your actual instance/database. For an existing installation, use its database name rather than accidentally creating a new empty database.

```powershell
$web = 'src/EmployeeManagement.Web/EmployeeManagement.Web.csproj'
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'Server=localhost\SQLEXPRESS;Database=EmployeeManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True' --project $web
dotnet user-secrets set 'SeedAdmin:Email' 'your-admin@example.com' --project $web
dotnet user-secrets set 'SeedAdmin:Password' 'YOUR-OWN-STRONG-PASSWORD' --project $web
```

The password must meet Identity's configured requirements: 8+ characters, upper/lowercase, a digit and a symbol. There is no built-in administrator password. Existing administrator passwords are not reset. Roles are seeded independently; the administrator is created only when its email is configured.

3. Restore, build and run tests:

```powershell
dotnet restore EmployeeManagement.slnx
dotnet build EmployeeManagement.slnx -c Release --no-restore
dotnet test EmployeeManagement.slnx -c Release --no-build --logger 'trx;LogFileName=results.trx'
```

Or run `powershell -ExecutionPolicy Bypass -File scripts/Run-Checks.ps1`.

4. Start the Web project:

```powershell
dotnet run --project src/EmployeeManagement.Web --launch-profile https
```

Open the URL printed by the application and sign in with your configured administrator. Startup applies migrations to the business and Identity contexts. `Database:SeedDemoData` defaults to false. On a new disposable database, enable sample departments, positions, employees and balances using:

```powershell
dotnet user-secrets set 'Database:SeedDemoData' 'true' --project $web
```

Turn it off again after first setup. The demo seeder was retained from the original project; it is intended for an empty demo database, not partially populated business data.

## Existing database upgrade

Use a restored copy of your working database to validate first. The new migration removes duplicate navigation foreign keys `DepartmentId1` and `PositionId1`; actual assignments in `DepartmentId` and `PositionId` remain authoritative. It also adds unique active-record indexes for attendance, leave balances and payroll per period.

The migration stops if active duplicates already exist. It does not silently delete records. Inspect duplicates with `docs/DUPLICATE-CHECK.sql`, review the records and correct them through a reviewed business-data process before retrying. Keep an existing database backup. Rolling this migration down recreates the redundant columns empty; it does not restore their old values.

To inspect the migration plan without starting the app:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
dotnet ef migrations script --idempotent --context EmployeeDbContext --project src/EmployeeManagement.Infrastructure --startup-project src/EmployeeManagement.Web --output employee-upgrade.sql
dotnet ef migrations script --idempotent --context ApplicationIdentityDbContext --project src/EmployeeManagement.Infrastructure --startup-project src/EmployeeManagement.Web --output identity-upgrade.sql
```

The dependency versions above match the uploaded project. Restore access to its NuGet package versions is required.

## Completed and corrected workflows

- Employee create/edit with assignment validation; archive/restore; safe historical-record checks before permanent deletion.
- Correct department/position employee counts through explicit inverse relationships.
- Department-scoped position names, unique codes, and active lookup filtering.
- Attendance enum aligned with existing SQL integer storage; time validation, employee validation on edit, bounded paging, and replacement after soft deletion.
- Leave requests, approvals and cancellations; a new Leave Allocations page for HR/administrators, including new employees and future years. Cross-year requests must be split.
- Payroll calculations and period lifecycle retained, with concurrency checks on status transitions and duplicate payroll prevention per employee/period.
- My Profile and Change Password pages. Employee-only accounts land on My Profile; staff management/leave/dashboard screens require a staff role.
- Administrator settings page showing active operational settings without exposing secrets.
- Native reports: master list, summary, by department, by status, and new hires. PDF preview/download/print and real `.xlsx` export. New hires defaults to the last 30 days when no From date is supplied.
- Unified image storage, extension/signature/size validation, constrained deletion, cleanup on failed employee updates, and replacement-image cleanup.
- Default authentication and antiforgery validation, conflict feedback, audited soft deletion, and safer correlation IDs.

## Payroll configuration and boundaries

The uploaded payroll rates and tax brackets were absent, so this version retains zero contribution rates and disabled withholding until you supply configuration. The existing engine uses configured flat contribution rates, a capped Pag-IBIG contribution, and an optional configurable tax table. It is **not a certified statutory payroll implementation**. It does not automatically prorate monthly salaries by payroll period or derive overtime/absence pay from attendance. Validate your organization's formulas and contribution/tax schedules before operational payroll use.

Configuration keys are `Payroll:Deductions:SssEmployeeRate`, `PhilHealthEmployeeRate`, `PagIbigEmployeeRate`, `PagIbigMaximumContribution`, `CalculateWithholdingTax`, and the `Payroll:TaxBrackets` array (`MinimumIncome`, `MaximumIncome`, `BaseTax`, `TaxRate`, `ExcessOver`). Rates are decimals, e.g. `0.01` means 1%. No jurisdiction-specific rates have been supplied by this refactor.

Leave day calculation retains the original Monday–Friday rule and does not use a holiday calendar. Settings display is read-only. The Employee role has profile/password access, not a full employee self-service portal. Image signatures reject obvious disguised files but are not a complete image decoder or re-encoder. Large native reports materialize their filtered records in memory; apply filters for large datasets.

## Optional Crystal Reports

Native reporting is the default and requires no Crystal installation. The original `.rpt` files and ASP.NET MVC 5/.NET Framework 4.8.1 project are preserved under `src/EmployeeManagement.CrystalReport`. Open `EmployeeManagement.Windows.slnx` in Visual Studio with the ASP.NET web workload, .NET Framework targeting pack and compatible SAP Crystal Reports developer/runtime components. Restore its `packages.config` packages separately and configure its `Web.config` connection string and IIS identity.

For the original master-list report only, set `Reporting:Provider` to `Crystal` and `Reporting:BaseUrl` to the hosted reporting application. The original remote service exports `.xls`; native exports `.xlsx`. Advanced report-selector choices are supported by the native provider. The Crystal host was not executed or changed into a cross-platform service. Do not expose the legacy reporting host publicly without adding service authentication.

`src/EmployeeManagement.Reporting` is an unused legacy prototype and is not part of either build solution.

## Deployment

Publish the main Web project with `dotnet publish src/EmployeeManagement.Web -c Release -o publish`. For IIS, install the .NET 10 Hosting Bundle, create the site and give its application-pool identity SQL access plus write access to `logs` and `wwwroot/uploads/employees`. Configure production settings via environment variables (for example `ConnectionStrings__DefaultConnection`), not development user secrets. Persist uploaded photos and Identity/Data Protection keys across redeployments. Keep `Database:SeedDemoData=false` in production. Review the QuestPDF license configuration in `Program.cs` against your organization's eligibility.

See `docs/CHANGELOG.md`, `docs/VALIDATION.md` and the test projects for details.
