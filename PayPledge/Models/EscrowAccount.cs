using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PayPledge.Models
{
    public class EscrowAccount
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "escrow";

        [Required]
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; } = string.Empty;

        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; } = "USD";

        [JsonProperty("status")]
        public EscrowStatus Status { get; set; } = EscrowStatus.Created;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("fundsReceivedAt")]
        public DateTime? FundsReceivedAt { get; set; }

        [JsonProperty("fundsReleasedAt")]
        public DateTime? FundsReleasedAt { get; set; }

        [JsonProperty("paymentMethodId")]
        public string? PaymentMethodId { get; set; }

        [JsonProperty("releaseConditions")]
        public List<ReleaseCondition> ReleaseConditions { get; set; } = new List<ReleaseCondition>();

        [JsonProperty("transactions")]
        public List<EscrowTransaction> Transactions { get; set; } = new List<EscrowTransaction>();

        [JsonProperty("fees")]
        public EscrowFees Fees { get; set; } = new EscrowFees();
    }

    public enum EscrowStatus
    {
        Created,
        AwaitingFunds,
        FundsHeld,
        ReadyForRelease,
        Released,
        Disputed,
        Refunded,
        Cancelled
    }

    public class ReleaseCondition
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("isMet")]
        public bool IsMet { get; set; } = false;

        [JsonProperty("verifiedAt")]
        public DateTime? VerifiedAt { get; set; }

        [JsonProperty("verificationMethod")]
        public string? VerificationMethod { get; set; }

        [JsonProperty("aiConfidence")]
        public double? AIConfidence { get; set; }
    }

    public class EscrowTransaction
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public EscrowTransactionType Type { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonProperty("paymentReference")]
        public string? PaymentReference { get; set; }
    }

    public enum EscrowTransactionType
    {
        Deposit,
        Release,
        Refund,
        Fee,
        Dispute,
        PartialRelease,
        PartialRefund
    }

    public class EscrowFees
    {
        [JsonProperty("serviceFeePercentage")]
        public decimal ServiceFeePercentage { get; set; } = 2.5m;

        [JsonProperty("serviceFeeAmount")]
        public decimal ServiceFeeAmount { get; set; }

        [JsonProperty("paymentProcessingFee")]
        public decimal PaymentProcessingFee { get; set; }

        [JsonProperty("totalFees")]
        public decimal TotalFees { get; set; }

        [JsonProperty("feesPaidBy")]
        public string FeesPaidBy { get; set; } = "buyer"; // buyer, seller, split
    }
}
