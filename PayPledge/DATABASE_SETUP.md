# PayPledge Database Setup Guide

This guide will walk you through setting up Couchbase database for the PayPledge application.

## Prerequisites

- Windows 11 (as per your system)
- .NET 9.0 SDK
- Administrative privileges for installation

## Step 1: Download and Install Couchbase Server

### Option A: Couchbase Community Edition (Free)

1. Go to https://www.couchbase.com/downloads
2. Download **Couchbase Server Community Edition** for Windows
3. Choose the latest stable version (7.x or 8.x)

### Option B: Couchbase Enterprise Edition (Trial)

1. Go to https://www.couchbase.com/downloads
2. Download **Couchbase Server Enterprise Edition** for Windows
3. You get a 30-day free trial

## Step 2: Install Couchbase Server

1. Run the downloaded installer as Administrator
2. Follow the installation wizard:
   - Accept the license agreement
   - Choose installation directory (default: `C:\Program Files\Couchbase\Server`)
   - Select components (keep all selected)
   - Choose to start Couchbase Server after installation

## Step 3: Initial Couchbase Configuration

### 3.1 Access Couchbase Web Console

1. Open your web browser
2. Navigate to: `http://localhost:8091`
3. You should see the Couchbase Server setup screen

### 3.2 Setup New Cluster

1. Click **"Setup New Cluster"**
2. Configure the following:
   - **Cluster Name**: `PayPledge-Cluster` (or any name you prefer)
   - **Admin Username**: `Administrator`
   - **Admin Password**: `password` (matches your appsettings.json)
   - **Confirm Password**: `password`

### 3.3 Configure Services

1. Keep all services selected:
   - ✅ Data Service
   - ✅ Index Service
   - ✅ Query Service
   - ✅ Search Service (optional)
   - ✅ Analytics Service (optional)
   - ✅ Eventing Service (optional)

### 3.4 Configure Memory

1. **Data Service RAM Quota**: 2048 MB (minimum, adjust based on your system)
2. **Index Service RAM Quota**: 512 MB
3. **Search Service RAM Quota**: 512 MB (if enabled)
4. Click **"Save & Finish"**

## Step 4: Create PayPledge Bucket

### 4.1 Navigate to Buckets

1. In the Couchbase Web Console, click **"Buckets"** in the left sidebar
2. Click **"ADD BUCKET"** button

### 4.2 Configure Bucket

1. **Name**: `paypledge` (must match appsettings.json)
2. **Memory Quota**: 1024 MB (adjust based on your needs)
3. **Bucket Type**: Couchbase
4. **Replicas**: 0 (for development)
5. **Flush**: Enable (for development/testing)
6. Click **"Add Bucket"**

## Step 5: Create Indexes for Query Performance

### 5.1 Access Query Workbench

1. Click **"Query"** in the left sidebar
2. This opens the Query Workbench

### 5.2 Create Primary Index

```sql
CREATE PRIMARY INDEX ON `paypledge`;
```

### 5.3 Create Type-Based Indexes

```sql
-- Index for user queries
CREATE INDEX idx_users ON `paypledge`(type, email) WHERE type = "user";

-- Index for transaction queries
CREATE INDEX idx_transactions ON `paypledge`(type, buyerId, sellerId, status) WHERE type = "transaction";

-- Index for escrow account queries
CREATE INDEX idx_escrow ON `paypledge`(type, transactionId, status) WHERE type = "escrowaccount";

-- Index for payment transaction queries
CREATE INDEX idx_payments ON `paypledge`(type, escrowAccountId, status) WHERE type = "paymenttransaction";

-- Index for proof submission queries
CREATE INDEX idx_proofs ON `paypledge`(type, transactionId, status) WHERE type = "proofsubmission";
```

## Step 6: Verify Configuration

### 6.1 Check Your appsettings.json

Ensure your configuration matches:

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

### 6.2 Test Connection

1. Open terminal in your PayPledge project directory
2. Run the application:

```bash
dotnet run
```

3. Check for any connection errors in the console

## Step 7: Optional - Create Sample Data

You can create sample data through the Query Workbench:

### Sample User Document

```sql
INSERT INTO `paypledge` (KEY, VALUE) VALUES (
  "user::sample-buyer-001",
  {
    "id": "sample-buyer-001",
    "type": "user",
    "email": "buyer@example.com",
    "firstName": "John",
    "lastName": "Buyer",
    "role": "Buyer",
    "passwordHash": "$2a$11$example.hash.here",
    "isActive": true,
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
);
```

### Sample Transaction Document

```sql
INSERT INTO `paypledge` (KEY, VALUE) VALUES (
  "transaction::sample-tx-001",
  {
    "id": "sample-tx-001",
    "type": "transaction",
    "buyerId": "sample-buyer-001",
    "sellerId": "sample-seller-001",
    "amount": 1000.00,
    "description": "Sample transaction for testing",
    "customTerms": "Deliver within 7 days",
    "status": "Created",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
);
```

## Troubleshooting

### Common Issues:

1. **Port 8091 already in use**

   - Check if another application is using port 8091
   - Stop other services or change Couchbase port during installation

2. **Connection refused**

   - Ensure Couchbase Server service is running
   - Check Windows Services for "Couchbase Server"
   - Restart the service if needed

3. **Authentication failed**

   - Verify username/password in appsettings.json match Couchbase setup
   - Check if the user has proper permissions

4. **Bucket not found**
   - Ensure bucket name "paypledge" exists in Couchbase
   - Check spelling and case sensitivity

### Service Management:

- **Start Service**: `net start CouchbaseServer`
- **Stop Service**: `net stop CouchbaseServer`
- **Restart Service**: `net stop CouchbaseServer && net start CouchbaseServer`

## Security Considerations for Production

1. **Change Default Credentials**: Use strong passwords
2. **Enable TLS/SSL**: Configure secure connections
3. **Network Security**: Restrict access to trusted networks
4. **Regular Backups**: Set up automated backup procedures
5. **User Management**: Create specific users with limited permissions

## Next Steps

After completing this setup:

1. Run your PayPledge application: `dotnet run`
2. Navigate to `https://localhost:5001` (or the port shown in console)
3. Test user registration and login functionality
4. Verify data is being stored in Couchbase through the web console

## Support Resources

- [Couchbase Documentation](https://docs.couchbase.com/)
- [Couchbase .NET SDK Guide](https://docs.couchbase.com/dotnet-sdk/current/hello-world/start-using-sdk.html)
- [PayPledge Project README](./README.md)

---

**Database Setup Complete!** 🎉

Your PayPledge application should now be able to connect to Couchbase and perform all database operations.
