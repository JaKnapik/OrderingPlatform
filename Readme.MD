# Cloud-Native Order System (.NET 10 + Azure)

Showcase of microservices with Azure.

## Architecture
- **Ordering Service**: .NET 10, Azure SQL, EF Core, Outbox Pattern.
- **Inventory Service**: .NET 10, Cosmos DB, Inbox Pattern.
- **Notification Function**: Azure Functions, Service Bus Trigger.
- **API Gateway**: Azure API Management (APIM).

## Technologies
- **Messaging**: MassTransit + Azure Service Bus.
- **Security**: MS Identity, Key Vault, Managed Identity.
- **Observability**: OpenTelemetry + Jaeger/Grafana.
- **Infra**: Terraform.