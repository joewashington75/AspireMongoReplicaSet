# AspireMongoReplicaSet
This repository shows how to enable replica sets for MongoDB when using Aspire.

The AspireMongoReplicaSet.Replica Project is where the replica set is configured, and where the health checks are created for Aspire to determine if the resource is ready.

A simple endpoint with a test has been created to test the database persists documents within the database.