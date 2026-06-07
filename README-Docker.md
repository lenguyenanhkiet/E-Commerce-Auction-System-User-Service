# Docker Guide - ECommerceAuction.UserService

This project is configured to run the ASP.NET Core API and SQL Server with Docker Compose.

## Prerequisites

- Docker Desktop
- .NET SDK 10 if you want to run EF Core migration from the host machine

## Files added

- `Dockerfile`: builds and runs the ASP.NET Core API container.
- `.dockerignore`: excludes Visual Studio, build output, and local files from Docker build context.
- `docker-compose.yml`: starts SQL Server and the API together.

## Run API + SQL Server

Open terminal at the project root, the folder that contains `ECommerceAuction.UserService.sln`.

```bash
docker compose up -d --build
```

API URL:

```text
http://localhost:5246
```

Swagger URL:

```text
http://localhost:5246/swagger
```

SQL Server connection from host machine:

```text
Server=localhost,1433;Database=ECommerceAuctionDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

SQL Server connection from API container:

```text
Server=sqlserver,1433;Database=ECommerceAuctionDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
```

## Stop containers

```bash
docker compose down
```

## Stop containers and remove database volume

Warning: this deletes SQL Server data stored by Docker.

```bash
docker compose down -v
```

## Environment variables

Default values are already set in `docker-compose.yml` for local development.

You can override them before running Docker Compose:

```bash
set SQLSERVER_SA_PASSWORD=YourStrong@Passw0rd
set API_PORT=5246
set SQLSERVER_PORT=1433
docker compose up -d --build
```

## EF Core migration

The compose file starts SQL Server, but it does not automatically run EF Core migrations.

Recommended for Sprint 1: run migration from the host machine after SQL Server is started.

```bash
dotnet ef database update --project src/Infrastructure/ECommerceAuction.UserService.Persistence --startup-project src/Presentation/ECommerceAuction.UserService.Api
```

Make sure your local connection string points to `localhost,1433` when running migration from the host.

