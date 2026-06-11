# Visiotech Pokémon API

## Introducción general

`Visiotech Pokémon API` es una Web API desarrollada en `.NET 10` para modelar un MVP de Pokédex y combate Pokémon.

El proyecto implementa:

- catálogo de especies Pokémon base
- catálogo de movimientos
- relaciones de movimientos aprendibles por especie
- gestión de instancias jugables de `Mi Pokémon`
- cálculo de daño usando la fórmula del requisito original
- tabla normativa de efectividad por tipos
- creación y ejecución de partidas de combate
- histórico trazable de fases de combate

La solución sigue una arquitectura limpia con separación explícita entre `Host`, `Api`, `Contracts`, `Application`, `Domain` e `Infrastructure`. La persistencia principal se realiza con Entity Framework Core sobre PostgreSQL, y existe soporte explícito para EF Core InMemory solo en desarrollo local.

## Requisitos

Para trabajar con el proyecto se necesita:

- `.NET SDK 10.0.107` o compatible según `global.json`
- Docker y Docker Compose, si se quieren levantar PostgreSQL y Seq localmente
- PostgreSQL, si se ejecuta con `Persistence__Provider=Postgres`

La solución principal es:

```bash
Visiotech.Pokemon.sln
```

El proyecto de arranque de la API es:

```bash
src/Host/Visiotech.Pokemon.Host/Visiotech.Pokemon.Host.csproj
```

## Modos de arranque y puesta en marcha de la API

### 1. Arranque local con PostgreSQL

Este es el modo recomendado para validar el comportamiento real de persistencia, migraciones y repositorios.

Primero, crea tu fichero local de variables:

```bash
cp .env.example .env
```

Después, levanta las dependencias:

```bash
docker compose up -d
```

Y arranca la API:

```bash
dotnet run --project src/Host/Visiotech.Pokemon.Host
```

En `Development`, la documentación interactiva queda disponible en:

```txt
/scalar
```

El host redirige `GET /` a `/scalar`.

### 2. Arranque local con InMemory

Este modo permite arrancar la API sin PostgreSQL, útil para desarrollo rápido o demos locales.

Debe configurarse:

```txt
ASPNETCORE_ENVIRONMENT=Development
Persistence__Provider=InMemory
```

La persistencia InMemory está bloqueada fuera de `Development`. Si se intenta usar en `Production`, `Staging` u otro entorno, la API falla durante el arranque de forma deliberada.

### 3. Arranque con Docker Compose

El `docker-compose.yml` actual levanta dependencias de infraestructura:

- `visiotech-postgres`: PostgreSQL 17
- `visiotech-seq`: Seq para logs estructurados

Comando principal:

```bash
docker compose up -d
```

El compose actual no incluye el contenedor de la API. La API se ejecuta normalmente desde el host con `dotnet run`, conectándose a PostgreSQL y Seq publicados por Docker.

### 4. Compilación y pruebas

Compilar la solución:

```bash
dotnet build Visiotech.Pokemon.sln
```

Ejecutar pruebas:

```bash
dotnet test Visiotech.Pokemon.sln
```

Las pruebas de integración usan PostgreSQL real cuando está disponible. Si no hay base de datos accesible, están preparadas para saltarse en lugar de romper el flujo completo de pruebas.

## Configuración de la API

La API carga variables de entorno desde `.env` usando `DotNetEnv`.

Regla importante:

- las variables reales del sistema tienen prioridad sobre `.env`
- el proyecto usa `Env.NoClobber()`, por lo que `.env` no sobrescribe valores ya definidos

Variables especialmente relevantes:

- `ASPNETCORE_ENVIRONMENT`
- `DOTNET_ENVIRONMENT`
- `Persistence__Provider`
- `Persistence__InMemoryDatabaseName`
- `ConnectionStrings__Pokemon2Db`
- `Observability__SeqUrl`
- `Seed__ApplyMvpRoster`

La documentación detallada de cada variable, valores válidos, valores no recomendados y diferencias entre API, Docker Compose y tests está en:

[docs/configuration/environment-variables.md](docs/configuration/environment-variables.md)

## Resumen de arquitectura

La solución aplica Clean Architecture:

```txt
Host
  -> Api
  -> Application
  -> Infrastructure

Api
  -> Application
  -> Contracts

Infrastructure
  -> Application
  -> Domain

Application
  -> Domain

Contracts
  -> sin dependencias internas

Domain
  -> sin dependencias internas
```

Responsabilidades principales:

- `Host`: composición, configuración, logging, arranque e inicialización de base de datos
- `Api`: endpoints HTTP, contratos de entrada/salida y traducción a comandos o queries
- `Contracts`: DTOs públicos de la API
- `Application`: casos de uso, validación funcional, handlers y abstracciones
- `Domain`: agregados, entidades, value objects, invariantes y cálculo de daño
- `Infrastructure`: EF Core, PostgreSQL, InMemory, migraciones, repositorios, seed y servicios técnicos

Para profundizar en la arquitectura backend:

[docs/architecture/backend.md](docs/architecture/backend.md)

## Resumen de dockerización

El proyecto incluye:

- `docker-compose.yml` en la raíz
- `Dockerfile` para la API en `src/Host/Visiotech.Pokemon.Host/Dockerfile`

El estado actual es:

- Docker Compose levanta PostgreSQL y Seq
- la API no está incluida como servicio en el compose actual
- PostgreSQL se usa como persistencia principal
- Seq se usa para observabilidad local
- el Dockerfile de la API usa build multi-stage con imágenes oficiales de .NET 10

Punto importante de red:

- si la API se ejecuta desde el host, PostgreSQL debe apuntar normalmente a `localhost`
- si la API se ejecuta dentro de Docker, PostgreSQL debe apuntar al servicio `visiotech-postgres`

Para ver el análisis completo de Dockerfile, Docker Compose, redes, puertos, variables y limitaciones actuales:

[docs/configuration/dockerization.md](docs/configuration/dockerization.md)

## Resumen de dominio

El dominio modela explícitamente la diferencia entre catálogo base y estado jugable.

Conceptos principales:

- `PokemonSpecies`: especie base del catálogo
- `PokemonMove`: movimiento disponible
- `PokemonLearnableMove`: relación de aprendizaje entre especie y movimiento
- `MyPokemon`: instancia jugable basada en una especie
- `Battle`: partida entre dos instancias jugables
- `BattleCombatant`: snapshot mutable de PS dentro de una partida
- `BattlePhase`: fase registrada de combate
- `PokemonTypeEffectivenessChart`: tabla normativa de efectividad
- `MoveDamageCalculator`: cálculo puro de daño

El modelo evita mezclar datos estáticos de catálogo con estado mutable de combate. Esto permite actualizar instancias jugables y partidas sin corromper especies base ni movimientos del catálogo.

Para ver el análisis completo del dominio y su justificación técnica:

[docs/architecture/domain.md](docs/architecture/domain.md)

## Resumen de funcionalidad

La API cubre los casos de uso funcionales `UC-01` a `UC-22`.

Bloques funcionales:

- CRUD de especies Pokémon base
- CRUD de movimientos
- asociación y consulta de movimientos aprendibles
- consulta de especies que pueden aprender un movimiento
- creación, consulta, actualización y eliminación de `Mis Pokémon`
- consulta de movimientos equipados
- cálculo trazable de daño
- creación de partidas
- consulta de estado de partida
- ejecución de fases de combate
- finalización automática por KO
- consulta del histórico de fases

Reglas relevantes:

- una especie tiene entre uno y dos tipos
- un movimiento puede ser `Physical`, `Special` o `Status`
- un `Mi Pokémon` equipa entre uno y cuatro movimientos
- los movimientos equipados deben ser aprendibles por la especie
- el daño usa la tabla normativa de efectividad
- el defensor puede tener uno o dos tipos
- una partida enfrenta exactamente dos `Mis Pokémon`
- una partida finalizada no acepta nuevas fases

Para ver casos de uso, reglas de negocio, escenarios mínimos, contratos HTTP y trazabilidad funcional:

[docs/functionality/use-cases.md](docs/functionality/use-cases.md)

## Estructura del repositorio

```txt
.
├── src
│   ├── Api
│   │   ├── Visiotech.Pokemon.Api
│   │   ├── Visiotech.Pokemon.Application
│   │   ├── Visiotech.Pokemon.Contracts
│   │   ├── Visiotech.Pokemon.Domain
│   │   └── Visiotech.Pokemon.Infrastructure
│   └── Host
│       └── Visiotech.Pokemon.Host
├── tests
│   ├── Visiotech.Pokemon.ArchitectureTests
│   ├── Visiotech.Pokemon.IntegrationTests
│   └── Visiotech.Pokemon.UnitTests
├── docs
│   ├── architecture
│   ├── configuration
│   └── functionality
├── docker-compose.yml
├── .env.example
├── global.json
└── Visiotech.Pokemon.sln
```

## Documentación relacionada

- [Arquitectura backend](docs/architecture/backend.md)
- [Dominio](docs/architecture/domain.md)
- [Variables de entorno](docs/configuration/environment-variables.md)
- [Dockerización](docs/configuration/dockerization.md)
- [Casos de uso funcionales](docs/functionality/use-cases.md)

