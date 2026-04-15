using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using BankingKata_MVC.Models;
using BankingKata_MVC.Models.Interfaces;
using BankingKata_MVC.ViewModels;

namespace BankingKata_MVC.Controllers;

[ApiController]
[Route("api/savings")]
public class SavingsController : Controller
{
    private readonly ISavingsAccountRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<SavingsController> _logger;

    public SavingsController(
        ISavingsAccountRepository repository,
        IMapper mapper,
        ILogger<SavingsController> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<SavingsAccountViewModel>> GetAll()
    {
        var accounts = _repository.GetAll().Select(a => _mapper.Map<SavingsAccountViewModel>(a));
        return Ok(accounts);
    }

    [HttpGet("{accountNumber}")]
    public ActionResult<SavingsAccountViewModel> Get(string accountNumber)
    {
        var account = _repository.GetByAccountNumber(accountNumber);
        if (account is null)
        {
            _logger.LogWarning("Savings account not found: {AccountNumber}", accountNumber);
            return NotFound(new { message = $"Savings account {accountNumber} not found" });
        }
        
        return Ok(_mapper.Map<SavingsAccountViewModel>(account));
    }

    [HttpPost]
    public ActionResult<SavingsAccountViewModel> Create([FromBody] CreateSavingsAccountViewModel model)
    {
        try
        {
            if (_repository.Exists(model.AccountNumber))
            {
                _logger.LogWarning("Duplicate savings account creation attempted: {AccountNumber}", model.AccountNumber);
                return Conflict(new { message = $"Savings account {model.AccountNumber} already exists" });
            }

            var account = new SavingsAccount(model.AccountNumber, model.DepositCeiling, model.InitialBalance);
            _repository.Save(account);
            
            _logger.LogInformation("Savings account created: {AccountNumber} with initial balance: {Balance}", 
                account.AccountNumber, account.Balance);
            
            return CreatedAtAction(nameof(Get), new { accountNumber = account.AccountNumber }, 
                _mapper.Map<SavingsAccountViewModel>(account));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid savings account creation request");
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
            
            _logger.LogInformation("Savings deposit successful: {AccountNumber} - {Amount}€ -> New balance: {Balance}€", 
                accountNumber, model.Amount, account.Balance);
            
            return Ok(_mapper.Map<SavingsAccountViewModel>(account));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("exceed"))
        {
            _logger.LogWarning("Savings deposit ceiling exceeded: {AccountNumber} - attempted: {Amount}€", 
                accountNumber, model.Amount);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Savings deposit failed - account not found: {AccountNumber}", accountNumber);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Savings deposit failed - invalid amount: {AccountNumber}", accountNumber);
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
            
            _logger.LogInformation("Savings withdrawal successful: {AccountNumber} - {Amount}€ -> New balance: {Balance}€", 
                accountNumber, model.Amount, account.Balance);
            
            return Ok(_mapper.Map<SavingsAccountViewModel>(account));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient funds"))
        {
            _logger.LogWarning("Insufficient funds for savings withdrawal: {AccountNumber} - attempted: {Amount}€", 
                accountNumber, model.Amount);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Savings withdrawal failed - account not found: {AccountNumber}", accountNumber);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Savings withdrawal failed - invalid amount: {AccountNumber}", accountNumber);
            return BadRequest(new { message = ex.Message });
        }
    }
}
