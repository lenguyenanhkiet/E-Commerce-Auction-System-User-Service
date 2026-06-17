
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080 

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["nuget-local/", "nuget-local/"]

COPY ["src/ECommerceAuction.UserService.Api/ECommerceAuction.UserService.Api.csproj", "src/ECommerceAuction.UserService.Api/"]
COPY ["src/ECommerceAuction.UserService.Application/ECommerceAuction.UserService.Application.csproj", "src/ECommerceAuction.UserService.Application/"]
COPY ["src/ECommerceAuction.UserService.Domain/ECommerceAuction.UserService.Domain.csproj", "src/ECommerceAuction.UserService.Domain/"]
COPY ["src/ECommerceAuction.UserService.Infrastructure/ECommerceAuction.UserService.Infrastructure.csproj", "src/ECommerceAuction.UserService.Infrastructure/"]
COPY ["src/ECommerceAuction.UserService.Persistence/ECommerceAuction.UserService.Persistence.csproj", "src/ECommerceAuction.UserService.Persistence/"]

RUN dotnet restore "src/ECommerceAuction.UserService.Api/ECommerceAuction.UserService.Api.csproj"

COPY . .
WORKDIR "/src/src/ECommerceAuction.UserService.Api"

RUN dotnet build "ECommerceAuction.UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ECommerceAuction.UserService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "ECommerceAuction.UserService.Api.dll"]