using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using BankingKata_MVC.Models;
using BankingKata_MVC.Models.Interfaces;
using BankingKata_MVC.ViewModels;

namespace BankingKata_MVC.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : Controller
{
    private readonly IBankAccountRepository _repository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(
        IBankAccountRepository repository,
        ITransactionRepository transactionRepository,
        IMapper mapper,
        ILogger<AccountsController> logger)
    {
        _repository = repository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountViewModel>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<AccountViewModel>> GetAll()
    {
        var accounts = _repository.GetAll().Select(a => _mapper.Map<AccountViewModel>(a));
        return Ok(accounts);
    }

    [HttpGet("{accountNumber}")]
    [ProducesResponseType(typeof(AccountViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AccountViewModel> Get(string accountNumber)
    {
        var account = _repository.GetByAccountNumber(accountNumber);
        if (account is null)
        {
            _logger.LogWarning("Account not found: {AccountNumber}", accountNumber);
            return NotFound(new { message = $"Account {accountNumber} not found" });
        }
        
        return Ok(_mapper.Map<AccountViewModel>(account));
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<AccountViewModel> Create([FromBody] CreateAccountViewModel model)
    {
        try
        {
            if (_repository.Exists(model.AccountNumber))
            {
                _logger.LogWarning("Duplicate account creation attempted: {AccountNumber}", model.AccountNumber);
                return Conflict(new { message = $"Account {model.AccountNumber} already exists" });
            }

            var account = new BankAccount(model.AccountNumber, model.InitialBalance, model.OverdraftLimit);
            _repository.Save(account);
            
            _logger.LogInformation("Account created: {AccountNumber} with initial balance: {Balance}", 
                account.AccountNumber, account.Balance);
            
            return CreatedAtAction(nameof(Get), new { accountNumber = account.AccountNumber }, 
                _mapper.Map<AccountViewModel>(account));
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid account creation request");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{accountNumber}/deposit")]
    [ProducesResponseType(typeof(AccountViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AccountViewModel> Deposit(string accountNumber, [FromBody] TransactionViewModel model)
    {
        try
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Account {accountNumber} not found");

            account.Deposit(model.Amount);
            _repository.Update(account);
            
            RecordTransaction(accountNumber, model.Amount, TransactionType.Deposit, account.Balance);
            
            _logger.LogInformation("Deposit successful: {AccountNumber} - {Amount}€ -> New balance: {Balance}€", 
                accountNumber, model.Amount, account.Balance);
            
            return Ok(_mapper.Map<AccountViewModel>(account));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Deposit failed - account not found: {AccountNumber}", accountNumber);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Deposit failed - invalid amount: {AccountNumber}", accountNumber);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{accountNumber}/withdraw")]
    [ProducesResponseType(typeof(AccountViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AccountViewModel> Withdraw(string accountNumber, [FromBody] TransactionViewModel model)
    {
        try
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Account {accountNumber} not found");

            account.Withdraw(model.Amount);
            _repository.Update(account);
            
            RecordTransaction(accountNumber, model.Amount, TransactionType.Withdrawal, account.Balance);
            
            _logger.LogInformation("Withdrawal successful: {AccountNumber} - {Amount}€ -> New balance: {Balance}€", 
                accountNumber, model.Amount, account.Balance);
            
            return Ok(_mapper.Map<AccountViewModel>(account));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Insufficient funds"))
        {
            _logger.LogWarning("Insufficient funds: {AccountNumber} - attempted: {Amount}€", 
                accountNumber, model.Amount);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Withdrawal failed - account not found: {AccountNumber}", accountNumber);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Withdrawal failed - invalid amount: {AccountNumber}", accountNumber);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{accountNumber}/overdraft")]
    [ProducesResponseType(typeof(AccountViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AccountViewModel> SetOverdraft(string accountNumber, [FromBody] OverdraftViewModel model)
    {
        try
        {
            var account = _repository.GetByAccountNumber(accountNumber)
                ?? throw new InvalidOperationException($"Account {accountNumber} not found");

            account.SetOverdraftLimit(model.OverdraftLimit);
            _repository.Update(account);
            
            _logger.LogInformation("Overdraft limit updated: {AccountNumber} -> {Limit}€", 
                accountNumber, model.OverdraftLimit);
            
            return Ok(_mapper.Map<AccountViewModel>(account));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Set overdraft failed - account not found: {AccountNumber}", accountNumber);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Set overdraft failed - invalid limit: {AccountNumber}", accountNumber);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{accountNumber}/statement")]
    [ProducesResponseType(typeof(StatementViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                Transactions = transactions.Select(t => _mapper.Map<OperationViewModel>(t)).ToList()
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Statement failed - account not found: {AccountNumber}", accountNumber);
            return NotFound(new { message = ex.Message });
        }
    }

    private void RecordTransaction(string accountNumber, decimal amount, TransactionType type, decimal balanceAfter)
    {
        var transaction = new Transaction(accountNumber, amount, type, balanceAfter);
        _transactionRepository.Save(transaction);
    }
}
