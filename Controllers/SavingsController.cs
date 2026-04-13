using Microsoft.AspNetCore.Mvc;
using BankingKata_MVC.Models;
using BankingKata_MVC.ViewModels;

namespace BankingKata_MVC.Controllers;

public class SavingsController : Controller
{
    private readonly SavingsAccountRepository _repository = new();

    [HttpGet]
    public ActionResult<IEnumerable<SavingsAccountViewModel>> GetAll()
    {
        var accounts = _repository.GetAll().Select(a => new SavingsAccountViewModel
        {
            AccountNumber = a.AccountNumber,
            Balance = a.Balance,
            DepositCeiling = a.DepositCeiling
        });
        return Ok(accounts);
    }

    [HttpGet("{accountNumber}")]
    public ActionResult<SavingsAccountViewModel> Get(string accountNumber)
    {
        var account = _repository.GetByAccountNumber(accountNumber);
        if (account is null)
            return NotFound(new { message = $"Savings account {accountNumber} not found" });
        
        return Ok(new SavingsAccountViewModel
        {
            AccountNumber = account.AccountNumber,
            Balance = account.Balance,
            DepositCeiling = account.DepositCeiling
        });
    }

    [HttpPost]
    public ActionResult<SavingsAccountViewModel> Create([FromBody] CreateSavingsAccountViewModel model)
    {
        try
        {
            if (_repository.Exists(model.AccountNumber))
                return Conflict(new { message = $"Savings account {model.AccountNumber} already exists" });

            var account = new SavingsAccount(model.AccountNumber, model.DepositCeiling, model.InitialBalance);
            _repository.Save(account);
            
            return CreatedAtAction(nameof(Get), new { accountNumber = account.AccountNumber }, new SavingsAccountViewModel
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                DepositCeiling = account.DepositCeiling
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{accountNumber}/deposit")]
    public ActionResult<SavingsAccountViewModel> Deposit(string accountNumber, [FromBody] TransactionViewModel model)
    {
        try
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Savings account {accountNumber} not found");

            account.Deposit(model.Amount);
            _repository.Update(account);
            
            return Ok(new SavingsAccountViewModel
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                DepositCeiling = account.DepositCeiling
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { message = ex.Message });
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

    [HttpPost("{accountNumber}/withdraw")]
    public ActionResult<SavingsAccountViewModel> Withdraw(string accountNumber, [FromBody] TransactionViewModel model)
    {
        try
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Savings account {accountNumber} not found");

            account.Withdraw(model.Amount);
            _repository.Update(account);
            
            return Ok(new SavingsAccountViewModel
            {
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                DepositCeiling = account.DepositCeiling
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
}