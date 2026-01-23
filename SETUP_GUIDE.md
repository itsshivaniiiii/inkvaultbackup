# InkVault Authentication Flow - Setup Guide

## Overview
This guide covers the complete authentication flow with email OTP verification for registration and password reset.

## Database Setup

The `ApplicationUser` model already includes the necessary fields for OTP management:
- `OTP`: Stores the One-Time Password
- `OTPExpiration`: Stores the expiration time of the OTP
- `EmailVerified`: Boolean flag indicating if the email is verified

**If you haven't already, run these commands in the Package Manager Console:**

```powershell
Add-Migration AddOTPFields
Update-Database
```

## Email Configuration

Update `appsettings.Development.json` with your SMTP settings:

```json
{
  "SmtpSettings": {
    "Server": "smtp.gmail.com",
    "Port": "587",
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  }
}
```

### For Gmail:
1. Enable 2-factor authentication on your Google account
2. Generate an App Password: https://myaccount.google.com/apppasswords
3. Use the generated 16-character password as `SenderPassword`

## Authentication Flow

### 1. Welcome Page (Entry Point)
- **Route**: `/Account/Welcome`
- **Action**: GET/POST
- **Features**:
  - Login form (Email + Password)
  - Link to registration
  - Forgot password link

### 2. Registration Flow
**Step 1: Register**
- **Route**: `/Account/Register`
- **Action**: POST
- **Process**:
  1. User submits registration form
  2. System creates user (with `EmailVerified = false`)
  3. System generates 6-digit OTP
  4. OTP is stored in database with 10-minute expiration
  5. OTP is sent via email

**Step 2: Verify OTP**
- **Route**: `/Account/VerifyOTP`
- **Action**: GET/POST
- **Parameters**: `email`, `purpose` (Registration/PasswordReset)
- **Validation**:
  - OTP must match stored value
  - OTP must not be expired
- **On Success**:
  - Sets `EmailVerified = true`
  - Clears OTP and expiration time
  - Redirects to Welcome page

### 3. Password Reset Flow
**Step 1: Forgot Password**
- **Route**: `/Account/ForgotPassword`
- **Action**: GET/POST
- **Process**:
  1. User enters email
  2. System validates email exists
  3. System generates 6-digit OTP
  4. OTP is stored with 10-minute expiration
  5. OTP is sent via email

**Step 2: Verify OTP (Same endpoint)**
- **Route**: `/Account/VerifyOTP`
- **Purpose**: PasswordReset
- **On Success**: Redirects to Reset Password page

**Step 3: Reset Password**
- **Route**: `/Account/ResetPassword`
- **Action**: GET/POST
- **Requirements**:
  - Password minimum 8 characters
  - Passwords must match
- **On Success**:
  - Password is updated
  - OTP is cleared
  - Redirects to Welcome page

### 4. Login & Post-Login Navigation
**Login**
- **Route**: `/Account/Login`
- **Validation**: 
  - Username and password must match
  - Email must be verified (`EmailVerified = true`)
- **On Success**: Redirects to `/Home/Index`

**Home Page**
- **Route**: `/Home/Index`
- **Access**: Requires authentication
- **Purpose**: User dashboard/home page

## Security Features

? **OTP Management**:
- Secure random number generation using `RNGCryptoServiceProvider`
- 6-digit numeric OTP
- 10-minute expiration time
- Automatically cleared after successful verification

? **Password Security**:
- Minimum 8 characters required
- Passwords hashed using ASP.NET Identity
- Token-based reset mechanism

? **Session Security**:
- Session timeout: 30 minutes idle
- HttpOnly and Secure cookies
- CSRF protection on all forms

? **Email Verification**:
- Users cannot login until email is verified
- Separate tracking via `EmailVerified` flag

## Testing the Flow

### Test Registration with OTP:
1. Go to `/Account/Welcome`
2. Click "Create a New Account"
3. Fill in registration form
4. Check email for OTP
5. Verify OTP on the verification page
6. You should be redirected to Welcome and can now login

### Test Password Reset:
1. Go to `/Account/Welcome`
2. Click "Forgot Password?"
3. Enter your registered email
4. Check email for OTP
5. Verify OTP
6. Enter new password (min 8 characters)
7. Confirm password
8. Should be able to login with new password

## Troubleshooting

### OTP not being sent?
- Check `appsettings.Development.json` SMTP settings
- Verify Gmail App Password is correctly set
- Check application logs for email errors

### OTP validation fails?
- Ensure OTP hasn't expired (10 minutes)
- Verify you're entering the correct OTP
- Check database to ensure OTP was stored

### Cannot login after email verification?
- Verify `EmailVerified` flag is true in database
- Check user's email address matches login attempt
- Clear browser cache/cookies and try again

## Files Modified/Created

### Core Services:
- `Services/OTPService.cs` - Secure OTP generation and validation
- `Services/EmailService.cs` - Email sending via SMTP

### Controllers:
- `Controllers/AccountController.cs` - All authentication flows
- `Controllers/HomeController.cs` - Post-login home page

### Views:
- `Views/Account/Welcome.cshtml` - Landing page with login
- `Views/Account/Register.cshtml` - Registration form
- `Views/Account/Login.cshtml` - Login page
- `Views/Account/VerifyOTP.cshtml` - OTP verification
- `Views/Account/ForgotPassword.cshtml` - Forgot password step 1
- `Views/Account/ResetPassword.cshtml` - Reset password step 2
- `Views/Home/Index.cshtml` - User dashboard

### ViewModels:
- `ViewModels/WelcomeViewModel.cs`
- `ViewModels/LoginViewModel.cs`
- `ViewModels/RegisterViewModel.cs`
- `ViewModels/VerifyOTPViewModel.cs`
- `ViewModels/ForgotPasswordViewModel.cs`
- `ViewModels/ResetPasswordViewModel.cs`

### Models:
- `Models/ApplicationUser.cs` - User model with OTP fields

## API Endpoints Summary

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/Account/Welcome` | GET/POST | Landing page & login |
| `/Account/Register` | GET/POST | User registration |
| `/Account/VerifyOTP` | GET/POST | OTP verification |
| `/Account/ForgotPassword` | GET/POST | Forgot password request |
| `/Account/ResetPassword` | GET/POST | Reset password |
| `/Account/Login` | GET/POST | Alternative login page |
| `/Account/Logout` | POST | Logout user |
| `/Home/Index` | GET | User dashboard |

## Next Steps

1. Configure SMTP settings in `appsettings.Development.json`
2. Run database migrations
3. Test the complete flow
4. Customize email templates as needed
5. Add additional dashboard features to `/Home/Index`
