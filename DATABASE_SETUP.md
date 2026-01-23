# Database Migration Commands

## Prerequisites
Ensure the InkVault project is selected as the Default project in Package Manager Console.

## If You Haven't Run Migrations Yet

Run these commands in the **Package Manager Console**:

```powershell
# Add a migration to create the initial database
Add-Migration InitialCreate -Project InkVault

# Apply the migration to create the database
Update-Database -Project InkVault
```

## If You Need to Update for OTP Fields

If you already have an older version of the database without OTP fields, run:

```powershell
# Create migration for OTP fields
Add-Migration AddOTPAndEmailVerifiedFields -Project InkVault

# Update database
Update-Database -Project InkVault
```

## Verify Database Creation

1. Open **SQL Server Object Explorer**
2. Navigate to: `(localdb)\MSSQLLocalDB` ? Databases
3. Look for `UserAuthDb` database
4. Expand it to verify tables are created

## Check ApplicationUser Table Structure

The `AspNetUsers` table should have these columns:
- `Id` (PK)
- `UserName`
- `Email`
- `PasswordHash`
- `FirstName`
- `LastName`
- `Gender`
- `PhoneNumber`
- `ProfilePicturePath`
- `OTP` (nullable)
- `OTPExpiration` (nullable)
- `EmailVerified` (default: false)

## Reset Database (Development Only)

If you need to completely reset the database:

```powershell
# Remove the database (careful!)
Remove-Migration -Project InkVault -Force

# Create fresh migration
Add-Migration InitialCreate -Project InkVault

# Create database
Update-Database -Project InkVault
```

## Alternative: Using dotnet CLI

If you prefer using the command line instead of Package Manager Console:

```bash
cd path/to/InkVault

# Create migration
dotnet ef migrations add InitialCreate -p InkVault.csproj

# Update database
dotnet ef database update
```

## Connection String Reference

The connection string in `appsettings.Development.json`:
```
Server=(localdb)\MSSQLLocalDB;Database=UserAuthDb;Trusted_Connection=True;TrustServerCertificate=True
```

This uses **SQL Server LocalDB** which is built-in to Visual Studio.

## Troubleshooting

### "The name 'AddMigration' is not recognized"
- Open **Package Manager Console** from Tools ? NuGet Package Manager
- Make sure you're in the correct directory

### "There is already an object named 'AspNetUsers'"
- Your database already exists
- You can either use `Update-Database` to apply pending migrations
- Or delete the database from SQL Server Object Explorer and recreate it

### "Migrations history does not exist"
- First time using migrations with this database
- Run `Update-Database` which will create the `__EFMigrationsHistory` table

## Next Steps After Migration

1. ? Migrations complete
2. ?? Update `appsettings.Development.json` with SMTP settings
3. ?? Run the application
4. ?? Test the registration flow
