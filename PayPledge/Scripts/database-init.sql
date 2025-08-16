-- PayPledge Database Initialization Script
-- Run these queries in Couchbase Query Workbench after creating the 'paypledge' bucket

-- =====================================================
-- PRIMARY INDEX (Required for basic operations)
-- =====================================================
CREATE PRIMARY INDEX ON `paypledge`;

-- =====================================================
-- SECONDARY INDEXES (For optimized queries)
-- =====================================================

-- Index for user queries by type and email
CREATE INDEX idx_users ON `paypledge`(type, email, role, isActive) WHERE type = "user";

-- Index for transaction queries
CREATE INDEX idx_transactions ON `paypledge`(type, buyerId, sellerId, status, createdAt) WHERE type = "transaction";

-- Index for escrow account queries
CREATE INDEX idx_escrow ON `paypledge`(type, transactionId, status, balance) WHERE type = "escrowaccount";

-- Index for payment transaction queries
CREATE INDEX idx_payments ON `paypledge`(type, escrowAccountId, status, amount, createdAt) WHERE type = "paymenttransaction";

-- Index for proof submission queries
CREATE INDEX idx_proofs ON `paypledge`(type, transactionId, status, submittedAt) WHERE type = "proofsubmission";

-- Index for general queries by creation date
CREATE INDEX idx_created_at ON `paypledge`(createdAt, type);

-- Index for status-based queries across all document types
CREATE INDEX idx_status ON `paypledge`(status, type);

-- =====================================================
-- SAMPLE DATA (Optional - for testing)
-- =====================================================

-- Sample Buyer User
INSERT INTO `paypledge` (KEY, VALUE) VALUES (
  "user::buyer-demo-001",
  {
    "id": "buyer-demo-001",
    "type": "user",
    "email": "buyer@paypledge.demo",
    "firstName": "Alice",
    "lastName": "Johnson",
    "role": "Buyer",
    "passwordHash": "$2a$11$N9qo8uLOickgx2ZMRZoMye.IjZGqGpqOW8k6tNw.gJ7hQRqHqS.Pu",
    "isActive": true,
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
);

-- Sample Seller User
INSERT INTO `paypledge` (KEY, VALUE) VALUES (
  "user::seller-demo-001",
  {
    "id": "seller-demo-001",
    "type": "user",
    "email": "seller@paypledge.demo",
    "firstName": "Bob",
    "lastName": "Smith",
    "role": "Seller",
    "passwordHash": "$2a$11$N9qo8uLOickgx2ZMRZoMye.IjZGqGpqOW8k6tNw.gJ7hQRqHqS.Pu",
    "isActive": true,
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
);

-- Sample Transaction
INSERT INTO `paypledge` (KEY, VALUE) VALUES (
  "transaction::demo-tx-001",
  {
    "id": "demo-tx-001",
    "type": "transaction",
    "buyerId": "buyer-demo-001",
    "sellerId": "seller-demo-001",
    "amount": 1500.00,
    "description": "Custom Web Development Project",
    "customTerms": "Deliver responsive website within 14 days with 3 rounds of revisions",
    "status": "Created",
    "createdAt": "2024-01-15T10:00:00Z",
    "updatedAt": "2024-01-15T10:00:00Z"
  }
);

-- Sample Escrow Account
INSERT INTO `paypledge` (KEY, VALUE) VALUES (
  "escrowaccount::demo-escrow-001",
  {
    "id": "demo-escrow-001",
    "type": "escrowaccount",
    "transactionId": "demo-tx-001",
    "balance": 1500.00,
    "status": "Active",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
);

-- Sample Payment Transaction (Deposit)
INSERT INTO `paypledge` (KEY, VALUE) VALUES (
  "paymenttransaction::demo-payment-001",
  {
    "id": "demo-payment-001",
    "type": "paymenttransaction",
    "escrowAccountId": "demo-escrow-001",
    "amount": 1500.00,
    "transactionType": "Deposit",
    "status": "Completed",
    "paymentMethod": "CreditCard",
    "gatewayTransactionId": "mock_txn_12345",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-15T10:30:00Z"
  }
);

-- Sample Proof Submission
INSERT INTO `paypledge` (KEY, VALUE) VALUES (
  "proofsubmission::demo-proof-001",
  {
    "id": "demo-proof-001",
    "type": "proofsubmission",
    "transactionId": "demo-tx-001",
    "submittedBy": "seller-demo-001",
    "proofType": "DeliveryConfirmation",
    "description": "Website delivered and deployed to staging server",
    "attachments": [
      {
        "fileName": "website-screenshot.png",
        "fileUrl": "/uploads/demo-proof-001/website-screenshot.png",
        "fileSize": 245760
      },
      {
        "fileName": "deployment-log.txt",
        "fileUrl": "/uploads/demo-proof-001/deployment-log.txt",
        "fileSize": 1024
      }
    ],
    "status": "Submitted",
    "submittedAt": "2024-01-20T14:30:00Z",
    "verificationResult": null,
    "verifiedAt": null
  }
);

-- =====================================================
-- VERIFICATION QUERIES (Run these to test your setup)
-- =====================================================

-- Check if indexes were created successfully
SELECT name, state FROM system:indexes WHERE keyspace_id = 'paypledge';

-- Count documents by type
SELECT type, COUNT(*) as count 
FROM `paypledge` 
WHERE type IS NOT NULL 
GROUP BY type 
ORDER BY type;

-- Verify sample users
SELECT id, email, role, firstName, lastName 
FROM `paypledge` 
WHERE type = 'user';

-- Verify sample transaction with related data
SELECT t.id as transactionId, 
       t.description, 
       t.amount, 
       t.status as transactionStatus,
       e.id as escrowId, 
       e.balance as escrowBalance,
       e.status as escrowStatus
FROM `paypledge` t
LEFT JOIN `paypledge` e ON t.id = e.transactionId AND e.type = 'escrowaccount'
WHERE t.type = 'transaction';

-- =====================================================
-- CLEANUP QUERIES (Use if you need to reset data)
-- =====================================================

-- Delete all sample data (uncomment to use)
-- DELETE FROM `paypledge` WHERE META().id LIKE 'user::%-demo-%';
-- DELETE FROM `paypledge` WHERE META().id LIKE 'transaction::demo-%';
-- DELETE FROM `paypledge` WHERE META().id LIKE 'escrowaccount::demo-%';
-- DELETE FROM `paypledge` WHERE META().id LIKE 'paymenttransaction::demo-%';
-- DELETE FROM `paypledge` WHERE META().id LIKE 'proofsubmission::demo-%';

-- Drop all indexes (uncomment to use)
-- DROP INDEX `paypledge`.`idx_users`;
-- DROP INDEX `paypledge`.`idx_transactions`;
-- DROP INDEX `paypledge`.`idx_escrow`;
-- DROP INDEX `paypledge`.`idx_payments`;
-- DROP INDEX `paypledge`.`idx_proofs`;
-- DROP INDEX `paypledge`.`idx_created_at`;
-- DROP INDEX `paypledge`.`idx_status`;
-- DROP PRIMARY INDEX ON `paypledge`;
