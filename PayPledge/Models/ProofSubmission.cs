using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PayPledge.Models
{
    public class ProofSubmission
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "proof";

        [Required]
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [JsonProperty("submittedBy")]
        public string SubmittedBy { get; set; } = string.Empty;

        [JsonProperty("verificationType")]
        public VerificationType VerificationType { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("fileUrls")]
        public List<string> FileUrls { get; set; } = new List<string>();

        [JsonProperty("metadata")]
        public ProofMetadata Metadata { get; set; } = new ProofMetadata();

        [JsonProperty("status")]
        public ProofStatus Status { get; set; } = ProofStatus.Submitted;

        [JsonProperty("aiVerificationResult")]
        public AIVerificationResult? AIVerificationResult { get; set; }

        [JsonProperty("submittedAt")]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("verifiedAt")]
        public DateTime? VerifiedAt { get; set; }

        [JsonProperty("rejectionReason")]
        public string? RejectionReason { get; set; }

        [JsonProperty("requiresHumanReview")]
        public bool RequiresHumanReview { get; set; } = false;
    }

    public enum ProofStatus
    {
        Submitted,
        Processing,
        Verified,
        Rejected,
        RequiresReview,
        Resubmitted
    }

    public class ProofMetadata
    {
        [JsonProperty("fileSize")]
        public long FileSize { get; set; }

        [JsonProperty("fileType")]
        public string FileType { get; set; } = string.Empty;

        [JsonProperty("capturedAt")]
        public DateTime? CapturedAt { get; set; }

        [JsonProperty("location")]
        public LocationData? Location { get; set; }

        [JsonProperty("deviceInfo")]
        public string? DeviceInfo { get; set; }

        [JsonProperty("checksum")]
        public string? Checksum { get; set; }
    }

    public class LocationData
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("accuracy")]
        public double? Accuracy { get; set; }

        [JsonProperty("address")]
        public string? Address { get; set; }
    }

    public class AIVerificationResult
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("overallScore")]
        public double OverallScore { get; set; }

        [JsonProperty("isAuthentic")]
        public bool IsAuthentic { get; set; }

        [JsonProperty("confidenceLevel")]
        public ConfidenceLevel ConfidenceLevel { get; set; }

        [JsonProperty("verificationChecks")]
        public List<VerificationCheck> VerificationChecks { get; set; } = new List<VerificationCheck>();

        [JsonProperty("processedAt")]
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("processingTimeMs")]
        public long ProcessingTimeMs { get; set; }

        [JsonProperty("aiModel")]
        public string AIModel { get; set; } = "PayPledge-Vision-v1.0";

        [JsonProperty("flags")]
        public List<string> Flags { get; set; } = new List<string>();

        [JsonProperty("summary")]
        public string Summary { get; set; } = string.Empty;
    }

    public class VerificationCheck
    {
        [JsonProperty("checkType")]
        public string CheckType { get; set; } = string.Empty;

        [JsonProperty("passed")]
        public bool Passed { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("details")]
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    public enum ConfidenceLevel
    {
        Low,
        Medium,
        High,
        VeryHigh
    }
}
