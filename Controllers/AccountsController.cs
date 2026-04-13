using Microsoft.AspNetCore.Mvc;
using BankingKata_MVC.Models;
using BankingKata_MVC.ViewModels;

namespace BankingKata_MVC.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : Controller
{
    private readonly BankAccountRepository _repository = new();
    private readonly TransactionRepository _transactionRepository = new();

    [HttpGet]
    public ActionResult<IEnumerable<AccountViewModel>> GetAll()
    {
        var accounts = _repository.GetAll().Select(a => new AccountViewModel
        {
            AccountNumber = a.AccountNumber,
            Balance = a.Balance,
            OverdraftLimit = a.OverdraftLimit
        });
        return Ok(accounts);
    }

    [HttpGet("{accountNumber}")]
    public ActionResult<AccountViewModel> Get(string accountNumber)
    {
        var account = _repository.GetByAccountNumber(accountNumber);
        if (account is null)
            return NotFound(new { message = $"Account {accountNumber} not found" });
        
        return Ok(new AccountViewModel
        {
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            OverdraftLimit = account.OverdraftLimit
        });
    }

    [HttpPost]
    public ActionResult<AccountViewModel> Create([FromBody] CreateAccountViewModel model)
    {
        try
        {
            if (_repository.Exists(model.AccountNumber))
                return Conflict(new { message = $"Account {model.AccountNumber} already exists" });

            var account = new BankAccount(model.AccountNumber, model.InitialBalance, model.OverdraftLimit);
            _repository.Save(account);
            
            return CreatedAtAction(nameof(Get), new { accountNumber = account.AccountNumber }, new AccountViewModel
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                OverdraftLimit = account.OverdraftLimit
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{accountNumber}/deposit")]
    public ActionResult<AccountViewModel> Deposit(string accountNumber, [FromBody] TransactionViewModel model)
    {
        try
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Account {accountNumber} not found");

            account.Deposit(model.Amount);
            _repository.Update(account);
            
            RecordTransaction(accountNumber, model.Amount, TransactionType.Deposit, account.Balance);
            
            return Ok(new AccountViewModel
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                OverdraftLimit = account.OverdraftLimit
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{accountNumber}/withdraw")]
    public ActionResult<AccountViewModel> Withdraw(string accountNumber, [FromBody] TransactionViewModel model)
    {
        try
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Account {accountNumber} not found");

            account.Withdraw(model.Amount);
            _repository.Update(account);
            
            RecordTransaction(accountNumber, model.Amount, TransactionType.Withdrawal, account.Balance);
            
            return Ok(new AccountViewModel
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                OverdraftLimit = account.OverdraftLimit
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{accountNumber}/overdraft")]
    public ActionResult<AccountViewModel> SetOverdraft(string accountNumber, [FromBody] OverdraftViewModel model)
    {
        try
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Account {accountNumber} not found");

            account.SetOverdraftLimit(model.OverdraftLimit);
            _repository.Update(account);
            
            return Ok(new AccountViewModel
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                OverdraftLimit = account.OverdraftLimit
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{accountNumber}/statement")]
    public ActionResult<StatementViewModel> GetStatement(string accountNumber, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        try
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Account {accountNumber} not found");

            var to = toDate ?? DateTime.UtcNow;
            var from = fromDate ?? to.AddMonths(-1);

            var transactions = _transactionRepository.GetByAccountNumberInRange(accountNumber, from, to);

            return Ok(new StatementViewModel
            {
                AccountNumber = account.AccountNumber,
                AccountType = "Compte Courant",
                Balance = account.Balance,
                StatementDate = to,
                Transactions = transactions.Select(t => new OperationViewModel
                {
                    Id = t.Id,
                    AccountNumber = t.AccountNumber,
                    Amount = t.Amount,
                    Type = t.Type.ToString(),
                    Date = t.Date,
                    BalanceAfterTransaction = t.BalanceAfterTransaction
                }).ToList()
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private void RecordTransaction(string accountNumber, decimal amount, TransactionType type, decimal balanceAfter)
    {
        var transaction = new Transaction(accountNumber, amount, type, balanceAfter);
        _transactionRepository.Save(transaction);
    }
}