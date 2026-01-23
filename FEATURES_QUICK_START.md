# Getting Started with New Features

## Quick Start Guide

### Step 1: Update the Database
Before running the application, apply the new database migration:

**Using Package Manager Console:**
```powershell
Update-Database
```

**Using .NET CLI:**
```bash
dotnet ef database update --project InkVault
```

### Step 2: Run the Application
```bash
dotnet run
```

### Step 3: Test the Features

#### Testing Explore
1. Log in to your account
2. Click "Explore" in the navigation bar
3. You should see a list of public journals
4. Try searching for journals
5. Try different sort options
6. Click "View" on any journal to see the full content

#### Testing Friends
1. Create another test account (or use an existing one)
2. Click "Friends" in the navigation bar
3. Click the search box and search for another user
4. Click "Add Friend" to send a friend request
5. Switch to the other account and accept the request
6. Verify the friends count updates in the dashboard
7. Try creating a journal with "Friends Only" privacy
8. Verify the other friend can see it in Explore

#### Testing Theme
1. Click "Settings" dropdown in the top-right
2. Click "Light Theme"
3. Verify all text is readable (especially dropdowns and form fields)
4. Click "Light Theme" again to switch back to dark theme
5. Test on all pages (Explore, Friends, Profile)

#### Testing Dashboard
1. Go to home page
2. Check that "Friends" count is accurate
3. Click on "Explore" and "Make Friends" cards
4. Verify they navigate to correct pages

## Feature Walkthrough

### Exploring Journals
- **Public Journals**: Visible to all authenticated users
- **Friends-Only Journals**: Only visible if you're confirmed friends
- **Private Journals**: Never visible to others (only in your library)

### Friend System
1. **Send Request**: Click "Add Friend" on user's search result
2. **Manage Requests**: View pending requests on Friends page
3. **Accept/Decline**: Buttons on pending requests
4. **Remove Friend**: Click "Remove Friend" to unfriend someone

### Journal Privacy Settings
When creating a journal, choose from:
- **Private (Only Me)**: Only you can see it
- **Friends Only**: Your friends can see it
- **Public**: Everyone can see it

## Troubleshooting

### Database Issues
If you get database errors:
```bash
# Reset the database (WARNING: Deletes all data)
dotnet ef database drop --project InkVault
dotnet ef database update --project InkVault
```

### Theme Not Working
Clear browser cache or open in incognito mode to reset theme preferences.

### Friends Not Appearing
Make sure you've accepted friend requests from both accounts.

## Known Limitations
- Friend requests are one-way until accepted
- View count increments each time a journal is viewed
- Friendships are bidirectional (both users see each other as friends)

## Next Steps (Optional Features)
- Add friend blocking functionality
- Add journal favoriting/bookmarking
- Add comments on journals
- Add notifications for friend requests
- Add user profiles with bio
- Add journal recommendations based on interests
