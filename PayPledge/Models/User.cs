using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PayPledge.Models
{
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("type")]
        public string Type { get; set; } = "user";

        [Required]
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [JsonProperty("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [JsonProperty("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [JsonProperty("lastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonProperty("role")]
        public UserRole Role { get; set; }

        [JsonProperty("phoneNumber")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("isEmailVerified")]
        public bool IsEmailVerified { get; set; } = false;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("lastLoginAt")]
        public DateTime? LastLoginAt { get; set; }

        [JsonProperty("profileImageUrl")]
        public string? ProfileImageUrl { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; } = true;

        // Computed property for display
        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";
    }

    public enum UserRole
    {
        Buyer,
        Seller,
        Both
    }
}
