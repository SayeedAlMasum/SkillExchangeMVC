# Bkash Payment Integration

This project has been successfully integrated with Bkash payment system following the MVC pattern.

## Features Implemented

### 1. Bkash Payment Models (`Models/BkashModels/`)
The Bkash models are now organized in separate files within the `BkashModels` folder:

**Token Management:**
- `BkashTokenResponse.cs` - For token management

**Payment Operations:**
- `BkashCreatePaymentRequest.cs` - For payment creation requests
- `BkashCreatePaymentResponse.cs` - For payment creation responses
- `BkashExecutePaymentRequest.cs` - For payment execution requests
- `BkashExecutePaymentResponse.cs` - For payment execution responses
- `BkashQueryPaymentResponse.cs` - For payment status queries

**Refund Operations:**
- `BkashRefundRequest.cs` - For refund requests
- `BkashRefundResponse.cs` - For refund responses

**Helper:**
- `BkashModelsIndex.cs` - Index file for easy reference to all models

### 2. Bkash Service (`Services/BkashService.cs`)
- `IBkashService` interface for dependency injection
- `BkashService` implementation with methods:
  - `GetTokenAsync()` - Get authentication token
  - `CreatePaymentAsync()` - Create payment request
  - `ExecutePaymentAsync()` - Execute payment
  - `QueryPaymentAsync()` - Query payment status
  - `RefundPaymentAsync()` - Process refunds

### 3. Controllers
- **BkashController** - Handles Bkash-specific operations
  - Payment creation and execution
  - Callback handling for payment success/failure
  - Payment queries and refunds
- **PaymentController** - Updated to support multiple payment methods
  - Card payments (existing)
  - Bkash payments (new)

### 4. Models Updated
- **Payment Model** - Extended to support Bkash fields:
  - Payment method tracking
  - Bkash transaction details
  - Amount with proper decimal precision
- **PaymentViewModel** - Updated for multiple payment methods
- **Course Model** - Added Price property for payment calculations

### 5. Views
- **CreatePayment.cshtml** - Updated with payment method selection (Card/Bkash)
- **ProcessBkashPayment.cshtml** - New view for Bkash payment processing

### 6. Configuration
- **appsettings.json** - Added Bkash configuration section
- **Program.cs** - Registered Bkash service and HTTP client

## File Organization

The project follows a clean organization structure:

```
Models/
??? BkashModels/                    # Bkash-specific models
?   ??? BkashTokenResponse.cs       # Token management
?   ??? BkashCreatePaymentRequest.cs
?   ??? BkashCreatePaymentResponse.cs
?   ??? BkashExecutePaymentRequest.cs
?   ??? BkashExecutePaymentResponse.cs
?   ??? BkashQueryPaymentResponse.cs
?   ??? BkashRefundRequest.cs
?   ??? BkashRefundResponse.cs
?   ??? BkashModelsIndex.cs         # Reference index
??? ViewModels/
?   ??? PaymentViewModel.cs         # Updated for multiple payment methods
??? Context/
?   ??? SkillExchangeContext.cs     # Updated with Payment DbSet
??? Payment.cs                      # Extended for Bkash support
??? Course.cs                       # Added Price property

Services/
??? BkashService.cs                 # Bkash API integration service

Controllers/
??? BkashController.cs              # Bkash-specific operations
??? PaymentController.cs            # Multi-method payment handling

Views/Payment/
??? CreatePayment.cshtml            # Multi-method payment form
??? ProcessBkashPayment.cshtml      # Bkash processing page
```

## Configuration Required

Update `appsettings.json` with your actual Bkash credentials:

```json
{
  "Bkash": {
    "BaseUrl": "https://tokenized.sandbox.bka.sh/v1.2.0-beta",
    "Username": "your_actual_bkash_username",
    "Password": "your_actual_bkash_password", 
    "AppKey": "your_actual_bkash_app_key",
    "AppSecret": "your_actual_bkash_app_secret"
  }
}
```

## How It Works

1. **Payment Selection**: Users can choose between Card or Bkash payment methods
2. **Bkash Flow**:
   - User selects Bkash and provides mobile number
   - System creates payment request with Bkash API
   - User is redirected to Bkash payment gateway
   - After payment, user is redirected back to callback URL
   - System executes payment and enrolls user in course

3. **Session Management**: Payment states are maintained in session for security
4. **Database Integration**: All payments are stored with detailed information
5. **Error Handling**: Comprehensive error handling for all API calls

## Security Features

- Token-based authentication with Bkash
- Session-based payment state management
- Secure callback handling
- Payment verification before enrollment
- Proper error handling and user feedback

## Database Migration

The Payment table has been added to support:
- Multiple payment methods
- Bkash transaction tracking
- Amount handling with proper decimal precision
- Comprehensive payment history

To apply migrations manually if needed:
```bash
dotnet ef migrations add AddBkashPaymentSupport
dotnet ef database update
```

## Dependencies Added

- `Newtonsoft.Json` - For JSON serialization/deserialization
- HTTP Client services for API calls
- Session services for state management

The integration follows ASP.NET Core best practices and maintains separation of concerns with proper MVC architecture. The new file organization provides better maintainability and readability.