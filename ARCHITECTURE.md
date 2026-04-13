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

    style HTTP fill:#e8f4f8,stroke:#333,color:#000000
    style AC fill:#d4e8f4,stroke:#333,color:#000000
    style SC fill:#d4e8f4,stroke:#333,color:#000000
    style AV fill:#e4d4f4,stroke:#333,color:#000000
    style SV fill:#e4d4f4,stroke:#333,color:#000000
    style OV fill:#e4d4f4,stroke:#333,color:#000000
    style StV fill:#e4d4f4,stroke:#333,color:#000000
    style BA fill:#e8f4d4,stroke:#333,color:#000000
    style SA fill:#e8f4d4,stroke:#333,color:#000000
    style T fill:#e8f4d4,stroke:#333,color:#000000
    style BR fill:#d4f4e8,stroke:#333,color:#000000
    style SR fill:#d4f4e8,stroke:#333,color:#000000
    style TR fill:#d4f4e8,stroke:#333,color:#000000
```

### Flux de données

1. **Requête HTTP** → Controller
2. **Controller** → ViewModel (prépare la réponse)
3. **Controller** → Model/Repository (logique métier + données)
4. **Model** → Repository (persistance in-memory)
5. **ViewModel** → Réponse JSON

### Structure du projet

```
BankingKata-MVC/
├── Controllers/      # Endpoints API
│   ├── AccountsController.cs
│   └── SavingsController.cs
├── ViewModels/       # Modèles de réponse
├── Models/           # Entités + Repositories
│   ├── AccountModels.cs
│   └── Repositories.cs
└── Program.cs        # Configuration
```