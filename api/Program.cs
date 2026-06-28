using System.Text.Json.Serialization;
using Amazon.S3;
using Api.Common;
using Api.Features.Categories.Archive;
using Api.Features.Categories.Create;
using Api.Features.Categories.Get;
using Api.Features.Categories.Restore;
using Api.Features.Categories.Update;
using Api.Features.ExchangeRates;
using Api.Features.Receipts.ScanReceipt;
using Api.Features.Receipts.UploadReceipt;
using Api.Features.Transactions.Create;
using Api.Features.Transactions.Delete;
using Api.Features.Transactions.Get;
using Api.Features.Transactions.GetById;
using Api.Features.Transactions.Update;
using Api.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Serilog
builder.Services.AddSerilog(config => config
    .MinimumLevel.Information()
    .WriteTo.Console());


//API NBP for exchange rates
builder.Services.AddHttpClient<NbpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.nbp.pl/api/");
})
.AddStandardResilienceHandler();

// Minio - S3 Storage for receipts
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("Storage");
    var config = new AmazonS3Config
    {
        ServiceURL = cfg["ServiceUrl"],
        ForcePathStyle = true
    };
    return new AmazonS3Client(cfg["AccessKey"], cfg["SecretKey"], config);
});

builder.Services.AddSingleton<ReceiptStorage>();
builder.Services.AddSingleton<ReceiptScanner>();

builder.Services.AddScoped<CurrencyConverter>();



builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Categories
app.MapCreateCategoryEndpoint();
app.MapUpdateCategoryEndpoint();
app.MapGetCategoriesEndpoint();
app.MapArchiveCategoryEndpoint();
app.MapRestoreCategoryEndpoint();

// Transactions
app.MapCreateTransactionEndpoint();
app.MapGetTransactionEndpoint();
app.MapGetTransactionByIdEndpoint();
app.MapUpdateTransactionEndpoint();
app.MapDeleteTransactionEndpoint();
app.MapGetEuroRateEndpoint();

// Receipts
app.MapUploadReceiptEndpoint();
app.MapScanReceiptEndpoint();




app.Run();

public partial class Program { }
