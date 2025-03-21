using AspireMongoReplicaSet.Replica;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("mongo-user", "admin");
var password = builder.AddParameter("mongo-password", "admin");
const int port = 27017;

var mongo = builder
    .AddMongoDB("mongo", port, username, password)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithReplicaSet("../AspireMongoReplicaSet.Replica");

var mongodb = mongo
    .AddDatabase("mongoDatabase");

var mongoReplicaSet = builder
    .AddMongoReplicaSet("mongoDb", mongodb.Resource);

builder.AddProject<AspireMongoReplicaSet_API>("aspiremongoreplicaset-api", "http")
    .WithReference(mongodb)
    .WithReference(mongoReplicaSet)
    .WaitFor(mongodb)
    .WaitFor(mongoReplicaSet);

await builder
    .Build()
    .RunAsync();