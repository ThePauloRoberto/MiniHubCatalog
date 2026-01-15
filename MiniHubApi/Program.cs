using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using MiniHubApi.Application.Configuration;
using MiniHubApi.Application.Services.Implementations;
using MiniHubApi.Application.Services.Interfaces;
using MiniHubApi.Infrastructure.Data;
using MiniHubApi.Middlewares;
using MongoDB.Driver;
using ServerVersion = Microsoft.EntityFrameworkCore.ServerVersion;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers() 
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});;

// Swagger/ OpenAPI
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configuração da API Externa
builder.Services.Configure<ExternalApiSettings>(
    builder.Configuration.GetSection(ExternalApiSettings.SectionName)
    );


builder.Services.AddHttpClient("ExternalApi", client =>
{
    client.BaseAddress = new Uri("https://69657a38f6de16bde44a70f6.mockapi.io/:endpoint");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<IExternalApiService, ExternalApiService>();

// Services
builder.Services.AddScoped<IDataImportService, DataImportService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();

// USE-SECRETS
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

//Configuração do Mysql
var mySqlConnection = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        mySqlConnection,
        ServerVersion.AutoDetect(mySqlConnection),
        mysqlOptions =>
        {
            mysqlOptions.MigrationsAssembly("MiniHubApi.Infrastructure");
        }
        );
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
        options.LogTo(Console.WriteLine, LogLevel.Information);
    }
});

//Configuração do MongoDB
var mongoDbConnection = builder.Configuration.GetConnectionString("MongoDbConnection");

builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var settings = MongoClientSettings.FromConnectionString(mongoDbConnection);
    return new MongoClient(settings);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("MiniHubAuditoria");
});

var app = builder.Build();

//Configuração das Seeds
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    
    if (app.Environment.IsDevelopment())
    {
        await dbContext.SeedBasicDataAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    builder.Configuration.AddUserSecrets<Program>();
    app.MapOpenApi();
}

//Middlewares
app.UseMiddleware<ServiceLoggingMiddleware>();
app.UseMiddleware<ServiceExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
