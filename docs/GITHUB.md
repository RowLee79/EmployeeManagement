# Publish to an existing GitHub repository

The project has not been pushed by this editing session. Publishing requires your repository URL and authenticated GitHub access.

## Empty existing repository

Open PowerShell in the extracted EmployeeManagement project root. Replace `YOUR-USERNAME` and `YOUR-REPOSITORY` with your real repository URL.

```powershell
git init -b main
git status --short
git add .
git diff --cached --stat
git commit -m "Add Employee Management application and documentation"
git remote add origin https://github.com/YOUR-USERNAME/YOUR-REPOSITORY.git
git push -u origin main
```

When Git prompts you, sign in through Git Credential Manager. If commit reports that your identity is missing, configure your own name and GitHub email locally in the repository:

```powershell
git config user.name 'YOUR NAME'
git config user.email 'YOUR GITHUB EMAIL OR NOREPLY ADDRESS'
```

Then retry the commit/push. Do not put a password/token in README, a remote URL, or project configuration.

## Repository already has files or commits

Clone it first so its existing history and files remain available:

```powershell
git clone https://github.com/YOUR-USERNAME/YOUR-REPOSITORY.git
cd YOUR-REPOSITORY
git switch -c add-employee-management
```

Copy the project into this clone, preserving its `.git` folder. Review collisions with an existing README or source files before replacing them. Then:

```powershell
git status --short
git add .
git diff --cached --stat
git commit -m "Add Employee Management application and documentation"
git push -u origin add-employee-management
```

Open GitHub and create a pull request from this branch. Do not force-push to replace an existing repository's history.

## Add UI screenshots

Capture the app using SCREENSHOTS.md, then commit the generated gallery and images:

```powershell
git add README.md docs/screenshots
git commit -m "Add application UI screenshots"
git push
```

The updated `.gitignore` excludes build output, development secrets, node_modules, authentication files and runtime employee photo uploads. The `docs/screenshots` folder is deliberately included in Git.

A GitHub repository stores the source; it does not host the running ASP.NET Core/SQL Server application. Deployment instructions are in SETUP.md.
