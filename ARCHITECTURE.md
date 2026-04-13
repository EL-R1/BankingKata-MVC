# Architecture MVC - BankingKata-MVC

```mermaid
graph TD
    subgraph Client
        HTTP[HTTP Requests]
    end

    subgraph Controllers
        AC[AccountsController<br/>api/accounts]
        SC[SavingsController<br/>api/savings]
    end

    subgraph ViewModels
        AV[AccountViewModel]
        SV[SavingsAccountViewModel]
        OV[OperationViewModel]
        StV[StatementViewModel]
    end

    subgraph Models
        BA[BankAccount]
        SA[SavingsAccount]
        T[Transaction]
        BR[BankAccountRepository]
        SR[SavingsAccountRepository]
        TR[TransactionRepository]
    end

    HTTP --> AC
    HTTP --> SC

    AC --> AV
    SC --> SV

    AV --> BA
    SV --> SA
    AC --> T
    AC --> BR
    AC --> TR
    SC --> SR

    style HTTP fill:#f9f,stroke:#333
    style AC fill:#bbf,stroke:#333
    style SC fill:#bbf,stroke:#333
    style BA fill:#dfd,stroke:#333
    style SA fill:#dfd,stroke:#333
    style T fill:#dfd,stroke:#333
```

### Flux de données

1. **Requête HTTP** → Controller
2. **Controller** → ViewModel (prépare la réponse)
3. **Controller** → Model/Repository (logique métier + données)
4. **Model** → Repository (persistance in-memory)
5. **ViewModel** → Réponse JSON