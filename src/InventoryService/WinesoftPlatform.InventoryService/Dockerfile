FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/InventoryService/WinesoftPlatform.InventoryService/WinesoftPlatform.InventoryService.csproj", "src/InventoryService/WinesoftPlatform.InventoryService/"]
COPY ["src/Shared/WinesoftPlatform.Shared/WinesoftPlatform.Shared.csproj", "src/Shared/WinesoftPlatform.Shared/"]
RUN dotnet restore "src/InventoryService/WinesoftPlatform.InventoryService/WinesoftPlatform.InventoryService.csproj"
COPY . .
WORKDIR "/src/src/InventoryService/WinesoftPlatform.InventoryService"
RUN dotnet build "WinesoftPlatform.InventoryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WinesoftPlatform.InventoryService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WinesoftPlatform.InventoryService.dll"]
