FROM mcr.microsoft.com/dotnet/sdk:9.0 AS builder
WORKDIR /app
COPY WinesoftPlatform.API/*.csproj WinesoftPlatform.API/
RUN dotnet restore ./WinesoftPlatform.API
COPY . .
RUN dotnet publish ./WinesoftPlatform.API -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=builder /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "WinesoftPlatform.API.dll"]