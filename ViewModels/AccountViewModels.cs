namespace BankingKata_MVC.ViewModels;

public class AccountViewModel
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal OverdraftLimit { get; set; }
}

public class CreateAccountViewModel
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal OverdraftLimit { get; set; }
}

public class TransactionViewModel
{
    public decimal Amount { get; set; }
}

public class OverdraftViewModel
{
    public decimal OverdraftLimit { get; set; }
}

public class StatementViewModel
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime StatementDate { get; set; }
    public List<OperationViewModel> Transactions { get; set; } = new();
}

public class OperationViewModel
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal BalanceAfterTransaction { get; set; }
}

public class SavingsAccountViewModel
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal DepositCeiling { get; set; }
}

public class CreateSavingsAccountViewModel
{
    public string AccountNumber { get; set; } = string.Empty;
    public decimal DepositCeiling { get; set; }
    public decimal InitialBalance { get; set; }
}