# InkVault Authentication - Quick Start Guide

## ?? Get Started in 5 Steps

### Step 1: Set Up Database
```powershell
# In Package Manager Console (make sure InkVault is default project)
Add-Migration InitialCreate
Update-Database
```
? Database is now ready!

### Step 2: Configure Email Settings
Edit `appsettings.Development.json`:

**For Gmail (Recommended for Testing):**
1. Go to https://myaccount.google.com/apppasswords
2. Select Mail and Windows app
3. Copy the 16-character password
4. Update the file:
```json
{
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": "587",
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "xxxxx xxxx xxxx xxxx"
  }
}
```

**For Other Providers (Outlook, SendGrid, etc.)**
- Outlook: smtp.outlook.com:587
- SendGrid: smtp.sendgrid.net:587 (username: apikey)

? Email configuration is ready!

### Step 3: Run the Application
- Press **F5** or click **Run** in Visual Studio
- Application starts at https://localhost:5001

? Application is running!

### Step 4: Test Registration Flow
1. Go to **"Create a new user account"** link
2. Fill in registration form
3. Submit registration
4. **Check your email for OTP** (check spam folder!)
5. Enter the 6-digit OTP
6. Click **Verify OTP**
7. You'll be redirected to login page
8. Now you can login with your email and password

? Registration and OTP verification works!

### Step 5: Test Password Reset
1. Go to **"Forgot Password?"** link
2. Enter your registered email
3. **Check your email for OTP**
4. Enter the OTP
5. Enter new password (min 8 characters)
6. Confirm password
7. You're redirected to login page
8. Login with your new password

? Complete! All features working!

## ?? Key Features Implemented

| Feature | Status | Location |
|---------|--------|----------|
| Welcome/Landing Page | ? | `/Account/Welcome` |
| User Registration | ? | `/Account/Register` |
| Email OTP Verification | ? | `/Account/VerifyOTP` |
| Email Verification Check | ? | Login validation |
| Forgot Password | ? | `/Account/ForgotPassword` |
| Password Reset | ? | `/Account/ResetPassword` |
| User Dashboard | ? | `/Home/Index` |
| Logout | ? | Any authenticated page |

## ?? Security Features

? Cryptographically secure OTP generation
? 6-digit numeric OTP (1M combinations)
? 10-minute OTP expiration
? Password hashing using ASP.NET Identity
? CSRF protection on all forms
? Email verification requirement
? Secure session management

## ?? Page Navigation Map

```
UNAUTHENTICATED USER
?? Welcome (Login/Register entry point)
?? Register ? VerifyOTP ? Welcome (auto-redirect to login)
?? ForgotPassword ? VerifyOTP ? ResetPassword ? Welcome

AUTHENTICATED USER
?? Home (Dashboard)
   ?? Logout ? Welcome
```

## ?? Configuration Files

### appsettings.Development.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=UserAuthDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": "587",
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  }
}
```

## ?? Email Configuration Quick Reference

### Gmail (Easiest for Testing)
- SMTP Server: `smtp.gmail.com`
- Port: `587` (TLS)
- Username: Your Gmail address
- Password: [App Password - 16 chars]
- [Generate App Password](https://myaccount.google.com/apppasswords)

### Microsoft Outlook
- SMTP Server: `smtp.outlook.com`
- Port: `587` (TLS)
- Username: Your Outlook email
- Password: Your Outlook password

### SendGrid
- SMTP Server: `smtp.sendgrid.net`
- Port: `587` (TLS)
- Username: `apikey`
- Password: [SendGrid API Key]

## ?? Testing Scenarios

### Successful Registration
1. New email address
2. All fields filled correctly
3. Password 8+ characters
4. Matching passwords
5. **Result**: Email received, OTP verification required

### Failed Registration
1. Existing email ? ? "Email is already registered"
2. Password < 8 chars ? ? "Password must be at least 8 characters"
3. Passwords don't match ? ? "Passwords do not match"

### Successful Login
1. Registered user
2. Email verified
3. Correct password
4. **Result**: Redirect to Home dashboard

### Failed Login
1. Unverified email ? ? Redirected to verify OTP
2. Wrong password ? ? "Invalid email or password"
3. Non-existent user ? ? "Invalid email or password"

### OTP Expiration
1. Wait 10+ minutes
2. Try to verify OTP
3. **Result**: ? "OTP has expired"

## ?? Common Issues & Solutions

### "Email not sent"
- ? Check Gmail App Password (not regular password)
- ? Check spam folder for test emails
- ? Verify appsettings.json SMTP settings
- ? Check that "Less secure apps" is allowed (Gmail)

### "OTP verification fails"
- ? Copy exact OTP from email (no spaces)
- ? Check OTP hasn't expired (10 min timeout)
- ? Verify you're using correct email in query param

### "Can't login after registration"
- ? Check EmailVerified = true in database
- ? Check user exists in AspNetUsers table
- ? Verify password is correctly hashed

### "Database not found"
- ? Run migrations in Package Manager Console
- ? Check connection string in appsettings.json
- ? Verify InkVault is default project

## ?? Detailed Documentation

For more detailed information, refer to:
- **SETUP_GUIDE.md** - Complete setup and configuration
- **IMPLEMENTATION_SUMMARY.md** - Technical implementation details
- **DATABASE_SETUP.md** - Database migration commands

## ?? Customization

### Change OTP Length
In `AccountController.cs`, find the OTP generation:
```csharp
var otp = _otpService.GenerateOTP(6); // Change 6 to desired length
```

### Change OTP Expiration
In `AccountController.cs`:
```csharp
user.OTPExpiration = DateTime.UtcNow.AddMinutes(10); // Change 10 to desired minutes
```

### Customize Email Template
In `EmailService.cs`, modify the HTML body:
```csharp
var mailMessage = new MailMessage
{
    // ... other properties
    Body = $@"
        <html>
        <body>
            <!-- Customize this HTML -->
            <h2>Your InkVault OTP</h2>
            <p>Code: <strong>{otp}</strong></p>
        </body>
        </html>",
    IsBodyHtml = true
};
```

## ? Pre-Deployment Checklist

- [ ] Database migrations completed
- [ ] SMTP settings configured (not hardcoded)
- [ ] Tested registration with real email
- [ ] Tested password reset flow
- [ ] Tested login and logout
- [ ] Verified email verification requirement
- [ ] Tested OTP expiration
- [ ] Customized email template (optional)
- [ ] Reviewed all error messages
- [ ] Set HTTPS certificate for production
- [ ] Reviewed security settings

## ?? Ready to Deploy?

Great! Your authentication system is complete and tested. The application includes:
- ? Secure registration with email OTP verification
- ? Email verification requirement before login
- ? OTP-based password reset
- ? User dashboard after login
- ? Complete session management
- ? Security best practices

For production deployment, ensure:
1. Use environment-specific appsettings files
2. Enable HTTPS
3. Use strong SMTP credentials
4. Set up proper error logging
5. Configure backup email provider

Enjoy building InkVault! ??
