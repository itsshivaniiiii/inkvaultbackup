# Remember Me - Development Testing Guide

## The Issue: Why Remember Me Doesn't Work in Development

When running the app in Visual Studio (Debug mode with F5):

### ? Problems
1. **IIS Express Restart** - Cookies cleared when you stop/restart debugging
2. **App Rebuild** - Rebuilding the app clears the application state
3. **HTTPS/HTTP Issues** - Development certificates can interfere
4. **Session Timeout** - App sessions don't persist across debug sessions
5. **Browser Cache** - Old cached content can interfere

### ? Solutions

---

## Solution 1: Test Without Stopping the App (BEST FOR DEVELOPMENT)

This is the recommended way to test "Remember Me" in development:

### Steps:
1. **Start the app** with F5 (Debug mode)
2. **Log in** and check "Remember Me"
3. **Click Login**
4. **Go to a restricted page** (e.g., `/Friends`)
5. **DON'T STOP THE DEBUGGER** - Keep the app running
6. **Close the browser** (Ctrl+W or close the tab)
7. **Reopen the browser** and navigate to the app
8. **You should be automatically logged in** ?

**Why this works:**
- App is still running, so authentication state persists
- Database connection stays active
- Cookies are not cleared

---

## Solution 2: Use Incognito/Private Browsing Mode

Incognito mode helps isolate cookies:

### Steps:
1. **Open Incognito/Private Window** (Ctrl+Shift+N on Windows)
2. **Log in** and check "Remember Me"
3. **Close the entire incognito window** (not just the tab)
4. **Open a new incognito window**
5. **Navigate to the app** (e.g., `https://localhost:7001`)
6. **You should be automatically logged in** ?

**Why this works:**
- Incognito cookies are isolated
- No interference from regular browsing cookies
- Cleaner test environment

---

## Solution 3: Publish to IIS Express (Closest to Production)

For more accurate testing:

### Steps:

1. **Create a local IIS Express binding**:
   - Open Project Properties ? Debug
   - Change from IIS Express default to a custom port
   - Example: `https://localhost:7201`

2. **Set environment to Production** (appsettings.Production.json):
   ```json
   {
     "Environment": "Production"
   }
   ```

3. **Start the app**
4. **Log in with Remember Me**
5. **Close Visual Studio completely**
6. **Wait 5 seconds**
7. **Open Visual Studio again**
8. **Navigate to the app**
9. **You should be automatically logged in** ?

---

## Solution 4: Clear Browser Cookies and Cache

If cookies seem to not be saving:

### Windows:
1. Press `Ctrl+Shift+Delete` to open Clear Browsing Data
2. Select:
   - ? Cookies and other site data
   - ? Cached images and files
3. Set "Time range" to "All time"
4. Click "Clear data"
5. Try logging in again with Remember Me checked

### Mac:
1. Press `Cmd+Shift+Delete` (or use menu: History ? Clear Browsing Data)
2. Select the same options
3. Click "Clear"

---

## Solution 5: Use Different Browser Profiles

Test with isolated browser profiles:

### Google Chrome:
1. Click profile icon (top right)
2. "Add another profile"
3. Create profile named "TestLogin"
4. Use this profile ONLY for testing Remember Me
5. Log in with Remember Me
6. Close browser
7. Reopen with same profile
8. Should be logged in

---

## Debugging: Check If Cookies Are Being Saved

### Using Browser DevTools (F12):

1. Press **F12** to open Developer Tools
2. Go to **Application** tab
3. Expand **Cookies** on the left
4. Click on `localhost:7001` (or your app URL)
5. Look for `.AspNetCore.Identity.Application` cookie

**What to check:**
- ? Cookie exists after login
- ? Has expiration date (14 days from now)
- ? HttpOnly is checked
- ? Path is `/`
- ? Secure is checked (for HTTPS) or unchecked (for HTTP)

**If cookie is NOT there:**
- Check browser's cookie policy settings
- Check if third-party cookies are blocked
- Try private/incognito mode
- Clear all cookies and try again

---

## Console Debug Output

Add this to see what's happening on login:

```csharp
// In AccountController.cs - Login method
if (result.Succeeded)
{
    System.Diagnostics.Debug.WriteLine($"LOGIN SUCCESSFUL");
    System.Diagnostics.Debug.WriteLine($"RememberMe: {model.RememberMe}");
    System.Diagnostics.Debug.WriteLine($"User: {user.UserName}");
    
    user.LastLoginAt = DateTime.UtcNow;
    await _userManager.UpdateAsync(user);
    
    return RedirectToAction("Index", "Home");
}
```

Check **Output Window** (Debug ? Windows ? Output) to see the debug messages.

---

## Common Development Issues & Fixes

| Issue | Cause | Fix |
|-------|-------|-----|
| Cookie not saving | Browser policy | Use Incognito mode |
| Cookie clears on F5 | App restart | Don't stop debugger |
| Cookie invalid | HTTPS cert issue | Use HTTP in development |
| Stays logged in wrong person | Cookies mixed up | Clear all cookies |
| Shows login page still logged in | Session cache | Hard refresh (Ctrl+Shift+R) |

---

## Testing Production-Like Setup

### Using Docker (Optional but Recommended):

1. Build app as Release: `dotnet publish -c Release`
2. Create Docker container
3. Deploy locally
4. Test Remember Me
5. Restart container
6. Should still be logged in ?

---

## Summary: Best Development Testing Approach

**For quick testing:**
1. Keep app running (don't stop debugger)
2. Log in with Remember Me
3. Close browser tab
4. Reopen tab ? Should be logged in ?

**For thorough testing:**
1. Use Incognito/Private mode
2. Log in with Remember Me
3. Close entire incognito window
4. Reopen incognito window
5. Navigate to app ? Should be logged in ?

**For production-like testing:**
1. Stop Visual Studio
2. Wait 5 seconds
3. Restart Visual Studio
4. Navigate to app ? Should be logged in ?

---

## Environment Variables to Check

In **Properties ? launchSettings.json**:

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:5201",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:7201;http://localhost:5201",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

Make sure `ASPNETCORE_ENVIRONMENT` is set to `Development` for development testing.

---

## Production Deployment Checklist

When deploying to production:

- ? Set environment to `Production`
- ? Use HTTPS with valid certificate
- ? Cookie.SecurePolicy = `Always`
- ? Test Remember Me thoroughly
- ? Clear all test cookies before production launch
- ? Monitor authentication logs
- ? Test across different devices

---

## Still Not Working?

Try these steps in order:

1. **Clean browser cache**
   - `Ctrl+Shift+Delete` ? Clear all cookies and cache

2. **Use fresh browser profile**
   - Incognito mode or new Chrome profile

3. **Check database**
   - Verify user record exists
   - Verify EmailVerified = true

4. **Restart Visual Studio**
   - Close and reopen Visual Studio

5. **Rebuild solution**
   - Clean ? Rebuild ? Run

6. **Check connection string**
   - Verify database is accessible
   - Verify user data is saved

7. **Review Program.cs**
   - Verify cookie configuration is correct
   - Check environment detection

If still not working, provide:
- Browser type and version
- Error messages from console
- Cookies shown in DevTools
- Any exception traces

---

**Last Updated:** January 20, 2025  
**Status:** Complete Development Testing Guide  
**For:** ASP.NET Core 10 Razor Pages with Identity
