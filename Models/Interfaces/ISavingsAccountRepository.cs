namespace BankingKata_MVC.Models.Interfaces;

public interface ISavingsAccountRepository
{
    bool Exists(string accountNumber);
    SavingsAccount? GetByAccountNumber(string accountNumber);
    IEnumerable<SavingsAccount> GetAll();
    void Save(SavingsAccount account);
    void Update(SavingsAccount account);
}
