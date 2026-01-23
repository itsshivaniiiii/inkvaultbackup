# InkVault Login Crash Troubleshooting Guide

## Quick Diagnosis Steps

### Step 1: Check for Exception Details
1. Open **Event Viewer** (Windows Key ? "Event Viewer")
2. Navigate to **Windows Logs ? Application**
3. Look for entries from "InkVault" or ".NET Runtime"
4. Copy the error details

### Step 2: Enable Detailed Logging
Add this to `Program.cs` to see more detailed errors:

```csharp
// After var app = builder.Build();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerPathFeature =
            context.Features.GetRequiredFeature<IExceptionHandlerPathFeature>();

        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        
        logger.LogError(exceptionHandlerPathFeature.Error, 
            "Unhandled exception occurred: {Message}", 
            exceptionHandlerPathFeature.Error.Message);

        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new 
        { 
            message = "An error occurred", 
            error = exceptionHandlerPathFeature.Error.Message 
        });
    });
});
```

### Step 3: Verify Database State

Run these SQL queries to check:

```sql
-- Check if DateOfBirth column exists
SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AspNetUsers' 
AND COLUMN_NAME IN ('DateOfBirth', 'Age', 'LastBirthdayEmailSent');

-- Check user count
SELECT COUNT(*) as UserCount FROM AspNetUsers;

-- Check for any NULL values in critical columns
SELECT Id, UserName, Email, DateOfBirth 
FROM AspNetUsers 
LIMIT 10;
```

### Step 4: Check BirthdayBackgroundService

The background service might be crashing on startup. Try temporarily disabling it:

```csharp
// In Program.cs, comment out this line temporarily:
// builder.Services.AddHostedService<BirthdayBackgroundService>();
```

Then test login.

## Common Issues & Fixes

### Issue: "Column does not exist: DateOfBirth"
**Solution:**
```powershell
dotnet ef database update
```

### Issue: "UserManager is null"
**Solution:** Verify in Program.cs:
```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
```

### Issue: "SignInManager failed"
**Solution:** Check that services are registered:
```csharp
builder.Services.ConfigureApplicationCookie(options => { ... });
```

### Issue: Theme JavaScript error
**Check** `Views/Shared/_Layout.cshtml`:
- Ensure Url.Action calls are correct
- Check for null reference errors in theme initialization script

## Quick Fix Checklist

- [ ] Run `dotnet ef database update`
- [ ] Verify database migrations are applied
- [ ] Check Event Viewer for exception details
- [ ] Disable BirthdayBackgroundService temporarily
- [ ] Clear browser cache (Ctrl+Shift+Delete)
- [ ] Restart Visual Studio
- [ ] Rebuild solution (Ctrl+Shift+B)
- [ ] Delete `bin` and `obj` folders, rebuild

## Getting Detailed Error Information

### Option A: Use Debugging
1. Open AccountController.cs
2. Set breakpoint on `Welcome` action
3. Press F5 to debug
4. The exact error will show in Visual Studio

### Option B: Add Try-Catch
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Welcome(WelcomeViewModel model)
{
    try
    {
        // ... existing code ...
    }
    catch (Exception ex)
    {
        // Log the exception
        System.Diagnostics.Debug.WriteLine($"Login Error: {ex}");
        ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
        return View(model);
    }
}
```

## Next Steps

1. **First:** Run the migration
   ```powershell
   dotnet ef database update
   ```

2. **Second:** Check Event Viewer for the exact error

3. **Third:** Let me know the exact error message, and I'll provide a targeted fix
