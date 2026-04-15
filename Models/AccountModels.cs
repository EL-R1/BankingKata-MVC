namespace BankingKata_MVC.Models;

public class BankAccount
{
    private readonly object _lock = new();
    
    public string AccountNumber { get; private set; }
    public decimal Balance { get; private set; }
    public decimal OverdraftLimit { get; private set; }

    public BankAccount(string accountNumber, decimal initialBalance = 0, decimal overdraftLimit = 0)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));
        if (overdraftLimit < 0)
            throw new ArgumentException("Overdraft limit cannot be negative", nameof(overdraftLimit));
        
        AccountNumber = accountNumber;
        Balance = initialBalance;
        OverdraftLimit = overdraftLimit;
    }

    public void Deposit(decimal amount)
    {
        lock (_lock)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive", nameof(amount));
            
            Balance += amount;
        }
    }

    public void Withdraw(decimal amount)
    {
        lock (_lock)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));
            
            if (amount > Balance + OverdraftLimit)
                throw new InvalidOperationException("Insufficient funds for withdrawal");
            
            Balance -= amount;
        }
    }

    public void SetOverdraftLimit(decimal limit)
    {
        if (limit < 0)
            throw new ArgumentException("Overdraft limit cannot be negative", nameof(limit));
        
        OverdraftLimit = limit;
    }
}

public enum TransactionType
{
    Deposit,
    Withdrawal
}

public class Transaction
{
    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public DateTime Date { get; private set; }
    public decimal BalanceAfterTransaction { get; private set; }

    public Transaction(string accountNumber, decimal amount, TransactionType type, decimal balanceAfterTransaction)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));
        
        Id = Guid.NewGuid();
        AccountNumber = accountNumber;
        Amount = amount;
        Type = type;
        Date = DateTime.UtcNow;
        BalanceAfterTransaction = balanceAfterTransaction;
    }
}

public class SavingsAccount
{
    private readonly object _lock = new();
    
    public string AccountNumber { get; private set; }
    public decimal Balance { get; private set; }
    public decimal DepositCeiling { get; private set; }

    public SavingsAccount(string accountNumber, decimal depositCeiling, decimal initialBalance = 0)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));
        if (depositCeiling <= 0)
            throw new ArgumentException("Deposit ceiling must be positive", nameof(depositCeiling));
        if (initialBalance > depositCeiling)
            throw new ArgumentException("Initial balance cannot exceed deposit ceiling", nameof(initialBalance));
        
        AccountNumber = accountNumber;
        DepositCeiling = depositCeiling;
        Balance = initialBalance;
    }

    public void Deposit(decimal amount)
    {
        lock (_lock)
        {
            if (amount <= 0)
                throw new ArgumentException("Deposit amount must be positive", nameof(amount));
            
            if (Balance + amount > DepositCeiling)
                throw new InvalidOperationException($"Deposit would exceed the ceiling of {DepositCeiling}");
            
            Balance += amount;
        }
    }

    public void Withdraw(decimal amount)
    {
        lock (_lock)
        {
            if (amount <= 0)
                throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));
            
            if (amount > Balance)
                throw new InvalidOperationException("Insufficient funds for withdrawal");
            
            Balance -= amount;
        }
    }
}
