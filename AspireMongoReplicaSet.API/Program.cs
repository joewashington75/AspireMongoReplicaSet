using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMongoDBClient("MongoDb");

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/test", async (IMongoClient client) =>
    {
        var collection = client.GetDatabase("Test").GetCollection<BsonDocument>("Test");
        await collection.InsertOneAsync([]);
        return await collection.CountDocumentsAsync(e => true);
    })
    .WithName("GetTest");

app.Run();

public abstract partial class Program;
