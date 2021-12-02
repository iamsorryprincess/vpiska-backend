FROM mcr.microsoft.com/dotnet/runtime:5.0.12-focal-amd64 AS base
WORKDIR /app
EXPOSE 9090

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Vpiska.OrleansSilo/Vpiska.OrleansSilo.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Vpiska.OrleansSilo.dll"]