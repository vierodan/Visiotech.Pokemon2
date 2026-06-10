# Visiotech Pokemon API

Base tecnica inicial de una Web API en ASP.NET Core sobre `.NET 10`, preparada para evolucionar con una Clean Architecture estricta y un dominio modelado con DDD.

## Estructura

```txt
src/
  BuildingBlocks/
  Host/
    Visiotech.Pokemon.Host/
  Api/
    Visiotech.Pokemon.Api/
    Visiotech.Pokemon.Application/
    Visiotech.Pokemon.Contracts/
    Visiotech.Pokemon.Domain/
    Visiotech.Pokemon.Infrastructure/

tests/
  Visiotech.Pokemon.ArchitectureTests/
  Visiotech.Pokemon.UnitTests/
  Visiotech.Pokemon.IntegrationTests/
```

## Decisiones arquitectonicas

- `Domain` no depende de ninguna otra capa y contiene las abstracciones base, value objects y agregados.
- `Application` define los casos de uso, contratos y abstracciones de infraestructura.
- `Infrastructure` implementa dependencias externas y adaptadores concretos.
- `Api` queda limitada a composicion, HTTP, OpenAPI y mapeo de contratos.
- La documentacion OpenAPI se genera con `Microsoft.AspNetCore.OpenApi` y se visualiza con Scalar.
- La disciplina arquitectonica queda automatizada con `NetArchTest.Rules`.

## Primeros endpoints

- `GET /api/v1/system`
- `GET /api/v1/pokemons`

La exposicion HTTP usa Minimal API; no hay controladores MVC en la solucion.

## Validacion

```bash
dotnet restore Visiotech.Pokemon.sln
dotnet test Visiotech.Pokemon.sln
dotnet run --project src/Host/Visiotech.Pokemon.Host
```

En entorno de desarrollo, la referencia OpenAPI queda disponible en `/scalar`.
