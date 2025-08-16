# PayPledge Backend Setup Guide

## Quick Start

### 1. Run the Backend Application

```bash
cd PayPledge
dotnet run
```

The application will start on:

- **HTTP**: http://localhost:5004
- **HTTPS**: https://localhost:7084 (if configured)

### 2. Access the Application

Open your browser and navigate to: http://localhost:5004

## Database Connection Setup

### Current Issue

The application connects to Couchbase Cloud but the N1QL Query service is not available.

### Option 1: Fix Couchbase Cloud (Recommended)

1. **Login to Couchbase Cloud Console**

   - Go to https://cloud.couchbase.com
   - Login with your credentials

2. **Enable Query Service**

   - Navigate to your cluster: `cb.aiqy7ljtpwu35jrz.cloud.couchbase.com`
   - Go to "Services" tab
   - Ensure "Query" service is enabled
   - If not enabled, add Query service to your cluster

3. **Verify Bucket Configuration**
   - Check that bucket `pay_pledge` exists
   - Verify user `PayPledge1` has proper permissions
   - Ensure user can access Query service

### Option 2: Use Local Couchbase (Alternative)

If you prefer to use a local Couchbase instance:

1. **Install Couchbase Server**

   - Download from: https://www.couchbase.com/downloads
   - Install and run locally

2. **Update Configuration**
   Edit `appsettings.json`:

   ```json
   {
     "Couchbase": {
       "ConnectionString": "couchbase://localhost",
       "Username": "Administrator",
       "Password": "password",
       "BucketName": "pay_pledge"
     }
   }
   ```

3. **Create Bucket**
   - Open Couchbase Web Console: http://localhost:8091
   - Create bucket named `pay_pledge`
   - Enable Query service

### Option 3: Mock Database (For Testing)

For development/testing without database:

1. **Create Mock Service**
   Create `Services/MockAuthService.cs`:

   ```csharp
   public class MockAuthService : IAuthService
   {
       private static readonly List<User> _users = new();

       public async Task<User?> RegisterAsync(string email, string password, string firstName, string lastName, UserRole role)
       {
           var user = new User
           {
               Id = Guid.NewGuid().ToString(),
               Email = email,
               FirstName = firstName,
               LastName = lastName,
               Role = role,
               CreatedAt = DateTime.UtcNow,
               IsActive = true
           };
           _users.Add(user);
           return user;
       }

       // Implement other interface methods...
   }
   ```

2. **Update Program.cs**
   Replace the AuthService registration:
   ```csharp
   // Comment out: builder.Services.AddScoped<IAuthService, AuthService>();
   builder.Services.AddScoped<IAuthService, MockAuthService>();
   ```

## Testing the Setup

### 1. Test Web Interface

- Navigate to http://localhost:5004
- Click "Register"
- Fill out the registration form
- Submit to test database connection

### 2. Test API Endpoints

- Access Swagger UI: http://localhost:5004/swagger
- Test authentication endpoints
- Verify API responses

### 3. Check Logs

Monitor the console output for:

- ✅ `Now listening on: http://localhost:5004`
- ✅ `Application started`
- ❌ Any Couchbase connection errors

## Troubleshooting

### Common Issues

1. **Port Already in Use**

   ```bash
   # Kill existing dotnet processes
   taskkill /f /im dotnet.exe
   ```

2. **Couchbase Connection Timeout**

   - Check internet connection
   - Verify Couchbase Cloud credentials
   - Ensure firewall allows outbound connections

3. **N1QL Service Error**
   - Enable Query service in Couchbase Cloud
   - Check user permissions
   - Verify bucket exists

### Configuration Files

- **Main Config**: `appsettings.json`
- **Development Config**: `appsettings.Development.json`
- **Launch Settings**: `Properties/launchSettings.json`

## Development Commands

```bash
# Build the application
dotnet build

# Run with specific environment
dotnet run --environment Development

# Run tests (if available)
dotnet test

# Restore packages
dotnet restore
```

## Next Steps

1. **Fix Database Connection**: Choose one of the options above
2. **Test Registration**: Try creating a user account
3. **Test Login**: Verify authentication works
4. **Explore Features**: Test dashboard and other functionality

## Support

If you encounter issues:

1. Check the console output for detailed error messages
2. Verify all configuration settings
3. Ensure Couchbase service is properly configured
4. Consider using the mock service for initial testing
