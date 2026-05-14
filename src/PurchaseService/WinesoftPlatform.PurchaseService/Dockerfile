FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/PurchaseService/WinesoftPlatform.PurchaseService/WinesoftPlatform.PurchaseService.csproj", "src/PurchaseService/WinesoftPlatform.PurchaseService/"]
COPY ["src/Shared/WinesoftPlatform.Shared/WinesoftPlatform.Shared.csproj", "src/Shared/WinesoftPlatform.Shared/"]
RUN dotnet restore "src/PurchaseService/WinesoftPlatform.PurchaseService/WinesoftPlatform.PurchaseService.csproj"
COPY . .
WORKDIR "/src/src/PurchaseService/WinesoftPlatform.PurchaseService"
RUN dotnet build "WinesoftPlatform.PurchaseService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WinesoftPlatform.PurchaseService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WinesoftPlatform.PurchaseService.dll"]
