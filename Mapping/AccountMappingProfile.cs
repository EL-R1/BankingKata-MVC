using AutoMapper;
using BankingKata_MVC.Models;
using BankingKata_MVC.ViewModels;

namespace BankingKata_MVC.Mapping;

public class AccountMappingProfile : Profile
{
    public AccountMappingProfile()
    {
        CreateMap<BankAccount, AccountViewModel>();
        CreateMap<AccountViewModel, BankAccount>();

        CreateMap<SavingsAccount, SavingsAccountViewModel>();
        CreateMap<SavingsAccountViewModel, SavingsAccount>();

        CreateMap<Transaction, OperationViewModel>();
        CreateMap<OperationViewModel, Transaction>();
    }
}
