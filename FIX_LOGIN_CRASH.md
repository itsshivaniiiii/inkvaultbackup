# How to Fix the Login Crash - Step by Step

## Problem
The application crashed with exit code `0xffffffff` when attempting to login, and the `dotnet ef` command was not found.

## Solution

### Step 1: Install Entity Framework Core CLI Tools

Run this command in PowerShell (as Administrator):

```powershell
dotnet tool install --global dotnet-ef
```

If you already have it installed but it's outdated:

```powershell
dotnet tool update --global dotnet-ef
```

### Step 2: Run Database Migrations

**Option A: Using Package Manager Console (Easiest)**

1. Open Visual Studio
2. Go to **Tools ? NuGet Package Manager ? Package Manager Console**
3. Make sure "InkVault" is selected as the Default Project
4. Run this command:

```powershell
Update-Database
```

**Option B: Using PowerShell / Command Line**

```powershell
cd C:\path\to\InkVault  # Navigate to your project directory
dotnet ef database update
```

### Step 3: Verify Migration Success

If successful, you should see:
- No errors in the console
- Message like: "Done. To undo this action, use 'ef migrations remove'"
- Or: "Build started..."

### Step 4: Rebuild and Test

1. **Clean the solution:**
   - Delete `bin` and `obj` folders
   - Or run: `dotnet clean`

2. **Rebuild:**
   - Press `Ctrl+Shift+B` in Visual Studio
   - Or run: `dotnet build`

3. **Run the app:**
   - Press `F5` or `Ctrl+F5`
   - Try logging in again

---

## Troubleshooting

### Issue: "Build Failed" after migration

**Solution:** Run these commands:
```powershell
dotnet clean
dotnet build
```

### Issue: "Database is already up to date"

**This is good!** The database already has the migrations applied. Just rebuild and test.

### Issue: "Cannot find database"

**Solution:** Check your `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=InkVault;Trusted_Connection=true;"
  }
}
```

Make sure the database name and connection string are correct.

### Issue: "The entity type ApplicationUser cannot be used"

**Solution:** This is usually resolved by running the migration. If it persists:
1. Delete the database file
2. Run `Update-Database` again

---

## Detailed Steps for Package Manager Console

1. **Open Package Manager Console:**
   - Visual Studio ? Tools ? NuGet Package Manager ? Package Manager Console

2. **Select Default Project:**
   - In the PM Console window, find the dropdown at the top
   - Select: **InkVault**

3. **Run Update Command:**
   ```powershell
   Update-Database
   ```

4. **Wait for completion:**
   - You should see: "Done."

---

## What This Does

The migration adds:
- ? `DateOfBirth` column to AspNetUsers table
- ? `LastBirthdayEmailSent` column for birthday email tracking

These columns are used for:
- User registration/profile with date of birth
- Automatic birthday email feature

---

## Quick Checklist

- [ ] Installed EF CLI tools: `dotnet tool install --global dotnet-ef`
- [ ] Ran migration: `Update-Database` in PM Console
- [ ] Migration shows "Done." message
- [ ] Cleaned and rebuilt solution
- [ ] Try logging in

---

**If you're still having issues:**
1. Take a screenshot of the error message
2. Check Event Viewer (Windows Key ? "Event Viewer" ? Windows Logs ? Application)
3. Look for detailed error messages
4. Share the error message in your next update

The AccountController.cs file has been cleaned up and should work now! ??
