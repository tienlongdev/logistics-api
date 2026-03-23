FROM mcr.microsoft.com/dotnet/sdk:10.0 AS sdk
WORKDIR /src

COPY ["Logistics.Api.slnx", "Directory.Build.props", "Directory.Packages.props", "global.json", "./"]

COPY ["src/BuildingBlocks/Logistics.Api.BuildingBlocks.Application/Logistics.Api.BuildingBlocks.Application.csproj", "src/BuildingBlocks/Logistics.Api.BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/Logistics.Api.BuildingBlocks.Contracts/Logistics.Api.BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/Logistics.Api.BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/Logistics.Api.BuildingBlocks.Domain/Logistics.Api.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/Logistics.Api.BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/Logistics.Api.BuildingBlocks.Infrastructure/Logistics.Api.BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/Logistics.Api.BuildingBlocks.Infrastructure/"]
COPY ["src/BuildingBlocks/Logistics.Api.BuildingBlocks.Observability/Logistics.Api.BuildingBlocks.Observability.csproj", "src/BuildingBlocks/Logistics.Api.BuildingBlocks.Observability/"]

COPY ["src/Host/Logistics.Api.Host/Logistics.Api.Host.csproj", "src/Host/Logistics.Api.Host/"]

COPY ["src/Services/Hubs/Logistics.Api.Hubs.Application/Logistics.Api.Hubs.Application.csproj", "src/Services/Hubs/Logistics.Api.Hubs.Application/"]
COPY ["src/Services/Hubs/Logistics.Api.Hubs.Domain/Logistics.Api.Hubs.Domain.csproj", "src/Services/Hubs/Logistics.Api.Hubs.Domain/"]
COPY ["src/Services/Hubs/Logistics.Api.Hubs.Infrastructure/Logistics.Api.Hubs.Infrastructure.csproj", "src/Services/Hubs/Logistics.Api.Hubs.Infrastructure/"]
COPY ["src/Services/Identity/Logistics.Api.Identity.Application/Logistics.Api.Identity.Application.csproj", "src/Services/Identity/Logistics.Api.Identity.Application/"]
COPY ["src/Services/Identity/Logistics.Api.Identity.Domain/Logistics.Api.Identity.Domain.csproj", "src/Services/Identity/Logistics.Api.Identity.Domain/"]
COPY ["src/Services/Identity/Logistics.Api.Identity.Infrastructure/Logistics.Api.Identity.Infrastructure.csproj", "src/Services/Identity/Logistics.Api.Identity.Infrastructure/"]
COPY ["src/Services/Merchants/Logistics.Api.Merchants.Application/Logistics.Api.Merchants.Application.csproj", "src/Services/Merchants/Logistics.Api.Merchants.Application/"]
COPY ["src/Services/Merchants/Logistics.Api.Merchants.Domain/Logistics.Api.Merchants.Domain.csproj", "src/Services/Merchants/Logistics.Api.Merchants.Domain/"]
COPY ["src/Services/Merchants/Logistics.Api.Merchants.Infrastructure/Logistics.Api.Merchants.Infrastructure.csproj", "src/Services/Merchants/Logistics.Api.Merchants.Infrastructure/"]
COPY ["src/Services/Notifications/Logistics.Api.Notifications.Application/Logistics.Api.Notifications.Application.csproj", "src/Services/Notifications/Logistics.Api.Notifications.Application/"]
COPY ["src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/Logistics.Api.Notifications.Infrastructure.csproj", "src/Services/Notifications/Logistics.Api.Notifications.Infrastructure/"]
COPY ["src/Services/Pricing/Logistics.Api.Pricing.Application/Logistics.Api.Pricing.Application.csproj", "src/Services/Pricing/Logistics.Api.Pricing.Application/"]
COPY ["src/Services/Pricing/Logistics.Api.Pricing.Domain/Logistics.Api.Pricing.Domain.csproj", "src/Services/Pricing/Logistics.Api.Pricing.Domain/"]
COPY ["src/Services/Pricing/Logistics.Api.Pricing.Infrastructure/Logistics.Api.Pricing.Infrastructure.csproj", "src/Services/Pricing/Logistics.Api.Pricing.Infrastructure/"]
COPY ["src/Services/Reconciliation/Logistics.Api.Reconciliation.Application/Logistics.Api.Reconciliation.Application.csproj", "src/Services/Reconciliation/Logistics.Api.Reconciliation.Application/"]
COPY ["src/Services/Reconciliation/Logistics.Api.Reconciliation.Domain/Logistics.Api.Reconciliation.Domain.csproj", "src/Services/Reconciliation/Logistics.Api.Reconciliation.Domain/"]
COPY ["src/Services/Reconciliation/Logistics.Api.Reconciliation.Infrastructure/Logistics.Api.Reconciliation.Infrastructure.csproj", "src/Services/Reconciliation/Logistics.Api.Reconciliation.Infrastructure/"]
COPY ["src/Services/Search/Logistics.Api.Search.Application/Logistics.Api.Search.Application.csproj", "src/Services/Search/Logistics.Api.Search.Application/"]
COPY ["src/Services/Search/Logistics.Api.Search.Infrastructure/Logistics.Api.Search.Infrastructure.csproj", "src/Services/Search/Logistics.Api.Search.Infrastructure/"]
COPY ["src/Services/Shipments/Logistics.Api.Shipments.Application/Logistics.Api.Shipments.Application.csproj", "src/Services/Shipments/Logistics.Api.Shipments.Application/"]
COPY ["src/Services/Shipments/Logistics.Api.Shipments.Domain/Logistics.Api.Shipments.Domain.csproj", "src/Services/Shipments/Logistics.Api.Shipments.Domain/"]
COPY ["src/Services/Shipments/Logistics.Api.Shipments.Infrastructure/Logistics.Api.Shipments.Infrastructure.csproj", "src/Services/Shipments/Logistics.Api.Shipments.Infrastructure/"]
COPY ["src/Services/Tracking/Logistics.Api.Tracking.Application/Logistics.Api.Tracking.Application.csproj", "src/Services/Tracking/Logistics.Api.Tracking.Application/"]
COPY ["src/Services/Tracking/Logistics.Api.Tracking.Infrastructure/Logistics.Api.Tracking.Infrastructure.csproj", "src/Services/Tracking/Logistics.Api.Tracking.Infrastructure/"]

RUN dotnet restore "src/Host/Logistics.Api.Host/Logistics.Api.Host.csproj"

FROM sdk AS publish
WORKDIR /src
COPY . .
RUN dotnet publish "src/Host/Logistics.Api.Host/Logistics.Api.Host.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "Logistics.Api.Host.dll"]