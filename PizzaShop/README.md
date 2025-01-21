## Where to find the OpenAPI documentation

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

