using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PayPledge.Models
{
    public class PaymentTransaction
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "payment";

        [Required]
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; } = string.Empty;

        [JsonProperty("paymentMethodId")]
        public string PaymentMethodId { get; set; } = string.Empty;

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; } = "USD";

        [JsonProperty("status")]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [JsonProperty("paymentType")]
        public PaymentType PaymentType { get; set; }

        [JsonProperty("gatewayTransactionId")]
        public string? GatewayTransactionId { get; set; }

        [JsonProperty("gatewayResponse")]
        public Dictionary<string, object> GatewayResponse { get; set; } = new Dictionary<string, object>();

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("processedAt")]
        public DateTime? ProcessedAt { get; set; }

        [JsonProperty("failureReason")]
        public string? FailureReason { get; set; }

        [JsonProperty("refundTransactionId")]
        public string? RefundTransactionId { get; set; }

        [JsonProperty("fees")]
        public PaymentFees Fees { get; set; } = new PaymentFees();
    }

    public class PaymentMethod
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "paymentMethod";

        [JsonProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonProperty("methodType")]
        public PaymentMethodType MethodType { get; set; }

        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; } = false;

        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = true;

        [JsonProperty("cardDetails")]
        public CardDetails? CardDetails { get; set; }

        [JsonProperty("bankDetails")]
        public BankDetails? BankDetails { get; set; }

        [JsonProperty("walletDetails")]
        public WalletDetails? WalletDetails { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("lastUsedAt")]
        public DateTime? LastUsedAt { get; set; }
    }

    public class CardDetails
    {
        [JsonProperty("last4")]
        public string Last4 { get; set; } = string.Empty;

        [JsonProperty("brand")]
        public string Brand { get; set; } = string.Empty;

        [JsonProperty("expiryMonth")]
        public int ExpiryMonth { get; set; }

        [JsonProperty("expiryYear")]
        public int ExpiryYear { get; set; }

        [JsonProperty("holderName")]
        public string HolderName { get; set; } = string.Empty;

        [JsonProperty("gatewayTokenId")]
        public string? GatewayTokenId { get; set; }
    }

    public class BankDetails
    {
        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; } = string.Empty;

        [JsonProperty("routingNumber")]
        public string RoutingNumber { get; set; } = string.Empty;

        [JsonProperty("accountType")]
        public string AccountType { get; set; } = string.Empty;

        [JsonProperty("bankName")]
        public string BankName { get; set; } = string.Empty;

        [JsonProperty("accountHolderName")]
        public string AccountHolderName { get; set; } = string.Empty;
    }

    public class WalletDetails
    {
        [JsonProperty("walletType")]
        public string WalletType { get; set; } = string.Empty;

        [JsonProperty("walletId")]
        public string WalletId { get; set; } = string.Empty;

        [JsonProperty("email")]
        public string? Email { get; set; }
    }

    public class PaymentFees
    {
        [JsonProperty("processingFee")]
        public decimal ProcessingFee { get; set; }

        [JsonProperty("platformFee")]
        public decimal PlatformFee { get; set; }

        [JsonProperty("totalFees")]
        public decimal TotalFees { get; set; }

        [JsonProperty("feeBreakdown")]
        public Dictionary<string, decimal> FeeBreakdown { get; set; } = new Dictionary<string, decimal>();
    }

    public enum PaymentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled,
        Refunded,
        PartiallyRefunded,
        Disputed,
        ChargedBack
    }

    public enum PaymentType
    {
        Deposit,
        Release,
        Refund,
        Fee,
        Chargeback
    }

    public enum PaymentMethodType
    {
        CreditCard,
        DebitCard,
        BankTransfer,
        PayPal,
        ApplePay,
        GooglePay,
        Cryptocurrency,
        DigitalWallet
    }
}
