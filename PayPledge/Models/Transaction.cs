using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PayPledge.Models
{
    public class Transaction
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "transaction";

        [Required]
        [JsonProperty("buyerId")]
        public string BuyerId { get; set; } = string.Empty;

        [Required]
        [JsonProperty("sellerId")]
        public string SellerId { get; set; } = string.Empty;

        [Required]
        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; } = "USD";

        [JsonProperty("status")]
        public TransactionStatus Status { get; set; } = TransactionStatus.Created;

        [JsonProperty("terms")]
        public TransactionTerms Terms { get; set; } = new TransactionTerms();

        [JsonProperty("escrowAccountId")]
        public string? EscrowAccountId { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("expectedDeliveryDate")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [JsonProperty("completedAt")]
        public DateTime? CompletedAt { get; set; }

        [JsonProperty("proofSubmissions")]
        public List<string> ProofSubmissionIds { get; set; } = new List<string>();

        [JsonProperty("paymentTransactionId")]
        public string? PaymentTransactionId { get; set; }

        [JsonProperty("disputeReason")]
        public string? DisputeReason { get; set; }

        [JsonProperty("refundAmount")]
        public decimal? RefundAmount { get; set; }
    }

    public enum TransactionStatus
    {
        Created,
        AwaitingPayment,
        PaymentReceived,
        InProgress,
        AwaitingProof,
        UnderReview,
        Completed,
        Disputed,
        Cancelled,
        Refunded
    }

    public class TransactionTerms
    {
        [JsonProperty("deliveryRequirements")]
        public List<string> DeliveryRequirements { get; set; } = new List<string>();

        [JsonProperty("qualityStandards")]
        public List<string> QualityStandards { get; set; } = new List<string>();

        [JsonProperty("verificationRequirements")]
        public List<VerificationRequirement> VerificationRequirements { get; set; } = new List<VerificationRequirement>();

        [JsonProperty("timeoutHours")]
        public int TimeoutHours { get; set; } = 72;

        [JsonProperty("allowPartialRefund")]
        public bool AllowPartialRefund { get; set; } = false;

        [JsonProperty("requiresSignature")]
        public bool RequiresSignature { get; set; } = false;

        [JsonProperty("customTerms")]
        public string? CustomTerms { get; set; }
    }

    public class VerificationRequirement
    {
        [JsonProperty("type")]
        public VerificationType Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("isRequired")]
        public bool IsRequired { get; set; } = true;

        [JsonProperty("aiPrompt")]
        public string? AIPrompt { get; set; }
    }

    public enum VerificationType
    {
        Photo,
        Video,
        Document,
        Receipt,
        Signature,
        Location,
        Timestamp
    }
}
