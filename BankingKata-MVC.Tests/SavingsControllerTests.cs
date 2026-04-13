using Microsoft.AspNetCore.Mvc;
using BankingKata_MVC.Controllers;
using BankingKata_MVC.ViewModels;
using Xunit;

namespace BankingKata_MVC.Tests;

public class SavingsControllerTests
{
    private readonly SavingsController _controller;

    public SavingsControllerTests()
    {
        _controller = new SavingsController();
    }

    [Fact]
    public void GetAll_ReturnsEmptyList_WhenNoAccounts()
    {
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

        _controller.Create(model);
        var result = _controller.Create(model);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public void Get_ExistingAccount_ReturnsAccount()
    {
        _controller.Create(new CreateSavingsAccountViewModel { AccountNumber = "SAV001", DepositCeiling = 10000 });

        var result = _controller.Get("SAV001");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var account = Assert.IsType<SavingsAccountViewModel>(okResult.Value);
        Assert.Equal("SAV001", account.AccountNumber);
    }

    [Fact]
    public void Get_NonExistingAccount_ReturnsNotFound()
    {
        var result = _controller.Get("NONEXISTENT");

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Deposit_ValidAmount_IncreasesBalance()
    {
        _controller.Create(new CreateSavingsAccountViewModel { AccountNumber = "SAV001", DepositCeiling = 10000, InitialBalance = 100 });

        var result = _controller.Deposit("SAV001", new TransactionViewModel { Amount = 50 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var account = Assert.IsType<SavingsAccountViewModel>(okResult.Value);
        Assert.Equal(150, account.Balance);
    }

    [Fact]
    public void Deposit_ExceedsCeiling_ReturnsBadRequest()
    {
        _controller.Create(new CreateSavingsAccountViewModel { AccountNumber = "SAV001", DepositCeiling = 100, InitialBalance = 50 });

        var result = _controller.Deposit("SAV001", new TransactionViewModel { Amount = 60 });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Withdraw_ValidAmount_DecreasesBalance()
    {
        _controller.Create(new CreateSavingsAccountViewModel { AccountNumber = "SAV001", DepositCeiling = 10000, InitialBalance = 100 });

        var result = _controller.Withdraw("SAV001", new TransactionViewModel { Amount = 30 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var account = Assert.IsType<SavingsAccountViewModel>(okResult.Value);
        Assert.Equal(70, account.Balance);
    }

    [Fact]
    public void Withdraw_InsufficientFunds_ReturnsBadRequest()
    {
        _controller.Create(new CreateSavingsAccountViewModel { AccountNumber = "SAV001", DepositCeiling = 10000, InitialBalance = 50 });

        var result = _controller.Withdraw("SAV001", new TransactionViewModel { Amount = 100 });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void Deposit_NonExistingAccount_ReturnsNotFound()
    {
        var result = _controller.Deposit("NONEXISTENT", new TransactionViewModel { Amount = 50 });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}