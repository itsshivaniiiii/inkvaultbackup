#!/bin/bash
# Email Configuration Verification Script for InkVault

echo "=========================================="
echo "InkVault Email Configuration Checker"
echo "=========================================="
echo ""

# Check if appsettings.json exists
if [ ! -f "appsettings.json" ]; then
    echo "? appsettings.json not found!"
    exit 1
fi

echo "? appsettings.json found"
echo ""

# Extract SMTP settings using grep (basic parsing)
echo "?? Current SMTP Configuration:"
echo "---"

if grep -q '"SmtpSettings"' appsettings.json; then
    echo "? SmtpSettings block found"
    
    SERVER=$(grep -A 3 '"SmtpSettings"' appsettings.json | grep '"Server"' | sed 's/.*"Server": *"\([^"]*\)".*/\1/')
    PORT=$(grep -A 3 '"SmtpSettings"' appsettings.json | grep '"Port"' | sed 's/.*"Port": *"\([^"]*\)".*/\1/')
    EMAIL=$(grep -A 3 '"SmtpSettings"' appsettings.json | grep '"SenderEmail"' | sed 's/.*"SenderEmail": *"\([^"]*\)".*/\1/')
    PASSWORD=$(grep -A 3 '"SmtpSettings"' appsettings.json | grep '"SenderPassword"' | sed 's/.*"SenderPassword": *"\([^"]*\)".*/\1/')
    
    echo "Server: $SERVER"
    echo "Port: $PORT"
    echo "Email: $EMAIL"
    
    if [ -z "$PASSWORD" ]; then
        echo "? Password: NOT SET"
    else
        if [[ $PASSWORD == *" "* ]]; then
            echo "? Password: SET but HAS SPACES (INVALID!) - Length: ${#PASSWORD}"
        else
            echo "? Password: SET, ${#PASSWORD} characters, no spaces"
        fi
    fi
else
    echo "? SmtpSettings block not found"
fi

echo ""
echo "=========================================="
echo "Verification Checklist:"
echo "=========================================="
echo ""
echo "Before running the application, ensure:"
echo "  [ ] Gmail app password generated (16 chars, no spaces)"
echo "  [ ] appsettings.json updated with app password"
echo "  [ ] appsettings.Development.json updated"
echo "  [ ] Password does NOT have spaces"
echo "  [ ] Server is smtp.gmail.com"
echo "  [ ] Port is 587"
echo ""
echo "After starting application:"
echo "  [ ] Look for [EMAIL] messages in console"
echo "  [ ] Try registering to trigger OTP email"
echo "  [ ] Check email inbox for OTP"
echo ""
echo "=========================================="
