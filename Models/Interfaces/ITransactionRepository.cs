namespace BankingKata_MVC.Models.Interfaces;

public interface ITransactionRepository
{
    void Save(Transaction transaction);
    IEnumerable<Transaction> GetByAccountNumberInRange(string accountNumber, DateTime fromDate, DateTime toDate);
}
