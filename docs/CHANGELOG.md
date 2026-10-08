# Refactor changes

## Structure
- Main .NET 10 solution excludes the Windows-only Crystal host; a separate Windows solution retains it.
- Removed unused MVC 5 and old ASP.NET HTTP package dependencies from application/infrastructure layers.
- Employee create DTO no longer inherits persistence/audit fields.
- Consolidated employee image writes in IFileStorageService.

## Correctness and completion
- Restored AttendanceStatus enum to match services and the last database migration.
- Payroll hour convenience properties are computed from minutes and not persisted; unused Remarks remains unmapped to preserve the uploaded schema.
- Removed duplicate employee inverse relationships; added migration and updated snapshot.
- Added filtered uniqueness for active attendance, employee/year/type leave balances and employee/period payrolls.
- Added leave balance/status and payroll status optimistic concurrency metadata and HTTP 409 feedback.
- Added profile editing, password changes, operational settings and leave allocation screens.
- Implemented all five native report choices, PDF and XLSX exports; wired report-type selection into all export forms.
- Corrected position lookups and position-name uniqueness scope.
- Added employee assignment/number validation, attendance edit validation, paging bounds and cross-year leave validation.
- Prevented ordinary employees from browsing organization-wide HR/leave/dashboard data.
- Permanent deletion requires administrator and no historical business records.
- Added safer CSV export, uploads, correlation IDs and audit handling.
- Replaced hard-coded administrator password with user-secret/environment configuration; roles and administrator writes check IdentityResult errors.
- Added global authentication and antiforgery defaults; native reporting is the default provider.
- Added regression tests and a Windows validation script/CI workflow.

## Verification limits
No .NET build, xUnit execution, SQL migration, Crystal report rendering, IIS deployment or browser interaction was possible in the editing environment. The source/package checks are recorded in VALIDATION.md. Automated tests and manual acceptance steps must be run on your development machine.
