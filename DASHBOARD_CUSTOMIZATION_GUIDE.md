# ?? InkVault Dashboard - Logo & Customization Guide

## ? What's Been Done

Your new InkVault dashboard now includes:

? **Modern Dark Theme** - Professional dark mode with gradient accents
? **Interactive Animations** - Smooth fade-in, slide, and hover effects
? **Feature Cards** - 6 main action cards with hover animations
? **Stats Dashboard** - Display journals, friends, readers, engagement
? **Getting Started Guide** - 6-step onboarding guide
? **Modals** - Create journal and browse topics pop-ups
? **Responsive Design** - Works on mobile, tablet, and desktop
? **Gradient Navigation** - Beautiful navbar with icon integration

---

## ??? How to Add Your Logo

### Step 1: Prepare Your Logo File

1. **Get your logo image** (PNG, JPG, or SVG preferred)
   - Recommended size: 200x200px or higher
   - Format: PNG (with transparency) is ideal
   - Keep it square or with equal padding

2. **Create the folder** `wwwroot/images/` (if it doesn't exist):
   - In Solution Explorer, right-click your project
   - Select "Open Folder in File Explorer"
   - Navigate to `wwwroot`
   - Create a new folder called `images`

### Step 2: Add Logo File

1. **Copy your logo** into `wwwroot/images/`
   - Name it: `logo.png` (or your preferred name)

2. **Example path after adding**:
   ```
   wwwroot/
   ??? images/
       ??? logo.png
   ```

### Step 3: Enable Logo Display

In the **_Layout.cshtml** file, change this line:

**Current** (logo is hidden):
```html
<img src="~/images/logo.png" alt="InkVault Logo" style="display:none;" />
```

**Change to** (logo is visible):
```html
<img src="~/images/logo.png" alt="InkVault Logo" />
```

### Step 4: Adjust Logo Size (Optional)

If your logo needs size adjustments, modify the navbar-brand styling in `_Layout.cshtml`:

```html
<a class="navbar-brand" asp-area="" asp-controller="Home" asp-action="Index">
    <img src="~/images/logo.png" alt="InkVault Logo" style="height: 40px; width: auto; margin-right: 10px;" />
    <i class="bi bi-book"></i> InkVault
</a>
```

**Adjust these values**:
- `height: 40px` - Change to your desired height
- `width: auto` - Keeps aspect ratio
- `margin-right: 10px` - Space between logo and text

### Step 5: Test & Verify

1. Press **F5** to run the application
2. Check the navbar - you should see your logo with "InkVault" text
3. The logo should appear on all pages

---

## ?? Color Customization

All colors are defined in CSS variables in `_Layout.cshtml`. To customize:

```css
:root {
    --primary-color: #667eea;      /* Purple */
    --secondary-color: #764ba2;    /* Dark Purple */
    --accent-color: #f093fb;       /* Pink */
    --dark-bg: #0f1419;            /* Almost Black */
    --card-bg: #1a1f2e;            /* Dark Blue-Gray */
    --text-primary: #e0e0e0;       /* Light Gray */
    --text-secondary: #b0b0b0;     /* Medium Gray */
}
```

**Example: Change Primary Color**
```css
--primary-color: #ff6b6b;  /* Change to red */
```

---

## ? Dashboard Features Explained

### 1. **Welcome Section**
- Personalized greeting with gradient text
- Has animated background blur effect
- Shows user's name

### 2. **Stats Dashboard**
Four statistics boxes showing:
- Journals Published
- Friends Count
- Readers Count
- Likes & Comments

### 3. **Quick Actions** (6 Cards)
1. **Create Journal** - Open modal to write new journal
2. **Explore Writings** - Browse all public journals
3. **Make Friends** - Find and connect with other writers
4. **Browse by Topic** - Filter journals by category
5. **My Library** - View all personal journals
6. **Friends Feed** - See updates from friends

### 4. **Getting Started**
Step-by-step guide for new users to get started

### 5. **Modals**
- **Create Journal Modal**: Form to write and publish a new journal
  - Title input
  - Content textarea
  - Topic selector
  - Privacy toggle (Private/Public)

- **Topics Modal**: Grid of journal categories
  - Science, Technology, Travel, Philosophy, Romance, Food, Health, Education, Business, Art

---

## ?? Navigation Structure

The navbar now includes:

**Authenticated Users:**
```
Home | My Journals | Explore | Friends
                                    User Settings | Logout
```

**Unauthenticated Users:**
```
Home
              Login | Register
```

---

## ?? Next Steps to Complete Features

### To Enable "Create Journal":
1. Create `JournalController.cs`
2. Create `Journal` model with Title, Content, Topic, IsPublic, etc.
3. Implement journal creation logic
4. Connect modal form to backend

### To Enable "My Journals":
1. Display user's journals in a new page
2. Add edit/delete functionality
3. Show draft and published status

### To Enable "Explore Writings":
1. Create browse page
2. Implement filtering by topic, author, date
3. Add search functionality

### To Enable "Make Friends":
1. Create friends system
2. Add friend request/accept logic
3. Display friend profiles

### To Enable "Friends Feed":
1. Create feed page
2. Show latest journals from friends
4. Add comments/likes functionality

---

## ?? Responsive Design

The dashboard is fully responsive:

- **Desktop** (1200px+): 3-column layout for cards
- **Tablet** (768px-1199px): 2-column layout
- **Mobile** (<768px): 1-column stacked layout

All features work seamlessly on mobile devices.

---

## ?? Animations Included

1. **Fade In Up** - Cards appear with fade-in effect
2. **Slide In Left** - Welcome section slides in from left
3. **Hover Effects** - Cards lift up and glow on hover
4. **Icon Scale** - Icons grow and rotate on card hover
5. **Button Translation** - Buttons move on hover

---

## ?? Dark Mode

The dashboard uses a dark theme by default:
- **Background**: Dark blue-black (#0f1419)
- **Cards**: Slightly lighter (#1a1f2e)
- **Text**: Light gray (#e0e0e0)
- **Accents**: Purple and pink gradients

To switch to light mode, modify the CSS variables in `_Layout.cshtml`.

---

## ?? Making Further Changes

### Edit Dashboard Styling:
1. Open `Views/Home/Index.cshtml`
2. Find the `<style>` section
3. Modify CSS as needed

### Edit Navigation Bar:
1. Open `Views/Shared/_Layout.cshtml`
2. Modify the `<nav>` section
3. Change colors, text, or links

### Edit Feature Cards:
1. In `Views/Home/Index.cshtml`
2. Modify the "Quick Actions" section
3. Add or remove cards as needed

---

## ?? Production Checklist

Before deploying:

- [ ] Add your logo to `wwwroot/images/`
- [ ] Uncomment the logo line in navbar
- [ ] Test logo displays correctly
- [ ] Test all links work properly
- [ ] Test modals open and close
- [ ] Test responsive design on mobile
- [ ] Customize colors to match your brand
- [ ] Update "InkVault" text if using different brand name
- [ ] Test all animations work smoothly
- [ ] Verify all navigation links point to correct pages

---

## ?? Quick Reference

### Logo Path:
```
~/images/logo.png
```

### Main Files Modified:
- `Views/Shared/_Layout.cshtml` - Navigation & styling
- `Views/Home/Index.cshtml` - Dashboard content

### CSS Variables Location:
```
Views/Shared/_Layout.cshtml, lines 19-28
```

### Color Codes Used:
- Primary Purple: `#667eea`
- Secondary Purple: `#764ba2`
- Accent Pink: `#f093fb`
- Dark Background: `#0f1419`

---

## ? You're Ready!

Your InkVault dashboard is now:
1. ? Modern and professional
2. ? Interactive with animations
3. ? Ready for logo addition
4. ? Fully responsive
5. ? Ready for feature implementation

**Next:** Add your logo and customize colors, then start implementing the journal features!

---

**Questions?** The structure is all in place. Just follow the steps to add your logo and you're good to go! ??
