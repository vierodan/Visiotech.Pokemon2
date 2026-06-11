# Componentes del backend

## 1. Propósito y alcance

Este documento describe la arquitectura backend real de `Visiotech.Pokemon` según el código actual del repositorio. No describe una arquitectura objetivo separada ni una propuesta pendiente: refleja las capas, dependencias, componentes de tiempo de ejecución, persistencia, contratos HTTP, observabilidad, seguridad técnica y estrategia de pruebas que existen hoy.

El backend está implementado como una Web API ASP.NET Core sobre `.NET 10`, con Minimal APIs, Clean Architecture, dominio modelado con DDD, Entity Framework Core y PostgreSQL como persistencia principal. Existe un proveedor `InMemory` solo para desarrollo local cuando se configura de forma explícita.

## 2. Vista general de arquitectura

La solución sigue una Clean Architecture estricta con estas capas:

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

La regla central es que las decisiones de framework, HTTP, EF Core, PostgreSQL, Serilog y configuración viven fuera del dominio. El dominio contiene invariantes y cálculos puros; la aplicación orquesta casos de uso; infraestructura implementa adaptadores; API traduce HTTP a comandos/queries y contratos; Host compone y arranca el proceso.

## 3. Proyectos y responsabilidades

| Proyecto | Responsabilidad actual |
| --- | --- |
| `src/Host/Visiotech.Pokemon.Host` | Composition root. Configura `WebApplication`, Serilog, carga `.env`, OpenAPI/Scalar, inicializador de base de datos y mapeo de endpoints. |
| `src/Api/Visiotech.Pokemon.Api` | Capa HTTP. Define Minimal APIs, manejo centralizado de excepciones, ProblemDetails y conversión de modelos de aplicación a contratos públicos. |
| `src/Api/Visiotech.Pokemon.Contracts` | DTOs públicos de petición/respuesta. No depende de dominio, aplicación, infraestructura, API ni Host. |
| `src/Api/Visiotech.Pokemon.Application` | Casos de uso. Define comandos, queries, handlers, modelos de respuesta, validación de entrada, abstracciones de persistencia, abstracciones de reloj/random y servicios de aplicación. |
| `src/Api/Visiotech.Pokemon.Domain` | Modelo de dominio. Agregados, entidades, value objects, excepciones de dominio, cálculo de daño y tabla normativa de efectividad. |
| `src/Api/Visiotech.Pokemon.Infrastructure` | Adaptadores concretos. EF Core, PostgreSQL, proveedor InMemory, repositorios, unidad de trabajo, migraciones, seed, reloj del sistema y random de daño. |
| `tests/Visiotech.Pokemon.ArchitectureTests` | Reglas automatizadas de dependencias y convenciones arquitectónicas con `NetArchTest.Rules`. |
| `tests/Visiotech.Pokemon.UnitTests` | Pruebas de dominio y aplicación con dependencias sustituidas. |
| `tests/Visiotech.Pokemon.IntegrationTests` | Pruebas extremo a extremo sobre `WebApplicationFactory` y PostgreSQL real cuando está disponible. |

## 4. Host y arranque

El proceso arranca en `Program.cs` del proyecto Host.

Responsabilidades de arranque:

- Carga variables desde `.env` buscando en directorios ancestros con `DotNetEnv`.
- Usa `Env.NoClobber()`, por lo que una variable de entorno real tiene prioridad sobre `.env`.
- Configura un bootstrap logger de Serilog a consola antes de construir la aplicación.
- Registra las capas con `AddApi()`, `AddApplication()` y `AddInfrastructure(configuration, environment)`.
- Configura OpenAPI y Scalar solo en `Development`.
- Ejecuta `DatabaseInitializer.InitializeAsync(...)` antes de aceptar peticiones HTTP.
- Mapea `GET /` como redirección a `/scalar`.
- Mapea todos los endpoints mediante `app.MapApi()`.

El host también configura `UseSerilogRequestLogging` con enriquecimiento de:

- `TraceId`
- host, esquema y protocolo de request
- nombre del endpoint
- presencia de query string
- nivel `Warning` para respuestas 4xx
- nivel `Error` para excepciones o 5xx

## 5. API HTTP

La API usa Minimal APIs. No hay controladores MVC. Cada grupo funcional tiene una clase de endpoints dedicada y los contratos HTTP viven en el proyecto `Contracts`.

Superficie HTTP actual:

| Método | Ruta | Caso de uso |
| --- | --- | --- |
| `GET` | `/api/v1/system` | Metadatos técnicos del servicio. |
| `POST` | `/api/v1/pokemons` | Crear especie base. |
| `PUT` | `/api/v1/pokemons/{id}` | Actualizar especie base. |
| `DELETE` | `/api/v1/pokemons/{id}` | Eliminar especie base si no tiene dependencias. |
| `GET` | `/api/v1/pokemons` | Consultar catálogo de especies con filtros y paginación. |
| `GET` | `/api/v1/pokemons/{id}` | Consultar detalle de especie. |
| `PUT` | `/api/v1/pokemons/{id}/learnable-moves` | Asociar o retirar movimientos aprendibles. |
| `GET` | `/api/v1/pokemons/{id}/learnable-moves` | Consultar movimientos aprendibles de una especie. |
| `POST` | `/api/v1/moves` | Crear movimiento. |
| `PUT` | `/api/v1/moves/{id}` | Actualizar movimiento. |
| `DELETE` | `/api/v1/moves/{id}` | Eliminar movimiento si no tiene dependencias. |
| `GET` | `/api/v1/moves` | Consultar catálogo de movimientos con filtros y paginación. |
| `GET` | `/api/v1/moves/{id}` | Consultar detalle de movimiento. |
| `GET` | `/api/v1/moves/{id}/pokemon-species` | Consultar especies que pueden aprender un movimiento. |
| `POST` | `/api/v1/my-pokemons` | Crear instancia jugable. |
| `PUT` | `/api/v1/my-pokemons/{id}` | Actualizar instancia jugable. |
| `DELETE` | `/api/v1/my-pokemons/{id}` | Eliminar instancia jugable si no participa en dependencias activas. |
| `GET` | `/api/v1/my-pokemons` | Consultar catálogo de instancias jugables. |
| `GET` | `/api/v1/my-pokemons/{id}` | Consultar detalle de instancia jugable. |
| `GET` | `/api/v1/my-pokemons/{id}/equipped-moves` | Consultar movimientos equipados de una instancia. |
| `POST` | `/api/v1/damage-calculations` | Calcular daño hipotético de un movimiento. |
| `POST` | `/api/v1/battles` | Crear partida entre dos instancias jugables. |
| `GET` | `/api/v1/battles/{id}` | Consultar estado actual de una partida. |
| `POST` | `/api/v1/battles/{id}/phases` | Ejecutar una fase de combate. |
| `GET` | `/api/v1/battles/{id}/phases` | Consultar histórico ordenado de fases. |

La API no accede directamente a EF Core ni a infraestructura. Inyecta `ICommandHandler<,>` o `IQueryHandler<,>` y delega el caso de uso en la capa de aplicación.

## 6. Contratos y errores

Los contratos públicos son records inmutables terminados en `Contract`. Esta convención está protegida por tests de arquitectura.

El manejo de errores se centraliza en `ApiExceptionHandler`:

| Excepción | HTTP | Respuesta |
| --- | --- | --- |
| `ApplicationValidationException` | `400 Bad Request` | `HttpValidationProblemDetails` con diccionario explícito de errores por campo. |
| `ApplicationConflictException` | `409 Conflict` | `ProblemDetails` con `target` cuando aplica. |
| `ApplicationNotFoundException` | `404 Not Found` | `ProblemDetails` con `target` cuando aplica. |
| `ArgumentException` | `400 Bad Request` | `ProblemDetails`. |
| Resto de excepciones | `500 Internal Server Error` | `ProblemDetails`. |

Los errores funcionales esperados se expresan desde aplicación como validación, conflicto o no encontrado. Las excepciones de dominio no se filtran directamente al borde HTTP: los handlers las capturan cuando procede y las traducen a errores de validación explícitos.

## 7. Capa de aplicación

La capa de aplicación implementa los casos de uso con comandos y queries propios:

- `ICommand<TResponse>` y `ICommandHandler<TCommand,TResponse>`.
- `IQuery<TResponse>` y `IQueryHandler<TQuery,TResponse>`.

Los handlers se registran manualmente en DI y se envuelven con decoradores:

- `LoggingCommandHandler<TCommand,TResponse>`
- `LoggingQueryHandler<TQuery,TResponse>`

Estos decoradores registran payload resumido, duración, resultado resumido y errores esperados sin acoplar los casos de uso a HTTP.

Familias de casos de uso actuales:

- `System`: información técnica del servicio.
- `Pokemons`: CRUD de especies base y movimientos aprendibles.
- `Moves`: CRUD de movimientos y consulta inversa de especies que los aprenden.
- `MyPokemons`: CRUD de instancias jugables y movimientos equipados.
- `Damage`: cálculo de daño reutilizable.
- `Battles`: creación, consulta de estado, ejecución de fases, KO automático e histórico.

La aplicación define las abstracciones de persistencia:

- repositorios de lectura/escritura para especies, movimientos, instancias jugables y partidas
- comprobadores de dependencias para borrados seguros
- `IUnitOfWork`

La aplicación también define abstracciones técnicas que deben ser sustituibles:

- `IClock`
- `IDamageRandomProvider`
- `IMoveDamageCalculationService`

## 8. Dominio

El dominio no depende de ninguna otra capa. Contiene las invariantes principales y usa `DomainException` para violaciones de reglas.

Agregados y entidades principales:

- `PokemonSpecies`: especie base con nombre normalizado, tipos, estadísticas base y movimientos aprendibles.
- `PokemonMove`: movimiento con nombre, tipo, categoría y potencia.
- `MyPokemon`: instancia jugable con especie asociada, nivel, PS y slots de movimientos equipados.
- `Battle`: partida de combate con dos combatientes, turno, estado, ganador/perdedor e histórico de fases.
- `BattleCombatant`: snapshot mutable de PS dentro de una partida.
- `BattlePhase`: fase registrada con atacante, defensor, movimiento, random, efectividad, daño y PS resultantes.
- `BattlePhaseEffectiveness`: detalle por tipo defensor del multiplicador usado.

Value objects principales:

- `Name`: trim, longitud máxima 100 y normalización a mayúsculas invariantes.
- `Level`: rango `1..100`.
- `BaseStats`: todas las estadísticas deben ser positivas.
- `Move`: valida potencia según categoría.
- `PokemonTyping`: uno o dos tipos, sin duplicados.
- `Ability`: value object preparado, aunque no participa en persistencia actual.

Reglas de instancia jugable:

- `MyPokemon` referencia una especie base mediante identificador estable.
- `CurrentHealthPoints` no puede ser negativo.
- `TotalHealthPoints` debe ser positivo.
- `CurrentHealthPoints` no puede superar `TotalHealthPoints`.
- Debe equipar entre 1 y 4 movimientos.
- No puede equipar ids vacíos ni repetidos.
- La validación de que los movimientos existen y son aprendibles se realiza en aplicación, cruzando especie y catálogo.

Reglas de combate:

- Una partida se crea con exactamente dos ids distintos de `MyPokemon`.
- Ambos combatientes deben empezar con PS actual mayor que `0`.
- La partida arranca en estado `Created`, turno `1` y próximo atacante igual al primer combatiente.
- No se pueden registrar fases en partidas `Finished`.
- El atacante de una fase debe pertenecer a la partida y coincidir con `NextAttackerMyPokemonId`.
- Si el defensor queda a `0` PS, la partida debe finalizar.
- Al finalizar, `WinnerMyPokemonId` y `LoserMyPokemonId` se informan y `NextAttackerMyPokemonId` queda en `null`.

## 9. Cálculo de daño y efectividad

El cálculo de daño está centralizado y es reutilizable:

- `MoveDamageCalculationService` orquesta carga de atacante, defensor, movimiento y especies.
- `MoveDamageCalculator` ejecuta el cálculo puro en dominio.
- `PokemonTypeEffectivenessChart` materializa la tabla normativa de efectividad como contrato explícito de código.

Decisiones actuales del cálculo:

- Solo movimientos `Physical` y `Special` calculan daño; `Status` se rechaza.
- `Physical` usa `Attack` del atacante y `Defense` del defensor.
- `Special` usa `SpecialAttack` del atacante y `SpecialDefense` del defensor.
- El defensor puede tener uno o dos tipos.
- La efectividad total es el producto de los multiplicadores por tipo defensor.
- El random se obtiene de `IDamageRandomProvider` en rango inclusivo `85..100`.
- El daño bruto usa `Floor`.
- El daño aplicado nunca es negativo y se limita a los PS actuales del defensor.
- La respuesta conserva trazabilidad: estadísticas usadas, random, daño base, desglose de efectividad, efectividad total, daño bruto, daño aplicado y PS restantes.

El flujo de combate invoca este mismo servicio. No duplica la fórmula ni la tabla de tipos.

## 10. Persistencia

La persistencia se implementa con Entity Framework Core en `PokemonDbContext`.

Proveedor principal:

- `Postgres` mediante `Npgsql.EntityFrameworkCore.PostgreSQL`.

Proveedor alternativo:

- `InMemory` mediante `Microsoft.EntityFrameworkCore.InMemory`.
- Solo se permite si `ASPNETCORE_ENVIRONMENT` es `Development`.
- Si se configura `InMemory` fuera de `Development`, el arranque falla con `InvalidOperationException`.

Esquema por defecto:

- `pokemon2`

Tabla de historial de migraciones:

- `pokemon2.__EFMigrationsHistory`

Tablas actuales:

- `pokemon_species`
- `pokemon_moves`
- `pokemon_species_learnable_moves`
- `my_pokemons`
- `my_pokemon_move_slots`
- `battles`
- `battle_combatants`
- `battle_phases`
- `battle_phase_effectiveness`

Migraciones actuales:

- `20260610120000_InitialPokemonSpeciesCatalog`
- `20260610183000_AddPokemonMovesCatalog`
- `20260610213000_AddPokemonSpeciesLearnableMoves`
- `20260610233000_AddMyPokemons`
- `20260611113000_AddBattles`
- `20260611131500_AddBattleHistory`
- `20260611153000_AddBattleOutcome`

El modelo EF refuerza reglas con constraints e índices:

- nombres normalizados únicos para especies y movimientos
- tipos secundarios distintos del primario
- estadísticas base positivas
- potencia coherente con categoría de movimiento
- PS no negativos y dentro de rango
- slots de movimientos entre `1..4`
- combatientes de batalla en slots `1..2`
- random de fase entre `85..100`
- daño no negativo
- consistencia de estado de batalla finalizada frente a ganador/perdedor/próximo atacante
- claves compuestas para slots, combatientes, fases y desglose de efectividad

Relaciones relevantes:

- `MyPokemon -> PokemonSpecies` con `DeleteBehavior.Restrict`.
- `MyPokemonMoveSlot -> MyPokemon` con cascade.
- `MyPokemonMoveSlot -> PokemonMove` con restrict.
- `PokemonLearnableMove` une especies y movimientos con restrict.
- `BattleCombatant -> Battle` con cascade.
- `BattleCombatant -> MyPokemon` con restrict.
- `BattlePhase -> Battle` con cascade.
- `BattlePhaseEffectiveness -> BattlePhase` con cascade.

## 11. Inicialización de base de datos

`DatabaseInitializer` se ejecuta durante el arranque del Host.

Comportamiento según proveedor:

- Con `Postgres`: ejecuta `dbContext.Database.MigrateAsync(...)`.
- Con `InMemory`: ejecuta `dbContext.Database.EnsureCreatedAsync(...)`.

El seed se controla con:

- `Seed:ApplyMvpRoster`
- Variable equivalente: `Seed__ApplyMvpRoster`

Si el seed está activo:

- inserta especies MVP si el catálogo de especies está vacío
- inserta movimientos MVP si el catálogo de movimientos está vacío
- inserta movimientos aprendibles MVP si esa tabla está vacía

Por defecto en `appsettings.json` y `appsettings.Development.json`, `ApplyMvpRoster` está en `false`.

## 12. Repositorios, unidad de trabajo y borrados seguros

Infraestructura implementa los repositorios definidos en aplicación:

- `PokemonSpeciesRepository`
- `PokemonMoveRepository`
- `MyPokemonRepository`
- `BattleRepository`

Los métodos de lectura usan `AsNoTracking()` cuando no van a mutar estado. Los métodos `GetForUpdateAsync` cargan entidades trackeadas e incluyen colecciones necesarias para aplicar cambios en el agregado.

`EntityFrameworkUnitOfWork` centraliza `SaveChangesAsync`. Actualmente traduce violaciones únicas de PostgreSQL a `ApplicationConflictException`.

Los borrados usan comprobadores de dependencias antes de eliminar:

- `PokemonSpeciesDeletionDependencyChecker`
- `PokemonMoveDeletionDependencyChecker`
- `MyPokemonDeletionDependencyChecker`

Estos comprobadores inspeccionan metadatos EF y consultan referencias reales en la base. En `MyPokemon`, además se bloquea explícitamente el borrado si participa en una batalla activa (`Created` o `InProgress`). Esto protege la integridad entre instancias jugables y combate.

## 13. Configuración

Configuración funcional actual:

| Clave | Uso |
| --- | --- |
| `ConnectionStrings:Pokemon2Db` / `ConnectionStrings__Pokemon2Db` | Cadena de conexión PostgreSQL. Obligatoria con proveedor `Postgres`. |
| `Persistence:Provider` / `Persistence__Provider` | `Postgres` o `InMemory`. Si falta, se asume `Postgres`. |
| `Persistence:InMemoryDatabaseName` / `Persistence__InMemoryDatabaseName` | Nombre de base InMemory en Development. |
| `Observability:SeqUrl` / `Observability__SeqUrl` | URL de Seq. Si está vacía, no se configura sink Seq. |
| `Seed:ApplyMvpRoster` / `Seed__ApplyMvpRoster` | Activa seed MVP durante inicialización. |
| `ASPNETCORE_ENVIRONMENT` | Controla entorno; `InMemory` solo es válido en `Development`. |
| `ASPNETCORE_URLS` | URLs de escucha del Host. |

El fichero `.env` de raíz puede definir variables, pero no sobrescribe variables ya existentes del proceso.

Variables usadas por tests de integración:

- `IntegrationTests__Pokemon2Db`
- `ConnectionStrings__Pokemon2Db`
- `POSTGRES_HOST`
- `POSTGRES_PORT`
- `POSTGRES_DATABASE`
- `POSTGRES_USERNAME`
- `POSTGRES_PASSWORD`
- `POSTGRES_POOLING`
- `POSTGRES_MIN_POOL_SIZE`
- `POSTGRES_MAX_POOL_SIZE`

## 14. Observabilidad

Serilog es el proveedor de logging estructurado.

Sinks actuales:

- consola siempre
- Seq si `Observability:SeqUrl` tiene valor

La aplicación registra:

- inicio del host y entorno
- si Seq está configurado
- inicialización de persistencia, proveedor EF y estado de seed
- peticiones HTTP con duración y endpoint
- comandos y queries con payload resumido
- errores de validación, conflictos y no encontrados como warnings
- excepciones no esperadas como errores

La estrategia actual evita registrar connection strings o secretos. Los payloads de comandos/queries contienen identificadores y datos funcionales no secretos.

## 15. Componentes de ejecución local

El repositorio contiene `docker-compose.yml` para dependencias de soporte:

- `visiotech-postgres`: PostgreSQL 17 con healthcheck `pg_isready`.
- `visiotech-seq`: Seq para logs estructurados.
- red Docker `visiotech`.

El compose actual no levanta la API como servicio. La API se ejecuta normalmente con:

```bash
dotnet run --project src/Host/Visiotech.Pokemon.Host
```

También existe `src/Host/Visiotech.Pokemon.Host/Dockerfile`, por lo que la API puede dockerizarse, pero ese servicio no está declarado actualmente en `docker-compose.yml`.

Implicación operativa:

- PostgreSQL y Seq pueden levantarse por Docker.
- La API puede correr en local contra esos servicios.
- Si se quiere ejecutar todo como stack Docker, hay que añadir explícitamente un servicio de API al compose.

## 16. Estrategia de pruebas

La solución tiene tres niveles de pruebas.

Arquitectura:

- Valida que `Domain` no dependa de capas externas.
- Valida que `Application` no dependa de `Contracts`, `Infrastructure`, `Api` ni `Host`.
- Valida que `Infrastructure` no dependa de `Contracts`, `Api` ni `Host`.
- Valida que endpoints de API no dependan de infraestructura.
- Valida que las abstracciones de persistencia sean interfaces.
- Valida que `Contracts` no dependan de capas internas.
- Valida convenciones de nombres de contratos.
- Valida value objects del dominio.

Unitarias:

- Cubren reglas de dominio.
- Cubren handlers de aplicación.
- Usan sustitutos para repositorios, random, servicios y unidad de trabajo cuando aplica.

Integración:

- Usan `WebApplicationFactory<Program>`.
- Usan PostgreSQL real, no EF InMemory.
- Crean o reutilizan una base `*_integration_tests`.
- Restablecen el esquema `pokemon2` antes de aplicar migraciones.
- Se saltan automáticamente con `PostgresFactAttribute` si PostgreSQL no está disponible o accesible.
- Incluyen tests específicos de arranque con `Persistence:Provider=InMemory` en `Development` y rechazo en `Production`.

Esta separación es intencionada: `InMemory` ayuda al arranque local de desarrollo, pero no sustituye las pruebas de integración sobre PostgreSQL, porque no reproduce constraints, SQL, migraciones ni comportamiento real del proveedor.

## 17. Seguridad e integridad

Decisiones de seguridad técnica existentes:

- Validación explícita en aplicación antes de ejecutar casos de uso.
- Errores funcionales con campos concretos para evitar fallos ambiguos.
- Identificadores estables `Guid` generados por la aplicación/dominio, no por la base.
- Constraints de base de datos como defensa adicional ante escrituras inválidas.
- `DeleteBehavior.Restrict` en relaciones críticas para evitar borrados accidentales de catálogo o instancias en uso.
- Borrado de `MyPokemon` bloqueado si participa en batallas activas.
- Estado de batalla finalizada consistente por dominio y por check constraints.
- Cálculo de daño centralizado para evitar divergencia entre consulta y combate.
- Tabla normativa de efectividad materializada en código y cubierta por pruebas.
- `InMemory` prohibido fuera de `Development`.
- `.env` no pisa variables de entorno reales.

Limitaciones actuales que deben tratarse como decisiones conscientes:

- No hay autenticación ni autorización implementadas en la API.
- No hay rate limiting.
- No hay health checks HTTP dedicados.
- No hay pipeline separado de migraciones; las migraciones se aplican en arranque del Host.
- No hay cifrado ni gestión de secretos dentro del código; se espera inyección por variables de entorno o entorno de despliegue.

## 18. Criterios para evolucionar

Evoluciones razonables sin romper la arquitectura actual:

- Añadir autenticación/autorización en API sin tocar dominio.
- Añadir health checks de PostgreSQL y Seq.
- Separar migraciones a un job/pipeline de despliegue en entornos productivos.
- Añadir un servicio `api` al `docker-compose.yml` si se quiere stack completo local.
- Introducir transacciones explícitas si aparecen casos de uso con múltiples unidades de persistencia externas.
- Añadir paginación/filtros adicionales en repositorios manteniendo contratos estables.
- Publicar eventos de dominio o integración si el combate empieza a alimentar otros bounded contexts.

## 19. Resumen ejecutivo

El backend actual es una API .NET 10 con Clean Architecture estricta. El dominio contiene las reglas de Pokémon, instancias jugables, daño y combate. La aplicación expone casos de uso mediante comandos y queries decorados con logging. La API solo adapta HTTP y contratos. Infraestructura implementa EF Core con PostgreSQL, migraciones, repositorios, seed y un proveedor InMemory limitado a Development.

La persistencia real es PostgreSQL sobre el esquema `pokemon2`; los tests de integración también usan PostgreSQL real y se saltan si no hay base disponible. La ejecución local se apoya en Docker Compose para PostgreSQL y Seq, mientras la API se arranca desde el proyecto Host salvo que se añada explícitamente al compose.
