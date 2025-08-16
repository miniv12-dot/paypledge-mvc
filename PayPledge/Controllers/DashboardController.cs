using Microsoft.AspNetCore.Mvc;
using PayPledge.Models;
using PayPledge.Services;

namespace PayPledge.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IRepository<EscrowAccount> _escrowRepository;
        private readonly IRepository<ProofSubmission> _proofRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IRepository<Transaction> transactionRepository,
            IRepository<EscrowAccount> escrowRepository,
            IRepository<ProofSubmission> proofRepository,
            IAuthService authService,
            ILogger<DashboardController> logger)
        {
            _transactionRepository = transactionRepository;
            _escrowRepository = escrowRepository;
            _proofRepository = proofRepository;
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Get user's transactions
                var buyerQuery = "SELECT * FROM `paypledge` WHERE type = 'transaction' AND buyerId = $userId ORDER BY createdAt DESC LIMIT 10";
                var sellerQuery = "SELECT * FROM `paypledge` WHERE type = 'transaction' AND sellerId = $userId ORDER BY createdAt DESC LIMIT 10";

                var buyerTransactions = await _transactionRepository.FindAsync(buyerQuery, new { userId });
                var sellerTransactions = await _transactionRepository.FindAsync(sellerQuery, new { userId });

                // Get recent proof submissions
                var proofQuery = "SELECT * FROM `paypledge` WHERE type = 'proof' AND submittedBy = $userId ORDER BY submittedAt DESC LIMIT 5";
                var recentProofs = await _proofRepository.FindAsync(proofQuery, new { userId });

                // Calculate statistics
                var totalBuyerTransactions = buyerTransactions.Count();
                var totalSellerTransactions = sellerTransactions.Count();
                var completedTransactions = buyerTransactions.Concat(sellerTransactions)
                    .Count(t => t.Status == TransactionStatus.Completed);

                var totalSpent = buyerTransactions
                    .Where(t => t.Status == TransactionStatus.Completed)
                    .Sum(t => t.Amount);

                var totalEarned = sellerTransactions
                    .Where(t => t.Status == TransactionStatus.Completed)
                    .Sum(t => t.Amount);

                var model = new DashboardViewModel
                {
                    User = user,
                    BuyerTransactions = buyerTransactions.ToList(),
                    SellerTransactions = sellerTransactions.ToList(),
                    RecentProofs = recentProofs.ToList(),
                    Statistics = new DashboardStatistics
                    {
                        TotalBuyerTransactions = totalBuyerTransactions,
                        TotalSellerTransactions = totalSellerTransactions,
                        CompletedTransactions = completedTransactions,
                        TotalSpent = totalSpent,
                        TotalEarned = totalEarned,
                        ActiveTransactions = buyerTransactions.Concat(sellerTransactions)
                            .Count(t => t.Status == TransactionStatus.InProgress ||
                                       t.Status == TransactionStatus.AwaitingProof)
                    }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard for user {UserId}", userId);
                TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
                return View(new DashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Transactions(string filter = "all")
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var transactions = new List<Transaction>();

                switch (filter.ToLower())
                {
                    case "buyer":
                        var buyerQuery = "SELECT * FROM `paypledge` WHERE type = 'transaction' AND buyerId = $userId ORDER BY createdAt DESC";
                        transactions = (await _transactionRepository.FindAsync(buyerQuery, new { userId })).ToList();
                        break;
                    case "seller":
                        var sellerQuery = "SELECT * FROM `paypledge` WHERE type = 'transaction' AND sellerId = $userId ORDER BY createdAt DESC";
                        transactions = (await _transactionRepository.FindAsync(sellerQuery, new { userId })).ToList();
                        break;
                    default:
                        var allQuery = "SELECT * FROM `paypledge` WHERE type = 'transaction' AND (buyerId = $userId OR sellerId = $userId) ORDER BY createdAt DESC";
                        transactions = (await _transactionRepository.FindAsync(allQuery, new { userId })).ToList();
                        break;
                }

                ViewBag.Filter = filter;
                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transactions for user {UserId}", userId);
                TempData["ErrorMessage"] = "Error loading transactions. Please try again.";
                return View(new List<Transaction>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> TransactionDetails(string id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id);
                if (transaction == null)
                {
                    TempData["ErrorMessage"] = "Transaction not found.";
                    return RedirectToAction("Transactions");
                }

                // Check if user is authorized to view this transaction
                if (transaction.BuyerId != userId && transaction.SellerId != userId)
                {
                    TempData["ErrorMessage"] = "You are not authorized to view this transaction.";
                    return RedirectToAction("Transactions");
                }

                // Get related data
                var buyer = await _authService.GetUserByIdAsync(transaction.BuyerId);
                var seller = await _authService.GetUserByIdAsync(transaction.SellerId);

                EscrowAccount? escrowAccount = null;
                if (!string.IsNullOrEmpty(transaction.EscrowAccountId))
                {
                    escrowAccount = await _escrowRepository.GetByIdAsync(transaction.EscrowAccountId);
                }

                var proofs = new List<ProofSubmission>();
                if (transaction.ProofSubmissionIds.Any())
                {
                    foreach (var proofId in transaction.ProofSubmissionIds)
                    {
                        var proof = await _proofRepository.GetByIdAsync(proofId);
                        if (proof != null) proofs.Add(proof);
                    }
                }

                var model = new TransactionDetailsViewModel
                {
                    Transaction = transaction,
                    Buyer = buyer,
                    Seller = seller,
                    EscrowAccount = escrowAccount,
                    ProofSubmissions = proofs,
                    CurrentUserId = userId,
                    IsCurrentUserBuyer = transaction.BuyerId == userId,
                    IsCurrentUserSeller = transaction.SellerId == userId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transaction details for {TransactionId}", id);
                TempData["ErrorMessage"] = "Error loading transaction details. Please try again.";
                return RedirectToAction("Transactions");
            }
        }
    }

    public class DashboardViewModel
    {
        public User User { get; set; } = new User();
        public List<Transaction> BuyerTransactions { get; set; } = new List<Transaction>();
        public List<Transaction> SellerTransactions { get; set; } = new List<Transaction>();
        public List<ProofSubmission> RecentProofs { get; set; } = new List<ProofSubmission>();
        public DashboardStatistics Statistics { get; set; } = new DashboardStatistics();
    }

    public class DashboardStatistics
    {
        public int TotalBuyerTransactions { get; set; }
        public int TotalSellerTransactions { get; set; }
        public int CompletedTransactions { get; set; }
        public int ActiveTransactions { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal TotalEarned { get; set; }
    }

    public class TransactionDetailsViewModel
    {
        public Transaction Transaction { get; set; } = new Transaction();
        public User? Buyer { get; set; }
        public User? Seller { get; set; }
        public EscrowAccount? EscrowAccount { get; set; }
        public List<ProofSubmission> ProofSubmissions { get; set; } = new List<ProofSubmission>();
        public string CurrentUserId { get; set; } = string.Empty;
        public bool IsCurrentUserBuyer { get; set; }
        public bool IsCurrentUserSeller { get; set; }
    }
}
