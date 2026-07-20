
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# Nexus.Ocr reads identity cards through TesseractOCR, which P/Invokes native Tesseract. That
# package ships Windows DLLs only — no runtimes/linux assets at all — so on Linux the natives have
# to come from the distro, and libdl has to be reachable under its bare name: InteropDotNet calls
# dlopen through DllImport("libdl"), which glibc 2.34+ folded into libc, leaving only libdl.so.2.
# libfontconfig1 is SkiaSharp's.
#
# Without this the service starts, passes health checks and serves every other endpoint, then throws
# DllNotFoundException the first time a user submits a card. Nothing at build time catches it.
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libtesseract5 \
        liblept5 \
        libfontconfig1 \
    && rm -rf /var/lib/apt/lists/* \
    && ln -sf /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so

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

# TesseractOCR's loader looks beside the assembly, under x64/, for the exact filename its Windows
# DLL would carry — never for the SONAME — so it cannot find the distro's liblept.so.5 on its own.
# The names claim leptonica 1.85 and tesseract 5.5 while Ubuntu 24.04 ships 1.82 and 5.3; they are
# close enough to read a card today, but a base image bump is the likeliest thing to break OCR, and
# it will break at runtime rather than in the build.
USER root
RUN mkdir -p /app/x64 \
    && ln -sf /lib/x86_64-linux-gnu/liblept.so.5 /app/x64/libleptonica-1.85.0.dll.so \
    && ln -sf /lib/x86_64-linux-gnu/libtesseract.so.5 /app/x64/libtesseract55.dll.so \
    # The apt install above runs as root, so /app lands root-owned; the app user still has to
    # create the Local storage provider's uploads folder here at startup.
    && chown -R app:app /app
USER app

ENTRYPOINT ["dotnet", "ECommerceAuction.UserService.Api.dll"]