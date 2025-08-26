# API Integration Guide

## Overview
This guide explains how to integrate the real SendMessage API when it becomes available.

## Current Implementation Status

### ✅ Completed Features
- User authentication (login/register/email verification)
- User listing and searching
- Conversation creation
- Message retrieval
- **Placeholder message sending** (ready for real API)

### 🔄 Placeholder Implementation
The `SendMessageAsync` method in `ChatService.cs` is currently a placeholder that:
- Simulates network delay (500ms)
- Always returns `true` (success)
- Includes detailed TODO comments for real implementation

## Integration Steps

### 1. Replace SendMessage Method
**File:** `MessengerApp/ApiFetcher/ChatService.cs`
**Method:** `SendMessageAsync(string token, string conversationId, string content)`

**Current placeholder code (lines ~158-195):**
```csharp
public static async Task<bool> SendMessageAsync(string token, string conversationId, string content)
{
    // PLACEHOLDER IMPLEMENTATION - Replace this entire method body
    await Task.Delay(500); // Remove this
    return true; // Replace with real API logic
}
```

**Replace with real implementation:**
```csharp
public static async Task<bool> SendMessageAsync(string token, string conversationId, string content)
{
    string url = "https://api.mmb.io.vn/py/api/chatbox/send-message"; // Update URL
    
    var payload = new { 
        conversation_id = conversationId, 
        content = content 
    };
    
    var jsonContent = new StringContent(
        JsonSerializer.Serialize(payload), 
        Encoding.UTF8, 
        "application/json"
    );
    
    using var request = new HttpRequestMessage(HttpMethod.Post, url);
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    request.Content = jsonContent;
    
    using var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();
    
    var json = await response.Content.ReadAsStringAsync();
    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(json, options);
    
    return apiResponse?.success ?? false;
}
```

### 2. Update Response Models (if needed)
If the API returns different response format, update the response classes in `ChatService.cs`:
- `ApiResponse`
- `ApiResponsePost` 
- `ApiResponseMessages`

### 3. Test Integration
1. Update the console test in `ConsoleAppTest/Program.cs`
2. Test with real API endpoints
3. Verify error handling works correctly

## UI Features Ready for Real API

### ContactWindow
- ✅ User loading and search
- ✅ Conversation creation
- ✅ Navigation to chat

### ChattingWindow  
- ✅ Message display with proper styling
- ✅ Message input and sending
- ✅ Optimistic UI updates
- ✅ Error handling and status messages
- ✅ Auto-scroll and refresh

## Testing the Current Implementation

1. **Run the application**
2. **Login** with valid credentials
3. **Search and select a user** in ContactWindow
4. **Start a chat** - ChattingWindow opens
5. **Type and send messages** - they appear immediately (placeholder)
6. **Refresh messages** to load real conversation history

## Notes for Your Friend

- The placeholder always returns `success = true`
- All error handling is already implemented
- The UI will work seamlessly once real API is integrated
- No UI changes needed - just replace the service method
- Follow the same pattern as other methods in `ChatService.cs`

## API Endpoint Assumptions

Based on existing patterns, the SendMessage API should:
- **Method:** POST
- **URL:** `https://api.mmb.io.vn/py/api/chatbox/send-message`
- **Headers:** Bearer token authentication
- **Body:** JSON with `conversation_id` and `content`
- **Response:** JSON with `success` boolean field

Adjust the implementation if the actual API differs from these assumptions.
