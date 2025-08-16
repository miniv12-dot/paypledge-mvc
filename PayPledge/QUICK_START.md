# PayPledge Quick Start Guide

Get your PayPledge application up and running in minutes!

## 🚀 Quick Setup (Automated)

### Option 1: PowerShell Script (Recommended for Windows)

1. **Open PowerShell as Administrator**

   ```powershell
   # Right-click PowerShell and select "Run as Administrator"
   ```

2. **Navigate to PayPledge directory**

   ```powershell
   cd "C:\Users\bheng\OneDrive\Desktop\MVC\PayPledge"
   ```

3. **Run the setup script**

   ```powershell
   .\Scripts\setup-couchbase.ps1
   ```

4. **Initialize the database**

   - Open http://localhost:8091 in your browser
   - Login with: `Administrator` / `password`
   - Go to Query tab
   - Copy and paste the contents of `Scripts/database-init.sql`
   - Execute the queries

5. **Start your application**
   ```powershell
   dotnet run
   ```

## 🛠️ Manual Setup

If the automated script doesn't work, follow the detailed guide in [DATABASE_SETUP.md](./DATABASE_SETUP.md)

## ✅ Verify Setup

### 1. Check Couchbase is Running

- Open http://localhost:8091
- You should see the Couchbase login screen

### 2. Check Database Connection

```powershell
dotnet run
```

Look for successful startup messages without database connection errors.

### 3. Test the Application

- Navigate to https://localhost:5001 (or the port shown in console)
- You should see the PayPledge homepage
- Try registering a new user account

## 🔧 Troubleshooting

### Common Issues:

**"Port 8091 is already in use"**

```powershell
# Check what's using the port
netstat -ano | findstr :8091
# Kill the process if needed
taskkill /PID <process_id> /F
```

**"Couchbase service won't start"**

```powershell
# Restart the service
net stop CouchbaseServer
net start CouchbaseServer
```

**"Connection refused" in application**

- Verify Couchbase is running: http://localhost:8091
- Check credentials in `appsettings.json` match your Couchbase setup
- Ensure the `paypledge` bucket exists

**"Bucket not found"**

- Login to Couchbase console
- Go to Buckets tab
- Create bucket named `paypledge` with 1024MB quota

## 📊 Default Configuration

Your `appsettings.json` should contain:

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

## 🎯 What's Included

After setup, you'll have:

- ✅ Couchbase Server running locally
- ✅ PayPledge bucket with proper indexes
- ✅ Sample data for testing
- ✅ Fully configured ASP.NET Core application
- ✅ User authentication system
- ✅ Transaction management
- ✅ Escrow and payment processing
- ✅ AI verification services

## 🚦 Application Features

Once running, you can:

1. **Register** as a Buyer or Seller
2. **Login** to your dashboard
3. **Create transactions** with custom terms
4. **Process payments** through escrow
5. **Submit proof** of delivery/completion
6. **AI verification** of submitted proofs

## 📱 Access Points

- **Web Application**: https://localhost:5001
- **Couchbase Console**: http://localhost:8091
- **API Documentation**: https://localhost:5001/swagger (when running)

## 🔐 Default Credentials

**Couchbase Admin:**

- Username: `Administrator`
- Password: `password`

**Sample App Users** (created by init script):

- Buyer: `buyer@paypledge.demo` / `password123`
- Seller: `seller@paypledge.demo` / `password123`

## 📚 Next Steps

1. **Explore the Application**: Register and test the full workflow
2. **Review the Code**: Check out the Services and Controllers
3. **Customize**: Modify the UI, add features, or integrate real payment gateways
4. **Deploy**: Consider deployment options for production use

## 🆘 Need Help?

- Check [DATABASE_SETUP.md](./DATABASE_SETUP.md) for detailed setup instructions
- Review [TODO.md](./TODO.md) for development status
- Check [HACKATHON_SUMMARY.md](./HACKATHON_SUMMARY.md) for project overview

---

**Happy coding!** 🎉

Your PayPledge application should now be ready for development and testing.
