# User Service

Clean Architecture backend skeleton for User Service.

## Layers

- $domain
- $application
- $persistence
- $infrastructure
- $api

## Run locally

`ash
dotnet restore
dotnet build
dotnet run --project src/Presentation/ECommerceAuction.UserService.Api/ECommerceAuction.UserService.Api.csproj
`

## Health check

`	ext
GET /api/health
`

## Database

For the learning/MVP setup, all services can point to the same physical SQL Server database ECommerceAuctionDb, but each service should only map and write its own owned tables.
