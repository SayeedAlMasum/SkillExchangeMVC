# Bkash Payment Troubleshooting Guide

## Current Issue
The bKash payment is not proceeding to the next step when the user clicks "Pay ?50 with bKash".

## Fixed Issues
? **Form Binding**: Fixed radio button binding for payment method selection
? **Anti-forgery Token**: Added proper anti-forgery token handling
? **Error Display**: Added error display in both views
? **Validation**: Enhanced form validation and error messages
? **Logging**: Added comprehensive logging throughout the payment flow

## Next Steps to Debug

### 1. **Check Bkash Configuration**
Update `appsettings.json` with actual Bkash sandbox credentials:

```json
{
  "Bkash": {
    "BaseUrl": "https://tokenized.sandbox.bka.sh/v1.2.0-beta",
    "Username": "your_actual_sandbox_username",
    "Password": "your_actual_sandbox_password",
    "AppKey": "your_actual_sandbox_app_key",
    "AppSecret": "your_actual_sandbox_app_secret"
  }
}
```

### 2. **Test the Flow Step by Step**

**Step 1**: Test form submission
- Select bKash payment method
- Fill in mobile number (e.g., 01712345678)
- Click "Pay ?50 with bKash"
- Check browser console for any JavaScript errors
- Check server logs for validation errors

**Step 2**: Check if reaching ProcessBkashPayment
- The form should redirect to `/Payment/ProcessBkashPayment`
- You should see the bKash payment confirmation page
- Check logs for: `"ProcessBkashPayment called with CourseId..."`

**Step 3**: Test Bkash API call
- Click "Pay with bKash" button on the confirmation page
- Check browser console for fetch request errors
- Check server logs for: `"Bkash CreatePayment called with CourseId..."`

### 3. **Common Issues and Solutions**

**Issue**: Form not submitting
- **Solution**: Check browser console for JavaScript errors
- **Check**: Radio button selection is working properly

**Issue**: Validation errors
- **Solution**: Check server logs for validation error messages
- **Check**: All required fields are filled

**Issue**: Bkash API errors
- **Solution**: Verify Bkash credentials in appsettings.json
- **Check**: Network connectivity to Bkash sandbox

**Issue**: Session timeout
- **Solution**: Check session configuration in Program.cs
- **Check**: Ensure session middleware is properly configured

### 4. **Debug Commands**

Open browser Developer Tools (F12) and check:
1. **Console Tab**: For JavaScript errors
2. **Network Tab**: For failed HTTP requests
3. **Application Tab**: For session storage issues

### 5. **Log Messages to Look For**

**Successful Flow**:
```
Payment form submitted. Payment Method: Bkash, CourseId: X, Amount: X
Bkash payment selected. Mobile: 01XXXXXXXXX
Redirecting to ProcessBkashPayment for CourseId: X
ProcessBkashPayment called with CourseId: X, Amount: X
Bkash CreatePayment called with CourseId: X, Amount: X
Successfully obtained Bkash token
Bkash payment created successfully with ID: XXXXXX
```

**Error Indicators**:
```
Model state is invalid. Errors: [error messages]
Failed to get Bkash token
Failed to create Bkash payment
Session expired or missing data
```

### 6. **Testing with Mock Data**

If you don't have actual Bkash credentials yet, you can temporarily modify the `BkashService.GetTokenAsync()` method to return mock data for testing the UI flow:

```csharp
public async Task<BkashTokenResponse?> GetTokenAsync()
{
    // Mock response for testing
    return new BkashTokenResponse
    {
        id_token = "mock_token_for_testing",
        token_type = "Bearer",
        expires_in = 3600,
        statusCode = "0000",
        statusMessage = "Successful"
    };
}
```

### 7. **Immediate Action Items**

1. **Configure Bkash Credentials**: Get sandbox credentials from bKash
2. **Test Browser Console**: Check for JavaScript errors
3. **Check Server Logs**: Look for validation or API errors
4. **Verify Form Submission**: Ensure the form reaches the controller
5. **Test Network Connectivity**: Ensure the server can reach bKash API

## Quick Test Checklist

- [ ] Browser console shows no JavaScript errors
- [ ] Form submits successfully (reaches PaymentController)
- [ ] bKash option is properly selected
- [ ] Mobile number is filled in
- [ ] Server logs show the payment flow steps
- [ ] Bkash credentials are configured
- [ ] Session is working properly
- [ ] Network can reach Bkash API

## Support

If the issue persists after checking these items, please provide:
1. Browser console errors (if any)
2. Server log output
3. Network tab showing failed requests
4. Steps that reproduce the issue

The logging has been added to help identify exactly where the process is stopping.