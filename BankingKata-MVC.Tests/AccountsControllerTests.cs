using Microsoft.AspNetCore.Mvc;
using BankingKata_MVC.Controllers;
using BankingKata_MVC.ViewModels;
using Xunit;

namespace BankingKata_MVC.Tests;

public class AccountsControllerTests
{
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _controller = new AccountsController();
    }

    [Fact]
    public void GetAll_ReturnsEmptyList_WhenNoAccounts()
    {
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

        _controller.Create(model);
        var result = _controller.Create(model);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public void Get_ExistingAccount_ReturnsAccount()
    {
        _controller.Create(new CreateAccountViewModel { AccountNumber = "ACC001", InitialBalance = 500 });

        var result = _controller.Get("ACC001");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var account = Assert.IsType<AccountViewModel>(okResult.Value);
        Assert.Equal("ACC001", account.AccountNumber);
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
        _controller.Create(new CreateAccountViewModel { AccountNumber = "ACC001", InitialBalance = 100 });

        var result = _controller.Deposit("ACC001", new TransactionViewModel { Amount = 50 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var account = Assert.IsType<AccountViewModel>(okResult.Value);
        Assert.Equal(150, account.Balance);
    }

    [Fact]
    public void Deposit_NonExistingAccount_ReturnsNotFound()
    {
        var result = _controller.Deposit("NONEXISTENT", new TransactionViewModel { Amount = 50 });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public void Withdraw_ValidAmount_DecreasesBalance()
    {
        _controller.Create(new CreateAccountViewModel { AccountNumber = "ACC001", InitialBalance = 100 });

        var result = _controller.Withdraw("ACC001", new TransactionViewModel { Amount = 30 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var account = Assert.IsType<AccountViewModel>(okResult.Value);
        Assert.Equal(70, account.Balance);
    }

    [Fact]
    public void Withdraw_InsufficientFunds_ReturnsBadRequest()
    {
        _controller.Create(new CreateAccountViewModel { AccountNumber = "ACC001", InitialBalance = 100, OverdraftLimit = 0 });

        var result = _controller.Withdraw("ACC001", new TransactionViewModel { Amount = 150 });

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void SetOverdraft_ValidLimit_UpdatesOverdraft()
    {
        _controller.Create(new CreateAccountViewModel { AccountNumber = "ACC001", InitialBalance = 100, OverdraftLimit = 0 });

        var result = _controller.SetOverdraft("ACC001", new OverdraftViewModel { OverdraftLimit = 200 });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var account = Assert.IsType<AccountViewModel>(okResult.Value);
        Assert.Equal(200, account.OverdraftLimit);
    }

    [Fact]
    public void GetStatement_ExistingAccount_ReturnsStatement()
    {
        _controller.Create(new CreateAccountViewModel { AccountNumber = "ACC001", InitialBalance = 100 });
        _controller.Deposit("ACC001", new TransactionViewModel { Amount = 50 });

        var result = _controller.GetStatement("ACC001", null, null);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var statement = Assert.IsType<StatementViewModel>(okResult.Value);
        Assert.Equal("ACC001", statement.AccountNumber);
        Assert.Equal(150, statement.Balance);
    }
}