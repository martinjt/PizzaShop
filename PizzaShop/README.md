# Running
## What are the different projects?
- **StoreFron**t: This is the project that the user interacts with. It is a web application that allows the user to place an order for a pizza.
- **PizzaShop**: This is the project that processes the orders. It is a console application that listens for orders and 
  cooks them. It assigns a courier to each pizza.
- **Courier**: This is the project that delivers the pizzas. It is a console application that listens for pizzas and 
  delivers them. YOU SHOULD RUN MULTIPLE INSTANCES OF THIS PROJECT TO SEE THE CONCURRENCY IN ACTION. We configure 
  three couriers. Each courier used its own named queues and streams to communicate. Each courier can deliver one pizza 
  at a time. 
  - alice, 
  - bob,
  - charlie. 
- **AsbGateway**: This is the project that abstracts the Azure Service Bus. It is a library that the other projects use to 
  send and receive messages from the Azure Service Bus. Lightweight, demo library.
- **KafkaGateway**: This is the project that abstracts the Kafka. It is a library that the other projects use to send and 
  receive messages from the Kafka. Lightweight, demo library.
- **AppHost**: This is the project that hosts the other projects. It is a console application that starts the 
  dependencies required by the other projects. It starts and the Kafka. This version does not start the Azure 
  Service Bus Emulator, you must do that independently.

## Using the ASB Emulator

To run the solution, you will need to have the Azure Service Bus Emulator running. You can download it from
[here](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview).

Follow the instructions to install and run the emulator.

In the folder AsbEmulator, you will find a Config directory, and in there a file called `config.json`. This file 
needs to be copied to the AsbEmulator and replaces the existing `config.json` file. This file contains the queues that we need to set up in the emulator.

## Setting Up Multiple Couriers

The couriers are configured through their `appsettings.json` files. You can run multiple instances of the Courier by 
adjusting the name in the `appsettings.json` file. Permissable values, are currently hardcoded in the StoreFront 
Program.cs file ("alice", "bob", "charlie").

## Running the sample
With the emulator running, you should be able to use the StoreFront project's `StoreFront.http` file to create an 
order for a pizza and use that to drive the whole flow.

## Other StoreFront Endpoints

The StoreFront contains an endpoint that allows you to see the orders that have been placed. You can use this to 
monitor the progress of the orders.

### Where to find the OpenAPI documentation

Using the MS OpenAPI tooling, you can see the API endpoints and the data that they expect and return at:

```
http://{host}/openapi/v1.json
```

## Design Notes

### Why not have a shared assembly for types like Order and Pizza

Messaging often involves sending data between different systems. If the systems are written in different languages, 
or are running on different platforms, then the data needs to be serialized and deserialized. This is often done 
using a format like JSON. This leads to the adage: "share schema and not type"; the contract is the schema of the 
serialized type. To keep with this approach, we do not share an assembly of common types, instead each application 
has its own types that map to the serialized schemas. In actuality, due to using dotnet throughout, these types are 
the same.

### Why not just read the orders from a shared database

Our design mirrors that of a microservices architecture. Each service is responsible for its own data. This allows 
different teams to work on each service. To prevent coupling, we do not want to share a database. Instead, we use a 
database per service. This allows each service to evolve independently.

Our messages contain the data that we need to communicate between the services.

