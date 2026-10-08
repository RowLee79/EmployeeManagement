# Validation and acceptance

## Completed in the editing environment
- Extracted source files while excluding generated bin/obj/.vs content.
- Parsed JSON configurations, XML project/solution/configuration files.
- Checked core solution/project references and new navigation destinations.
- Checked report forms transmit ReportType and native Excel exports use XLSX MIME/extension.
- Checked attendance enum/storage alignment and removal of literal default administrator credentials.
- Checked generated archive integrity and required deliverables.

These are source checks, not a C# compiler or application test run.

## Verified on GitHub Actions

On 2026-10-08, commit `630c982eceda88d83287fc4788ba51c01cc5f8da` built successfully with .NET 10 and passed 22 tests (8 unit, 14 integration). The UI workflow started the app against a fresh SQL Server 2022 demo database, completed startup migrations and captured eight actual UI pages after sign-in.

- [Build and tests](https://github.com/RowLee79/EmployeeManagement/actions/runs/37729515680)
- [Demo UI capture](https://github.com/RowLee79/EmployeeManagement/actions/runs/37729515632)

## Repeat automated checks
Run scripts/Run-Checks.ps1. It restores, builds and runs xUnit tests, stopping on the first failing command. The included regression tests cover inverse employee counts, attendance duplicate/replacement behavior, missing employee edits, leave allocation/approval/cancellation and stale balance writes, SQL Server model-vs-migration consistency, every native report choice, spreadsheet XML escaping/formula safety and image/path validation. Existing payroll deduction/tax tests remain.

The model consistency test uses the SQL Server provider without opening a connection. In-memory tests do not prove SQL Server index enforcement or migration behavior; check those against a disposable database.

## Manual acceptance against SQL Server
1. Start on a new database with supplied administrator credentials. Sign in and confirm roles. Sign out and verify unauthenticated management requests redirect to login.
2. Create departments and positions; use the same position name in two departments with distinct codes. Create an employee and verify both counts. Reject a mismatched department/position and duplicate employee number.
3. Edit/upload a valid photo, replace it and confirm old upload cleanup. Reject a text file renamed PNG and files over 5 MB. Profile/password forms should save and report validation errors.
4. Create working and absent attendance, check late/undertime values, reject reversed/missing times and duplicates, archive a record and recreate its date.
5. Allocate leave to a new employee; create a request, approve it and cancel it. Used/remaining balance must change and restore. Reject allocations below used days and requests crossing calendar years.
6. Configure payroll formulas on a test database. Exercise period generation, calculation, approval, finalization and locking. Confirm duplicate generation does not create duplicate employee/period records. Confirm stale lifecycle updates return a conflict.
7. Preview/download each native report type, apply every filter and open XLSX in Excel. Print opens a PDF for browser printing. Validate zero-match reports. Optional Crystal master-list exports require its separate Windows host.
8. Archive/restore an employee. Permanent deletion with historical payroll/leave/attendance must be refused. Employee-only accounts should have profile access and be denied HR/leave management.
9. Repeat migration on a restored existing database. Check assignments/counts and payroll/leave records. Inspect unique-index duplicate preflight errors and resolve data before retrying. Do not test by deleting existing payroll records.
10. Verify IIS SQL/file permissions and retained upload/key storage on the target server.

## Remaining product boundaries
Read README.md for payroll formulas, holiday calendars, employee self-service, large report memory use, legacy Crystal hosting and settings-editing scope. These have not been presented as completed statutory/compliance features.
