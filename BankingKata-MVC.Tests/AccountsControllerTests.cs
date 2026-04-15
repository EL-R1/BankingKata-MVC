using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using BankingKata_MVC.Controllers;
using BankingKata_MVC.Mapping;
using BankingKata_MVC.Models;
using BankingKata_MVC.Models.Interfaces;
using BankingKata_MVC.ViewModels;
using Xunit;

namespace BankingKata_MVC.Tests;

public class AccountsControllerTests
{
    private readonly Mock<IBankAccountRepository> _mockRepository;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<AccountsController>> _mockLogger;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _mockRepository = new Mock<IBankAccountRepository>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockLogger = new Mock<ILogger<AccountsController>>();
        
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AccountMappingProfile>());
        _mapper = config.CreateMapper();

        _controller = new AccountsController(
            _mockRepository.Object,
            _mockTransactionRepository.Object,
            _mapper,
            _mockLogger.Object);
    }

    [Fact]
    public void GetAll_ReturnsEmptyList_WhenNoAccounts()
    {
        _mockRepository.Setup(r => r.GetAll()).Returns(new List<BankAccount>());

        var result = _controller.GetAll();
        
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var accounts = Assert.IsAssignableFrom<IEnumerable<AccountViewModel>>(okResult.Value);
        Assert.Empty(accounts);
    }

    [Fact]
    public void Create_ValidAccount_ReturnsCreated()
    {
        var model = new CreateAccountViewModel
        {
            AccountNumber = "ACC001",
            InitialBalance = 1000,
            OverdraftLimit = 500
        };

        _mockRepository.Setup(r => r.Exists(model.AccountNumber)).Returns(false);
        _mockRepository.Setup(r => r.Save(It.IsAny<BankAccount>()));

        var result = _controller.Create(model);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var account = Assert.IsType<AccountViewModel>(createdResult.Value);
        Assert.Equal("ACC001", account.AccountNumber);
        Assert.Equal(1000, account.Balance);
        Assert.Equal(500, account.OverdraftLimit);
    }

    [Fact]
    public void Create_DuplicateAccount_ReturnsConflict()
    {
        var model = new CreateAccountViewModel
        {
            AccountNumber = "ACC001",
            InitialBalance = 1000
        };

        _mockRepository.Setup(r => r.Exists(model.AccountNumber)).Returns(true);

        var result = _controller.Create(model);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public void Get_ExistingAccount_ReturnsAccount()
    {
        var account = new BankAccount("ACC001", 500);
        _mockRepository.Setup(r => r.GetByAccountNumber("ACC001")).Returns(account);

        var result = _controller.Get("ACC001");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAccount = Assert.IsType<AccountViewModel>(okResult.Value);
        Assert.Equal("ACC001", returnedAccount.AccountNumber);
    }

    [Fact]
    public void Get_NonExistingAccount_ReturnsNotFound()
    {
        _mockRepository.Setup(r => r.GetByAccountNumber("NONEXISTENT")).Returns((BankAccount?)null);

        var result = _controller.Get("NONEXISTENT");

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Deposit_ValidAmount_IncreasesBalance()
    {
        var account = new BankAccount("ACC001", 100);
        _mockRepository.Setup(r => r.GetByAccountNumber("ACC001")).Returns(account);
        _mockRepository.Setup(r => r.Update(It.IsAny<BankAccount>()));
        _mockTransactionRepository.Setup(r => r.Save(It.IsAny<Transaction>()));

        var result = _controller.Deposit("ACC001", new TransactionViewModel { Amount = 50 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedAccount = Assert.IsType<AccountViewModel>(okResult.Value);
        Assert.Equal(150, updatedAccount.Balance);
    }

    [Fact]
    public void Deposit_NonExistingAccount_ReturnsNotFound()
    {
        _mockRepository.Setup(r => r.GetByAccountNumber("NONEXISTENT")).Returns((BankAccount?)null);

        var result = _controller.Deposit("NONEXISTENT", new TransactionViewModel { Amount = 50 });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Withdraw_ValidAmount_DecreasesBalance()
    {
        var account = new BankAccount("ACC001", 100);
        _mockRepository.Setup(r => r.GetByAccountNumber("ACC001")).Returns(account);
        _mockRepository.Setup(r => r.Update(It.IsAny<BankAccount>()));
        _mockTransactionRepository.Setup(r => r.Save(It.IsAny<Transaction>()));

        var result = _controller.Withdraw("ACC001", new TransactionViewModel { Amount = 30 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedAccount = Assert.IsType<AccountViewModel>(okResult.Value);
        Assert.Equal(70, updatedAccount.Balance);
    }

    [Fact]
    public void Withdraw_InsufficientFunds_ReturnsBadRequest()
    {
        var account = new BankAccount("ACC001", 100);
        _mockRepository.Setup(r => r.GetByAccountNumber("ACC001")).Returns(account);

        var result = _controller.Withdraw("ACC001", new TransactionViewModel { Amount = 150 });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void SetOverdraft_ValidLimit_UpdatesOverdraft()
    {
        var account = new BankAccount("ACC001", 100);
        _mockRepository.Setup(r => r.GetByAccountNumber("ACC001")).Returns(account);
        _mockRepository.Setup(r => r.Update(It.IsAny<BankAccount>()));

        var result = _controller.SetOverdraft("ACC001", new OverdraftViewModel { OverdraftLimit = 200 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedAccount = Assert.IsType<AccountViewModel>(okResult.Value);
        Assert.Equal(200, updatedAccount.OverdraftLimit);
    }

    [Fact]
    public void GetStatement_ExistingAccount_ReturnsStatement()
    {
        var account = new BankAccount("ACC001", 150);
        _mockRepository.Setup(r => r.GetByAccountNumber("ACC001")).Returns(account);
        _mockTransactionRepository
            .Setup(r => r.GetByAccountNumberInRange("ACC001", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(new List<Transaction>());

        var result = _controller.GetStatement("ACC001", null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var statement = Assert.IsType<StatementViewModel>(okResult.Value);
        Assert.Equal("ACC001", statement.AccountNumber);
        Assert.Equal(150, statement.Balance);
    }
}
