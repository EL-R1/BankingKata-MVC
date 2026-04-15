namespace BankingKata_MVC.Models.Interfaces;

public interface IBankAccountRepository
{
    bool Exists(string accountNumber);
    BankAccount? GetByAccountNumber(string accountNumber);
    IEnumerable<BankAccount> GetAll();
    void Save(BankAccount account);
    void Update(BankAccount account);
}
