FROM mcr.microsoft.com/dotnet/sdk:10.0.102 AS build
WORKDIR /source

COPY global.json ./
COPY src/SmartBuilding.Api/SmartBuilding.Api.csproj src/SmartBuilding.Api/
COPY src/SmartBuilding.Application/SmartBuilding.Application.csproj src/SmartBuilding.Application/
COPY src/SmartBuilding.Domain/SmartBuilding.Domain.csproj src/SmartBuilding.Domain/
COPY src/SmartBuilding.Infrastructure/SmartBuilding.Infrastructure.csproj src/SmartBuilding.Infrastructure/
RUN dotnet restore src/SmartBuilding.Api/SmartBuilding.Api.csproj

COPY src/ src/
RUN dotnet publish src/SmartBuilding.Api/SmartBuilding.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish ./
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "SmartBuilding.Api.dll"]
