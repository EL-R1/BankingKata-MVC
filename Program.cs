using AutoMapper;
using BankingKata_MVC.Mapping;
using BankingKata_MVC.Models;
using BankingKata_MVC.Models.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Banking API",
        Version = "v1",
        Description = "API for banking operations including accounts, savings accounts, and transactions",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Banking Support",
            Email = "support@banking.com"
        }
    });
});

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AccountMappingProfile>(), typeof(AccountMappingProfile).Assembly);

builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddScoped<ISavingsAccountRepository, SavingsAccountRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking API v1");
});

app.MapControllers();

app.Run();
