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

public class SavingsControllerTests
{
    private readonly Mock<ISavingsAccountRepository> _mockRepository;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<SavingsController>> _mockLogger;
    private readonly SavingsController _controller;

    public SavingsControllerTests()
    {
        _mockRepository = new Mock<ISavingsAccountRepository>();
        _mockLogger = new Mock<ILogger<SavingsController>>();
        
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AccountMappingProfile>());
        _mapper = config.CreateMapper();

        _controller = new SavingsController(
            _mockRepository.Object,
            _mapper,
            _mockLogger.Object);
    }

    [Fact]
    public void GetAll_ReturnsEmptyList_WhenNoAccounts()
    {
        _mockRepository.Setup(r => r.GetAll()).Returns(new List<SavingsAccount>());

        var result = _controller.GetAll();
        
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var accounts = Assert.IsAssignableFrom<IEnumerable<SavingsAccountViewModel>>(okResult.Value);
        Assert.Empty(accounts);
    }

    [Fact]
    public void Create_ValidSavingsAccount_ReturnsCreated()
    {
        var model = new CreateSavingsAccountViewModel
        {
            AccountNumber = "SAV001",
            DepositCeiling = 10000,
            InitialBalance = 500
        };

        _mockRepository.Setup(r => r.Exists(model.AccountNumber)).Returns(false);
        _mockRepository.Setup(r => r.Save(It.IsAny<SavingsAccount>()));

        var result = _controller.Create(model);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var account = Assert.IsType<SavingsAccountViewModel>(createdResult.Value);
        Assert.Equal("SAV001", account.AccountNumber);
        Assert.Equal(500, account.Balance);
        Assert.Equal(10000, account.DepositCeiling);
    }

    [Fact]
    public void Create_DuplicateAccount_ReturnsConflict()
    {
        var model = new CreateSavingsAccountViewModel
        {
            AccountNumber = "SAV001",
            DepositCeiling = 10000
        };

        _mockRepository.Setup(r => r.Exists(model.AccountNumber)).Returns(true);

        var result = _controller.Create(model);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public void Get_ExistingAccount_ReturnsAccount()
    {
        var account = new SavingsAccount("SAV001", 10000);
        _mockRepository.Setup(r => r.GetByAccountNumber("SAV001")).Returns(account);

        var result = _controller.Get("SAV001");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedAccount = Assert.IsType<SavingsAccountViewModel>(okResult.Value);
        Assert.Equal("SAV001", returnedAccount.AccountNumber);
    }

    [Fact]
    public void Get_NonExistingAccount_ReturnsNotFound()
    {
        _mockRepository.Setup(r => r.GetByAccountNumber("NONEXISTENT")).Returns((SavingsAccount?)null);

        var result = _controller.Get("NONEXISTENT");

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Deposit_ValidAmount_IncreasesBalance()
    {
        var account = new SavingsAccount("SAV001", 10000, 100);
        _mockRepository.Setup(r => r.GetByAccountNumber("SAV001")).Returns(account);
        _mockRepository.Setup(r => r.Update(It.IsAny<SavingsAccount>()));

        var result = _controller.Deposit("SAV001", new TransactionViewModel { Amount = 50 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedAccount = Assert.IsType<SavingsAccountViewModel>(okResult.Value);
        Assert.Equal(150, updatedAccount.Balance);
    }

    [Fact]
    public void Deposit_ExceedsCeiling_ReturnsBadRequest()
    {
        var account = new SavingsAccount("SAV001", 100, 50);
        _mockRepository.Setup(r => r.GetByAccountNumber("SAV001")).Returns(account);

        var result = _controller.Deposit("SAV001", new TransactionViewModel { Amount = 60 });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Withdraw_ValidAmount_DecreasesBalance()
    {
        var account = new SavingsAccount("SAV001", 10000, 100);
        _mockRepository.Setup(r => r.GetByAccountNumber("SAV001")).Returns(account);
        _mockRepository.Setup(r => r.Update(It.IsAny<SavingsAccount>()));

        var result = _controller.Withdraw("SAV001", new TransactionViewModel { Amount = 30 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updatedAccount = Assert.IsType<SavingsAccountViewModel>(okResult.Value);
        Assert.Equal(70, updatedAccount.Balance);
    }

    [Fact]
    public void Withdraw_InsufficientFunds_ReturnsBadRequest()
    {
        var account = new SavingsAccount("SAV001", 10000, 50);
        _mockRepository.Setup(r => r.GetByAccountNumber("SAV001")).Returns(account);

        var result = _controller.Withdraw("SAV001", new TransactionViewModel { Amount = 100 });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Deposit_NonExistingAccount_ReturnsNotFound()
    {
        _mockRepository.Setup(r => r.GetByAccountNumber("NONEXISTENT")).Returns((SavingsAccount?)null);

        var result = _controller.Deposit("NONEXISTENT", new TransactionViewModel { Amount = 50 });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
