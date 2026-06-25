FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/IoTSimulatorService/WinesoftPlatform.IoTSimulatorService/WinesoftPlatform.IoTSimulatorService.csproj", "src/IoTSimulatorService/WinesoftPlatform.IoTSimulatorService/"]
RUN dotnet restore "src/IoTSimulatorService/WinesoftPlatform.IoTSimulatorService/WinesoftPlatform.IoTSimulatorService.csproj"
COPY . .
WORKDIR "/src/src/IoTSimulatorService/WinesoftPlatform.IoTSimulatorService"
RUN dotnet build "WinesoftPlatform.IoTSimulatorService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WinesoftPlatform.IoTSimulatorService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WinesoftPlatform.IoTSimulatorService.dll"]
