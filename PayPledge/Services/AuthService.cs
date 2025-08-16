using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using PayPledge.Models;

namespace PayPledge.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(string email, string password, string firstName, string lastName, UserRole role);
        Task<(User? user, string? token)> LoginAsync(string email, string password);
        Task<User?> GetUserByIdAsync(string userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UpdateUserAsync(User user);
        string GenerateJwtToken(User user);
        bool ValidatePassword(string password, string hashedPassword);
    }

    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly string _bucketName;

        public AuthService(
            IRepository<User> userRepository,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
            _bucketName = _configuration["Couchbase:BucketName"] ?? "pay_pledge";
        }

        public async Task<User?> RegisterAsync(string email, string password, string firstName, string lastName, UserRole role)
        {
            try
            {
                // Check if user already exists
                var existingUser = await GetUserByEmailAsync(email);
                if (existingUser != null)
                {
                    return null; // User already exists
                }

                // Hash password
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                // Create new user
                var user = new User
                {
                    Email = email.ToLowerInvariant(),
                    PasswordHash = hashedPassword,
                    FirstName = firstName,
                    LastName = lastName,
                    Role = role,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var userId = await _userRepository.CreateAsync(user);
                user.Id = userId;

                _logger.LogInformation("User registered successfully: {Email}", email);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {Email}", email);
                throw;
            }
        }

        public async Task<(User? user, string? token)> LoginAsync(string email, string password)
        {
            try
            {
                var user = await GetUserByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    return (null, null);
                }

                if (!ValidatePassword(password, user.PasswordHash))
                {
                    return (null, null);
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user.Id, user);

                var token = GenerateJwtToken(user);
                _logger.LogInformation("User logged in successfully: {Email}", email);

                return (user, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", email);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _userRepository.GetByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                var query = $"SELECT * FROM `{_bucketName}` WHERE type = 'user' AND email = $email";
                var parameters = new { email = email.ToLowerInvariant() };

                var users = await _userRepository.FindAsync(query, parameters);
                return users.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                user.UpdatedAt = DateTime.UtcNow;
                return await _userRepository.UpdateAsync(user.Id, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }

        public string GenerateJwtToken(User user)
        {
            var key = _configuration["JWT:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var issuer = _configuration["JWT:Issuer"] ?? "PayPledge";
            var audience = _configuration["JWT:Audience"] ?? "PayPledgeUsers";
            var expiryHours = int.Parse(_configuration["JWT:ExpiryInHours"] ?? "24");

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("firstName", user.FirstName),
                new("lastName", user.LastName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expiryHours),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool ValidatePassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
