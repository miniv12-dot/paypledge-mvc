# PayPledge - Secure Escrow Payment Platform

PayPledge is a revolutionary escrow payment platform that uses AI verification to ensure secure transactions between buyers and sellers. Built with ASP.NET Core and Couchbase, it provides a trustworthy environment for digital transactions with custom terms and automated verification.

## 🚀 Quick Start

### Prerequisites

- Windows 11
- .NET 9.0 SDK
- Administrative privileges

### 1. Database Setup (Choose One)

#### Option A: Automated Setup (Recommended)

```powershell
# Run as Administrator
.\Scripts\setup-couchbase.ps1
```

#### Option B: Manual Setup

Follow the detailed guide in [DATABASE_SETUP.md](./DATABASE_SETUP.md)

### 2. Initialize Database

```powershell
# After Couchbase is running, execute the SQL script
# Copy contents of Scripts/database-init.sql into Couchbase Query console
```

### 3. Run Application

```powershell
dotnet run
```

### 4. Verify Setup

```powershell
# Check database health
.\Scripts\check-database.ps1
```

## 📋 Features

### Core Functionality

- ✅ **User Authentication** - Secure registration and login for Buyers and Sellers
- ✅ **Transaction Management** - Create transactions with custom terms and conditions
- ✅ **Escrow Services** - Secure fund holding until conditions are met
- ✅ **Payment Processing** - Mock payment gateway integration (ready for real gateways)
- ✅ **AI Verification** - Automated proof verification using AI services
- ✅ **Real-time Dashboard** - Track transaction status and history

### Technical Features

- ✅ **NoSQL Database** - Couchbase for scalable document storage
- ✅ **JWT Authentication** - Secure token-based authentication
- ✅ **Repository Pattern** - Clean architecture with dependency injection
- ✅ **RESTful APIs** - Full API support with Swagger documentation
- ✅ **Responsive UI** - Bootstrap-based modern interface
- ✅ **Real-time Updates** - SignalR ready for live notifications

## 🏗️ Architecture

### Technology Stack

- **Backend**: ASP.NET Core 9.0 MVC
- **Database**: Couchbase NoSQL
- **Authentication**: JWT Bearer Tokens
- **Frontend**: Razor Pages with Bootstrap 5
- **API Documentation**: Swagger/OpenAPI
- **Real-time**: SignalR (configured)

### Project Structure

```
PayPledge/
├── Controllers/          # MVC Controllers
├── Models/              # Data Models
├── Services/            # Business Logic Services
├── Views/               # Razor Views
├── Scripts/             # Database Setup Scripts
├── wwwroot/             # Static Assets
└── Configuration Files
```

## 🔧 Configuration

### Database Configuration (`appsettings.json`)

```json
{
  "Couchbase": {
    "ConnectionString": "couchbase://localhost",
    "Username": "Administrator",
    "Password": "password",
    "BucketName": "paypledge"
  }
}
```

### JWT Configuration

```json
{
  "JWT": {
    "Key": "PayPledgeSecretKeyForJWTTokenGeneration2024!",
    "Issuer": "PayPledge",
    "Audience": "PayPledgeUsers",
    "ExpiryInHours": 24
  }
}
```

## 📊 Data Models

### Core Entities

- **User** - Buyer/Seller accounts with authentication
- **Transaction** - Main transaction records with custom terms
- **EscrowAccount** - Secure fund holding accounts
- **PaymentTransaction** - Payment processing records
- **ProofSubmission** - Evidence submissions for verification

### Document Structure

All documents follow a consistent structure with:

- Unique ID and document type
- Timestamps (createdAt, updatedAt)
- Status tracking
- Relationship references

## 🔐 Security Features

### Authentication & Authorization

- BCrypt password hashing
- JWT token-based authentication
- Role-based access control (Buyer/Seller)
- Secure session management

### Data Security

- Input validation and sanitization
- SQL injection prevention through parameterized queries
- XSS protection in views
- HTTPS enforcement

## 🧪 Testing

### Sample Data

The initialization script creates sample users:

- **Buyer**: `buyer@paypledge.demo` / `password123`
- **Seller**: `seller@paypledge.demo` / `password123`

### Health Checks

Run the health check script to verify setup:

```powershell
.\Scripts\check-database.ps1
```

## 🚀 Deployment

### Development

```powershell
dotnet run
```

Access at: https://localhost:5001

### Production Considerations

- Update JWT secrets
- Configure production database
- Enable HTTPS certificates
- Set up monitoring and logging
- Configure backup strategies

## 📚 Documentation

- [DATABASE_SETUP.md](./DATABASE_SETUP.md) - Detailed database setup guide
- [QUICK_START.md](./QUICK_START.md) - Quick setup instructions
- [TODO.md](./TODO.md) - Development progress and tasks
- [HACKATHON_SUMMARY.md](./HACKATHON_SUMMARY.md) - Project overview

## 🛠️ Development

### Adding New Features

1. Create models in `Models/`
2. Implement services in `Services/`
3. Add controllers in `Controllers/`
4. Create views in `Views/`
5. Update database indexes if needed

### Database Operations

- All CRUD operations use the generic repository pattern
- Queries use Couchbase N1QL syntax
- Indexes are optimized for common query patterns

## 🤝 Contributing

### Development Workflow

1. Fork the repository
2. Create feature branch
3. Implement changes
4. Test thoroughly
5. Submit pull request

### Code Standards

- Follow C# naming conventions
- Use dependency injection
- Implement proper error handling
- Add XML documentation for public APIs

## 📞 Support

### Troubleshooting

1. Check [DATABASE_SETUP.md](./DATABASE_SETUP.md) for setup issues
2. Run health check script: `.\Scripts\check-database.ps1`
3. Verify Couchbase is running: http://localhost:8091
4. Check application logs for errors

### Common Issues

- **Port conflicts**: Ensure ports 8091 (Couchbase) and 5001 (app) are available
- **Service issues**: Restart Couchbase service if connection fails
- **Authentication**: Verify credentials match between app and database

## 📄 License

This project is developed for educational and demonstration purposes.

---

**PayPledge** - Revolutionizing secure digital transactions with AI-powered verification! 🎉
