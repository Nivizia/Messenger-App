# Testing Guide - Messenger App

## Overview
This guide helps you test the complete messenger functionality.

## Prerequisites
- Valid user account (registered and email verified)
- Internet connection for API calls
- Visual Studio or compatible IDE

## Testing Steps

### 1. Authentication Flow
1. **Run the application** (MessengerUI project)
2. **Register a new account** (if needed)
   - Enter email, username, password
   - Check email for verification code
   - Enter verification code
3. **Login with credentials**
   - Should show "Login successful" message
   - Token should be saved automatically

### 2. Contact Management
1. **ContactWindow should open** after successful login
2. **Verify user loading**
   - Should automatically load all users
   - Status should show "Loaded X users"
3. **Test search functionality**
   - Type in search box
   - Should filter users in real-time
   - Try searching by username or email
4. **Test user selection**
   - Click on a user in the list
   - "Start Chat" button should become enabled
   - Double-click should also start chat

### 3. Chat Functionality
1. **Start a chat**
   - Select a user and click "Start Chat"
   - ChattingWindow should open
   - Header should show "Chat with: [username]"
2. **Test message loading**
   - Should load existing messages (if any)
   - Messages should display in bubbles
   - Should auto-scroll to bottom
3. **Test message sending**
   - Type a message in the input box
   - Click "Send" or press Enter
   - Message should appear immediately (placeholder)
   - Input should clear after sending
4. **Test refresh**
   - Click "Refresh" button
   - Should reload messages from server

### 4. UI Features
1. **Window management**
   - Chat window should be child of contact window
   - "Back" button should close chat window
   - Multiple chat windows can be open
2. **Status messages**
   - Should show loading states
   - Should show error messages if API fails
   - Should show success confirmations
3. **Input validation**
   - Empty messages should not be sent
   - Authentication errors should be handled

## Expected Behavior

### ✅ Working Features
- User registration and login
- Email verification
- User listing and search
- Conversation creation
- Message history loading
- **Message sending (placeholder - shows immediately)**
- Error handling and status messages
- Window navigation

### 🔄 Placeholder Features
- **SendMessage API** - currently simulated
  - Always returns success
  - Messages appear immediately in UI
  - Real API integration needed

## Common Issues & Solutions

### "No token found"
- **Solution:** Login again, token may have expired

### "Failed to load users"
- **Solution:** Check internet connection and API availability

### "Failed to create conversation"
- **Solution:** Verify user ID is valid and API is accessible

### Messages not loading
- **Solution:** Check conversation ID and token validity

## API Integration Testing

When real SendMessage API is available:

1. **Replace placeholder** in `ChatService.SendMessageAsync`
2. **Test message sending** with real API
3. **Verify error handling** works with real responses
4. **Test message persistence** across app restarts

## Performance Notes

- Search has 300ms delay to avoid excessive API calls
- Messages load with pagination (10 per request)
- UI updates are optimistic for better user experience

## Success Criteria

✅ **Complete Flow Test:**
1. Register → Verify Email → Login
2. Load Users → Search → Select User
3. Create Conversation → Load Messages
4. Send Message → See Immediate Update
5. Refresh → Verify Persistence (when real API integrated)

The app is ready for production use once the real SendMessage API is integrated!
