# ?? Friends Feature - Quick Reference Guide

## What's Available Now

### On Friends Page (`/Friends`)

#### 1. **Search for Friends** ??
- Search by name (first/last) or username
- Instant results as you type (via search form)
- Filter results by connection status

#### 2. **Send Friend Requests** ?
- Click "Add Friend" button
- Toast notification confirms: "Friend request sent to [Name]!"
- Button changes to "Withdraw"

#### 3. **Withdraw Requests** ??
- Click "Withdraw" button on pending requests
- Toast notification: "Friend request withdrawn from [Name]"
- Can re-send after withdrawing

#### 4. **View Pending Requests** ??
- Section titled "Pending Requests"
- Shows all users who sent YOU friend requests
- Displays when request was sent

#### 5. **Accept Requests** ?
- Click "Accept" button on pending request
- Toast: "You are now friends with [Name]!"
- User immediately appears in "Your Friends"
- Page auto-refreshes

#### 6. **Decline Requests** ?
- Click "Decline" button to reject request
- Toast: "Friend request from [Name] declined"
- Request removed from pending list
- Can still add them later
- Page auto-refreshes

#### 7. **View All Friends** ??
- Section "Your Friends" shows confirmed friends
- Shows "Friends since [Date]"

#### 8. **Remove Friends** ??
- Click "Remove Friend" button
- Asks for confirmation
- Toast: "[Name] has been removed from your friends"
- User removed from friends list
- Page auto-refreshes

---

## ?? UI Elements

### Buttons and Their Meanings

| Button | Color | Meaning | Action |
|--------|-------|---------|--------|
| Add Friend | Purple | Not connected yet | Send request |
| Withdraw | Orange | You sent a request | Cancel request |
| Accept | Green | They sent request to you | Accept and become friends |
| Decline | Red | They sent request to you | Reject the request |
| Friends | Blue (Disabled) | Already connected | Remove friend option available |
| Remove Friend | Red | Confirmed friend | Delete friendship |

### Sections on Page

1. **Search Bar** - Find users by name/email
2. **Search Results** (when searching) - Shows matching users
3. **Pending Requests** (if any) - Requests you received
4. **Your Friends** (always visible) - Your confirmed friends

---

## ?? Toast Notifications

### Success (Green) ?
- "Friend request sent to [Name]!"
- "Friend request withdrawn from [Name]"
- "You are now friends with [Name]!"
- "Friend request from [Name] declined"
- "[Name] has been removed from your friends"

### Error (Red) ?
- "Error sending friend request"
- "Error withdrawing request"
- "Error accepting friend request"
- "Error declining request"
- "Error removing friend"

### Location
- **Top-right corner** of screen
- **Non-blocking** - doesn't interfere with content
- **Auto-dismisses** after 4 seconds
- **Smooth animations** slide in and out

---

## ?? Request Status Flow

```
    [Not Connected]
           ?
    (Click "Add Friend")
           ?
    [Pending] ? Waiting for response
    ?         ?
(They Accept)  (Withdraw / They Decline)
    ?              ?
[Friends]    [Not Connected]
    ?
(Click "Remove Friend")
    ?
[Not Connected]
```

---

## ? Quick Actions

### I Want To...

**Find someone to friend**
1. Go to Friends page
2. Type their name in search box
3. Click Search
4. Click "Add Friend"
5. ? Success notification appears

**Cancel a request I sent**
1. Go to Friends page
2. Search for the person (optional)
3. Look for their name with "Withdraw" button
4. Click "Withdraw"
5. ? Request cancelled

**Accept a friend request**
1. Go to Friends page
2. Look at "Pending Requests" section
3. Find the request
4. Click "Accept"
5. ? You're now friends!

**Reject a friend request**
1. Go to Friends page
2. Look at "Pending Requests" section
3. Find the request
4. Click "Decline"
5. ? Request rejected (can add later)

**Remove a friend**
1. Go to Friends page
2. Scroll to "Your Friends" section
3. Find the friend
4. Click "Remove Friend"
5. Click "OK" on confirmation
6. ? Friend removed

**See all my friends**
1. Go to Friends page
2. Scroll to "Your Friends" section
3. See everyone you're connected with

---

## ?? Features at a Glance

### Sent Requests
- ? You can withdraw them
- ? Shows as "Pending" status
- ? Can be re-sent after withdrawal

### Received Requests
- ? Separate "Pending Requests" section
- ? Easy accept/decline buttons
- ? Shows sender's profile picture
- ? Shows when request was sent

### Friendships
- ? Shows "Friends since" date
- ? Can be removed anytime
- ? Confirmation required for removal

### Search
- ? Search by first name, last name, username, email
- ? Shows real-time status
- ? Buttons update after actions

---

## ?? Request Statuses

### From Your Perspective

| Status | Your View | Their View | Actions Available |
|--------|-----------|-----------|-------------------|
| Friends | "Friends" button | "Friends" button | Remove Friend |
| You Sent Request | "Withdraw" button | (pending) | Withdraw |
| They Sent Request | (in Pending section) | (can't see it) | Accept / Decline |
| Not Connected | "Add Friend" button | "Add Friend" button | Send Request |

---

## ?? Tips & Tricks

1. **Search Tips**
   - Search is case-insensitive
   - Search by partial names (e.g., "john" finds "Johnson")
   - You can search by email or username too

2. **Managing Requests**
   - You won't see who you sent requests to in search results (shows "Pending")
   - You can't friend yourself
   - Withdrawing is safe - they won't be notified

3. **Notifications**
   - Notifications stay for 4 seconds
   - All notifications appear in same spot (top-right)
   - Check them even if you tab away - they'll still show

4. **Mobile Tips**
   - All features work on mobile
   - Buttons are large and easy to tap
   - Notifications fully visible on small screens

---

## ?? Things to Remember

- ? Friend requests are **not** notifications (you see them on the Friends page)
- ? Withdrawn requests are **permanent** (can re-send anytime though)
- ? Declined requests are **permanent** (can send new request anytime)
- ? Removing a friend is **permanent** (can become friends again)
- ? All actions require **confirmation** when needed

---

## ?? Examples

### Example 1: Making a New Friend
```
1. Go to Friends page
2. Type "Alice" in search
3. Click Search
4. See "Alice Johnson" in results
5. Click "Add Friend" button
6. Green toast: "Friend request sent to Alice Johnson!"
7. Button now says "Withdraw"
8. Wait for Alice to accept
```

### Example 2: Accepting a Request
```
1. Go to Friends page
2. See "Pending Requests" section (it has 1 request)
3. See "Bob Smith" - "Sent Jan 15, 2025"
4. Click "Accept" button
5. Green toast: "You are now friends with Bob Smith!"
6. Page refreshes
7. Bob now appears in "Your Friends" section
```

### Example 3: Changing Your Mind
```
1. Send request to "Charlie"
2. See button changed to "Withdraw"
3. Change your mind
4. Click "Withdraw"
5. Green toast: "Friend request withdrawn from Charlie"
6. Button back to "Add Friend"
7. Can send new request anytime
```

---

## ?? Need Help?

- **Request didn't send?** Check your internet connection and try again
- **Can't find someone?** Make sure they're registered in the system
- **Button not changing?** Refresh the page if it seems stuck
- **Notification didn't appear?** Check top-right corner of screen

---

**Last Updated**: January 2025
**Status**: ? Fully Functional
