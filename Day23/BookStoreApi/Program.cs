using BookStoreApi.Models;
using BookStoreApi.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Bind MongoDB settings
builder.Services.Configure<BookstoreDatabaseSettings>(
    builder.Configuration.GetSection("BookstoreDatabaseSettings"));

// Register MongoClient
builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetValue<string>("BookstoreDatabaseSettings:ConnectionString")));

// Register BooksService
builder.Services.AddSingleton<BooksService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();