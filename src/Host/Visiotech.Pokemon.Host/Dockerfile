FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY global.json ./
COPY Visiotech.Pokemon.sln ./

COPY src/Api/Visiotech.Pokemon.Api/Visiotech.Pokemon.Api.csproj src/Api/Visiotech.Pokemon.Api/
COPY src/Api/Visiotech.Pokemon.Application/Visiotech.Pokemon.Application.csproj src/Api/Visiotech.Pokemon.Application/
COPY src/Api/Visiotech.Pokemon.Contracts/Visiotech.Pokemon.Contracts.csproj src/Api/Visiotech.Pokemon.Contracts/
COPY src/Api/Visiotech.Pokemon.Domain/Visiotech.Pokemon.Domain.csproj src/Api/Visiotech.Pokemon.Domain/
COPY src/Api/Visiotech.Pokemon.Infrastructure/Visiotech.Pokemon.Infrastructure.csproj src/Api/Visiotech.Pokemon.Infrastructure/
COPY src/Host/Visiotech.Pokemon.Host/Visiotech.Pokemon.Host.csproj src/Host/Visiotech.Pokemon.Host/

RUN dotnet restore src/Host/Visiotech.Pokemon.Host/Visiotech.Pokemon.Host.csproj

COPY src ./src

RUN dotnet publish src/Host/Visiotech.Pokemon.Host/Visiotech.Pokemon.Host.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Visiotech.Pokemon.Host.dll"]
