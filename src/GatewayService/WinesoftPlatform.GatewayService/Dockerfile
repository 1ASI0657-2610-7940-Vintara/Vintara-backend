FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/GatewayService/WinesoftPlatform.GatewayService/WinesoftPlatform.GatewayService.csproj", "src/GatewayService/WinesoftPlatform.GatewayService/"]
COPY ["src/Shared/WinesoftPlatform.Shared/WinesoftPlatform.Shared.csproj", "src/Shared/WinesoftPlatform.Shared/"]
RUN dotnet restore "src/GatewayService/WinesoftPlatform.GatewayService/WinesoftPlatform.GatewayService.csproj"
COPY . .
WORKDIR "/src/src/GatewayService/WinesoftPlatform.GatewayService"
RUN dotnet build "WinesoftPlatform.GatewayService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WinesoftPlatform.GatewayService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WinesoftPlatform.GatewayService.dll"]
