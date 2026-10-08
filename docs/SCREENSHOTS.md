# Capture UI screenshots

Capture genuine screenshots from the running app. No screenshots were generated in the editing environment because it cannot run the .NET application. The README gallery intentionally has no broken image links.

## Automatic capture on your PC

Start the Web application as described in SETUP.md. Use a demo database and an administrator account so the screens are available without exposing working employee records.

In a second PowerShell terminal, from the project root:

```powershell
cd .\tools\ui-screenshots
npm install
npx playwright install chromium
npm run capture -- https://localhost:7029
```

Use the actual URL printed by `dotnet run` if it differs. The tool opens Chromium, saves a clean login screen, and waits while you sign in manually. After signing in, return to PowerShell and press Enter. Your password is not passed to or saved by the script. The tool captures Dashboard, Employees, Attendance, Leave, Leave Allocations, Payroll and Reports, then inserts the gallery into the root README.

Images are saved at `docs/screenshots/*.png`, relative to the project root. Review them before committing. This is a screenshot utility, not proof that every workflow is correct.

## Manual alternative

Open each screen and use Windows Snipping Tool (`Win + Shift + S`). Save PNG files using these exact names:

| Screen | Path |
| --- | --- |
| Login | `docs/screenshots/login.png` |
| Dashboard | `docs/screenshots/dashboard.png` |
| Employees | `docs/screenshots/employees.png` |
| Attendance | `docs/screenshots/attendance.png` |
| Leave | `docs/screenshots/leave.png` |
| Leave allocations | `docs/screenshots/leave-allocations.png` |
| Payroll | `docs/screenshots/payroll.png` |
| Reports | `docs/screenshots/reports.png` |

After saving any of these images:

```powershell
node .\tools\ui-screenshots\update-readme.mjs
```

This command has no npm dependency. Only existing screenshot files are added to README.md.

The capture utility uses Playwright's documented [page screenshot API](https://playwright.dev/docs/api/class-page#page-screenshot).

## GitHub Actions demo capture

The `Capture demo UI screenshots` pull-request workflow builds the app and starts it against an isolated SQL Server demo database. It creates a temporary administrator and uses environment variables to sign in automatically. Completed UI images and the updated README are uploaded as the `employee-management-ui` workflow artifact. It does not write to the repository automatically. The workflow may fail if build, startup or browser checks fail; inspect its logs before treating the images as valid.
