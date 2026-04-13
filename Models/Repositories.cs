namespace BankingKata_MVC.Models;

public class BankAccountRepository
{
    private readonly Dictionary<string, BankAccount> _accounts = new();

    public bool Exists(string accountNumber) => _accounts.ContainsKey(accountNumber);

    public BankAccount? GetByAccountNumber(string accountNumber)
    {
        _accounts.TryGetValue(accountNumber, out var account);
        return account;
    }

    public IEnumerable<BankAccount> GetAll() => _accounts.Values;

    public void Save(BankAccount account)
    {
        _accounts[account.AccountNumber] = account;
    }

    public void Update(BankAccount account)
    {
        _accounts[account.AccountNumber] = account;
    }
}

public class TransactionRepository
{
    private readonly List<Transaction> _transactions = new();

    public void Save(Transaction transaction)
    {
        _transactions.Add(transaction);
    }

    public IEnumerable<Transaction> GetByAccountNumberInRange(string accountNumber, DateTime fromDate, DateTime toDate)
    {
        return _transactions
            .Where(t => t.AccountNumber == accountNumber && t.Date >= fromDate && t.Date <= toDate)
            .OrderByDescending(t => t.Date);
    }
}

public class SavingsAccountRepository
{
    private readonly Dictionary<string, SavingsAccount> _accounts = new();

    public bool Exists(string accountNumber) => _accounts.ContainsKey(accountNumber);

    public SavingsAccount? GetByAccountNumber(string accountNumber)
    {
        _accounts.TryGetValue(accountNumber, out var account);
        return account;
    }

    public IEnumerable<SavingsAccount> GetAll() => _accounts.Values;

    public void Save(SavingsAccount account)
    {
        _accounts[account.AccountNumber] = account;
    }

    public void Update(SavingsAccount account)
    {
        _accounts[account.AccountNumber] = account;
    }
}