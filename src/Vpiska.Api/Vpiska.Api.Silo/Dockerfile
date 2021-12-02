FROM mcr.microsoft.com/dotnet/runtime:5.0.12-focal-amd64 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Vpiska.Api/Vpiska.Api.Silo/Vpiska.Api.Silo.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Vpiska.Api.Silo.dll"]