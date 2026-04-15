using System.Collections.Concurrent;
using BankingKata_MVC.Models.Interfaces;

namespace BankingKata_MVC.Models;

public class BankAccountRepository : IBankAccountRepository
{
    private readonly ConcurrentDictionary<string, BankAccount> _accounts = new();

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

public class TransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();
    private readonly object _lock = new();

    public void Save(Transaction transaction)
    {
        lock (_lock)
        {
            _transactions.Add(transaction);
        }
    }

    public IEnumerable<Transaction> GetByAccountNumberInRange(string accountNumber, DateTime fromDate, DateTime toDate)
    {
        lock (_lock)
        {
            return _transactions
                .Where(t => t.AccountNumber == accountNumber && t.Date >= fromDate && t.Date <= toDate)
                .OrderByDescending(t => t.Date)
                .ToList();
        }
    }
}

public class SavingsAccountRepository : ISavingsAccountRepository
{
    private readonly ConcurrentDictionary<string, SavingsAccount> _accounts = new();

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
