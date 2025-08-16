using PayPledge.Models;

namespace PayPledge.Services
{
    public interface IPaymentService
    {
        Task<PaymentTransaction> ProcessDepositAsync(string transactionId, string paymentMethodId, decimal amount);
        Task<PaymentTransaction> ProcessReleaseAsync(string escrowAccountId, decimal amount);
        Task<PaymentTransaction> ProcessRefundAsync(string escrowAccountId, decimal amount, string reason);
        Task<EscrowAccount> CreateEscrowAccountAsync(string transactionId, decimal amount);
        Task<bool> UpdateEscrowStatusAsync(string escrowAccountId, EscrowStatus status);
        Task<bool> CanReleaseFundsAsync(string escrowAccountId);
        Task<PaymentFees> CalculateFeesAsync(decimal amount, PaymentMethodType methodType);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IRepository<PaymentTransaction> _paymentRepository;
        private readonly IRepository<EscrowAccount> _escrowRepository;
        private readonly IRepository<PaymentMethod> _paymentMethodRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly bool _mockMode;

        public PaymentService(
            IRepository<PaymentTransaction> paymentRepository,
            IRepository<EscrowAccount> escrowRepository,
            IRepository<PaymentMethod> paymentMethodRepository,
            IConfiguration configuration,
            ILogger<PaymentService> logger)
        {
            _paymentRepository = paymentRepository;
            _escrowRepository = escrowRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _configuration = configuration;
            _logger = logger;
            _mockMode = _configuration.GetValue<bool>("PaymentGateway:MockMode");
        }

        public async Task<PaymentTransaction> ProcessDepositAsync(string transactionId, string paymentMethodId, decimal amount)
        {
            try
            {
                _logger.LogInformation("Processing deposit for transaction {TransactionId}, amount: {Amount}",
                    transactionId, amount);

                var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
                if (paymentMethod == null)
                {
                    throw new ArgumentException("Payment method not found");
                }

                var fees = await CalculateFeesAsync(amount, paymentMethod.MethodType);

                var paymentTransaction = new PaymentTransaction
                {
                    TransactionId = transactionId,
                    PaymentMethodId = paymentMethodId,
                    Amount = amount,
                    Status = PaymentStatus.Processing,
                    PaymentType = PaymentType.Deposit,
                    Fees = fees
                };

                // Simulate payment processing
                var processingResult = await SimulatePaymentProcessing(paymentTransaction, paymentMethod);

                paymentTransaction.Status = processingResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
                paymentTransaction.ProcessedAt = DateTime.UtcNow;
                paymentTransaction.GatewayTransactionId = processingResult.GatewayTransactionId;
                paymentTransaction.GatewayResponse = processingResult.Response;

                if (!processingResult.Success)
                {
                    paymentTransaction.FailureReason = processingResult.FailureReason;
                }

                await _paymentRepository.CreateAsync(paymentTransaction);

                _logger.LogInformation("Deposit processing completed for transaction {TransactionId}. Status: {Status}",
                    transactionId, paymentTransaction.Status);

                return paymentTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing deposit for transaction {TransactionId}", transactionId);
                throw;
            }
        }

        public async Task<PaymentTransaction> ProcessReleaseAsync(string escrowAccountId, decimal amount)
        {
            try
            {
                _logger.LogInformation("Processing fund release for escrow {EscrowAccountId}, amount: {Amount}",
                    escrowAccountId, amount);

                var escrowAccount = await _escrowRepository.GetByIdAsync(escrowAccountId);
                if (escrowAccount == null)
                {
                    throw new ArgumentException("Escrow account not found");
                }

                if (escrowAccount.Balance < amount)
                {
                    throw new InvalidOperationException("Insufficient funds in escrow");
                }

                var paymentTransaction = new PaymentTransaction
                {
                    TransactionId = escrowAccount.TransactionId,
                    Amount = amount,
                    Status = PaymentStatus.Processing,
                    PaymentType = PaymentType.Release
                };

                // Simulate release processing
                var processingResult = await SimulateReleaseProcessing(paymentTransaction);

                paymentTransaction.Status = processingResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
                paymentTransaction.ProcessedAt = DateTime.UtcNow;
                paymentTransaction.GatewayTransactionId = processingResult.GatewayTransactionId;
                paymentTransaction.GatewayResponse = processingResult.Response;

                if (processingResult.Success)
                {
                    // Update escrow account
                    escrowAccount.Balance -= amount;
                    escrowAccount.Transactions.Add(new EscrowTransaction
                    {
                        Type = EscrowTransactionType.Release,
                        Amount = amount,
                        Description = "Funds released to seller",
                        PaymentReference = paymentTransaction.Id
                    });

                    if (escrowAccount.Balance == 0)
                    {
                        escrowAccount.Status = EscrowStatus.Released;
                        escrowAccount.FundsReleasedAt = DateTime.UtcNow;
                    }

                    await _escrowRepository.UpdateAsync(escrowAccountId, escrowAccount);
                }
                else
                {
                    paymentTransaction.FailureReason = processingResult.FailureReason;
                }

                await _paymentRepository.CreateAsync(paymentTransaction);

                _logger.LogInformation("Fund release completed for escrow {EscrowAccountId}. Status: {Status}",
                    escrowAccountId, paymentTransaction.Status);

                return paymentTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fund release for escrow {EscrowAccountId}", escrowAccountId);
                throw;
            }
        }

        public async Task<PaymentTransaction> ProcessRefundAsync(string escrowAccountId, decimal amount, string reason)
        {
            try
            {
                _logger.LogInformation("Processing refund for escrow {EscrowAccountId}, amount: {Amount}",
                    escrowAccountId, amount);

                var escrowAccount = await _escrowRepository.GetByIdAsync(escrowAccountId);
                if (escrowAccount == null)
                {
                    throw new ArgumentException("Escrow account not found");
                }

                if (escrowAccount.Balance < amount)
                {
                    throw new InvalidOperationException("Insufficient funds in escrow for refund");
                }

                var paymentTransaction = new PaymentTransaction
                {
                    TransactionId = escrowAccount.TransactionId,
                    Amount = amount,
                    Status = PaymentStatus.Processing,
                    PaymentType = PaymentType.Refund
                };

                // Simulate refund processing
                var processingResult = await SimulateRefundProcessing(paymentTransaction, reason);

                paymentTransaction.Status = processingResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
                paymentTransaction.ProcessedAt = DateTime.UtcNow;
                paymentTransaction.GatewayTransactionId = processingResult.GatewayTransactionId;
                paymentTransaction.GatewayResponse = processingResult.Response;

                if (processingResult.Success)
                {
                    // Update escrow account
                    escrowAccount.Balance -= amount;
                    escrowAccount.Transactions.Add(new EscrowTransaction
                    {
                        Type = EscrowTransactionType.Refund,
                        Amount = amount,
                        Description = $"Refund to buyer: {reason}",
                        PaymentReference = paymentTransaction.Id
                    });

                    if (escrowAccount.Balance == 0)
                    {
                        escrowAccount.Status = EscrowStatus.Refunded;
                    }

                    await _escrowRepository.UpdateAsync(escrowAccountId, escrowAccount);
                }
                else
                {
                    paymentTransaction.FailureReason = processingResult.FailureReason;
                }

                await _paymentRepository.CreateAsync(paymentTransaction);

                _logger.LogInformation("Refund processing completed for escrow {EscrowAccountId}. Status: {Status}",
                    escrowAccountId, paymentTransaction.Status);

                return paymentTransaction;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing refund for escrow {EscrowAccountId}", escrowAccountId);
                throw;
            }
        }

        public async Task<EscrowAccount> CreateEscrowAccountAsync(string transactionId, decimal amount)
        {
            try
            {
                var fees = await CalculateFeesAsync(amount, PaymentMethodType.CreditCard);

                var escrowAccount = new EscrowAccount
                {
                    TransactionId = transactionId,
                    Balance = 0, // Will be updated when deposit is processed
                    Status = EscrowStatus.Created,
                    Fees = new EscrowFees
                    {
                        ServiceFeeAmount = fees.PlatformFee,
                        PaymentProcessingFee = fees.ProcessingFee,
                        TotalFees = fees.TotalFees
                    }
                };

                await _escrowRepository.CreateAsync(escrowAccount);

                _logger.LogInformation("Escrow account created for transaction {TransactionId}", transactionId);

                return escrowAccount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating escrow account for transaction {TransactionId}", transactionId);
                throw;
            }
        }

        public async Task<bool> UpdateEscrowStatusAsync(string escrowAccountId, EscrowStatus status)
        {
            try
            {
                var escrowAccount = await _escrowRepository.GetByIdAsync(escrowAccountId);
                if (escrowAccount == null) return false;

                escrowAccount.Status = status;
                escrowAccount.UpdatedAt = DateTime.UtcNow;

                return await _escrowRepository.UpdateAsync(escrowAccountId, escrowAccount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating escrow status for {EscrowAccountId}", escrowAccountId);
                throw;
            }
        }

        public async Task<bool> CanReleaseFundsAsync(string escrowAccountId)
        {
            try
            {
                var escrowAccount = await _escrowRepository.GetByIdAsync(escrowAccountId);
                if (escrowAccount == null) return false;

                // Check if all release conditions are met
                return escrowAccount.Status == EscrowStatus.FundsHeld &&
                       escrowAccount.ReleaseConditions.All(c => c.IsMet) &&
                       escrowAccount.Balance > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking fund release eligibility for {EscrowAccountId}", escrowAccountId);
                throw;
            }
        }

        public async Task<PaymentFees> CalculateFeesAsync(decimal amount, PaymentMethodType methodType)
        {
            await Task.Delay(100); // Simulate calculation time

            var processingFeeRate = methodType switch
            {
                PaymentMethodType.CreditCard => 0.029m, // 2.9%
                PaymentMethodType.DebitCard => 0.025m,  // 2.5%
                PaymentMethodType.BankTransfer => 0.01m, // 1.0%
                PaymentMethodType.PayPal => 0.034m,     // 3.4%
                _ => 0.029m
            };

            var platformFeeRate = 0.025m; // 2.5% platform fee

            var processingFee = Math.Round(amount * processingFeeRate, 2);
            var platformFee = Math.Round(amount * platformFeeRate, 2);
            var totalFees = processingFee + platformFee;

            return new PaymentFees
            {
                ProcessingFee = processingFee,
                PlatformFee = platformFee,
                TotalFees = totalFees,
                FeeBreakdown = new Dictionary<string, decimal>
                {
                    ["processing"] = processingFee,
                    ["platform"] = platformFee
                }
            };
        }

        private async Task<PaymentProcessingResult> SimulatePaymentProcessing(PaymentTransaction transaction, PaymentMethod paymentMethod)
        {
            // Simulate processing delay
            await Task.Delay(new Random().Next(1000, 3000));

            if (_mockMode)
            {
                // Simulate 95% success rate in mock mode
                var success = new Random().NextDouble() > 0.05;

                return new PaymentProcessingResult
                {
                    Success = success,
                    GatewayTransactionId = success ? $"mock_txn_{Guid.NewGuid():N}" : null,
                    FailureReason = success ? null : "Simulated payment failure",
                    Response = new Dictionary<string, object>
                    {
                        ["gateway"] = "mock",
                        ["timestamp"] = DateTime.UtcNow,
                        ["amount"] = transaction.Amount,
                        ["currency"] = transaction.Currency
                    }
                };
            }

            // In real implementation, this would integrate with actual payment gateway
            throw new NotImplementedException("Real payment gateway integration not implemented");
        }

        private async Task<PaymentProcessingResult> SimulateReleaseProcessing(PaymentTransaction transaction)
        {
            await Task.Delay(new Random().Next(500, 1500));

            return new PaymentProcessingResult
            {
                Success = true,
                GatewayTransactionId = $"release_{Guid.NewGuid():N}",
                Response = new Dictionary<string, object>
                {
                    ["type"] = "release",
                    ["timestamp"] = DateTime.UtcNow,
                    ["amount"] = transaction.Amount
                }
            };
        }

        private async Task<PaymentProcessingResult> SimulateRefundProcessing(PaymentTransaction transaction, string reason)
        {
            await Task.Delay(new Random().Next(500, 1500));

            return new PaymentProcessingResult
            {
                Success = true,
                GatewayTransactionId = $"refund_{Guid.NewGuid():N}",
                Response = new Dictionary<string, object>
                {
                    ["type"] = "refund",
                    ["reason"] = reason,
                    ["timestamp"] = DateTime.UtcNow,
                    ["amount"] = transaction.Amount
                }
            };
        }
    }

    public class PaymentProcessingResult
    {
        public bool Success { get; set; }
        public string? GatewayTransactionId { get; set; }
        public string? FailureReason { get; set; }
        public Dictionary<string, object> Response { get; set; } = new();
    }
}
