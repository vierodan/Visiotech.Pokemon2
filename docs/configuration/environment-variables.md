# Variables de entorno

## 1. Propósito

Este documento explica el fichero `.env.example`, qué hace cada variable, qué parte del sistema la consume, qué valores acepta y qué valores deben evitarse.

El proyecto usa configuración estándar de ASP.NET Core y carga variables desde un fichero `.env` mediante `DotNetEnv` antes de construir el `WebApplicationBuilder`.

Regla importante:

- Las variables de entorno reales del proceso tienen prioridad sobre el fichero `.env`.
- El Host usa `Env.NoClobber()`, por lo que no sobrescribe una variable que ya exista en el sistema operativo, en Docker, en el IDE o en el comando de arranque.

## 2. Fichero `.env.example`

El fichero actual contiene:

```txt
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development

# PERSISTENCE
# Allowed values: Postgres | InMemory
Persistence__Provider=InMemory
Persistence__InMemoryDatabaseName=Pokemon2-development

#OBSERVABILITY
Seq__ServerUrl=http://localhost:5341
Seq__ApiKey=

#METADATA STORAGE - Postgres -
POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DATABASE=Pokemon2
POSTGRES_USERNAME=postgres
POSTGRES_PASSWORD=password
POSTGRES_POOLING=true
POSTGRES_MIN_POOL_SIZE=1
POSTGRES_MAX_POOL_SIZE=50
ConnectionStrings__Pokemon2Db="Host=${POSTGRES_HOST};Port=${POSTGRES_PORT};Database=${POSTGRES_DATABASE};Username=${POSTGRES_USERNAME};Password=${POSTGRES_PASSWORD};Pooling=${POSTGRES_POOLING};MinPoolSize=${POSTGRES_MIN_POOL_SIZE};MaxPoolSize=${POSTGRES_MAX_POOL_SIZE}"
```

No todas las variables tienen el mismo papel:

- Algunas son leídas directamente por ASP.NET Core.
- Algunas son leídas directamente por la infraestructura de la API.
- Algunas son auxiliares para componer la cadena de conexión PostgreSQL.
- Algunas son usadas por Docker Compose.
- Algunas aparecen en el ejemplo, pero el código actual no las consume.

## 3. Convención de nombres

En variables de entorno, ASP.NET Core usa doble guion bajo `__` para representar secciones de configuración.

Ejemplos:

| Variable de entorno | Clave de configuración equivalente |
| --- | --- |
| `Persistence__Provider` | `Persistence:Provider` |
| `Persistence__InMemoryDatabaseName` | `Persistence:InMemoryDatabaseName` |
| `ConnectionStrings__Pokemon2Db` | `ConnectionStrings:Pokemon2Db` |
| `Observability__SeqUrl` | `Observability:SeqUrl` |
| `Seed__ApplyMvpRoster` | `Seed:ApplyMvpRoster` |

Esta convención es importante porque el código usa `IConfiguration` y lee claves jerárquicas como `Persistence:Provider` u `Observability:SeqUrl`.

## 4. Variables de entorno de ejecución

### 4.1 `ASPNETCORE_ENVIRONMENT`

Define el entorno de ejecución de ASP.NET Core.

Uso en el proyecto:

- Activa OpenAPI y Scalar cuando el entorno es `Development`.
- Permite usar `Persistence__Provider=InMemory` solo en `Development`.
- Aparece en la información del sistema expuesta por la API.

Valores aceptados:

- `Development`
- `Staging`
- `Production`
- Cualquier otro nombre técnicamente puede existir en ASP.NET Core, pero el código del proyecto solo trata de forma especial `Development`.

Valores recomendados:

- `Development` para desarrollo local.
- `Production` para despliegues productivos.

Valores no recomendados:

- Vacío, porque ASP.NET Core aplicará sus valores por defecto y puede no coincidir con lo esperado.
- `Production` junto con `Persistence__Provider=InMemory`, porque el arranque fallará de forma deliberada.

Ejemplo válido:

```txt
ASPNETCORE_ENVIRONMENT=Development
```

Ejemplo inválido para este proyecto:

```txt
ASPNETCORE_ENVIRONMENT=Production
Persistence__Provider=InMemory
```

Motivo: la persistencia InMemory solo se permite en desarrollo.

### 4.2 `DOTNET_ENVIRONMENT`

Define el entorno genérico de .NET.

Uso en el proyecto:

- No se lee de forma explícita en código propio.
- Puede ser usado por el host genérico de .NET como alternativa o complemento a `ASPNETCORE_ENVIRONMENT`.

Valores aceptados:

- `Development`
- `Staging`
- `Production`
- Otros nombres personalizados, aunque no se recomiendan si no existe una política clara de configuración.

Valor recomendado en local:

```txt
DOTNET_ENVIRONMENT=Development
```

Consideración técnica:

- Para una API ASP.NET Core, `ASPNETCORE_ENVIRONMENT` es la variable más relevante.
- Conviene mantener `ASPNETCORE_ENVIRONMENT` y `DOTNET_ENVIRONMENT` alineadas para evitar diagnósticos confusos.

Ejemplo coherente:

```txt
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development
```

Ejemplo no recomendado:

```txt
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Production
```

Motivo: aunque puede arrancar, deja una configuración ambigua.

## 5. Variables de persistencia

### 5.1 `Persistence__Provider`

Selecciona el proveedor de persistencia que usará Entity Framework Core.

Uso en el proyecto:

- Lo lee `PersistenceProviderResolver`.
- Determina si `PokemonDbContext` se configura con PostgreSQL o con EF Core InMemory.
- Determina si el inicializador ejecuta migraciones o `EnsureCreated`.

Valores aceptados:

- `Postgres`
- `InMemory`

La comparación no distingue mayúsculas de minúsculas, por lo que `postgres`, `POSTGRES` o `inmemory` se pueden parsear. Aun así, se recomienda usar los nombres canónicos:

```txt
Persistence__Provider=Postgres
```

o:

```txt
Persistence__Provider=InMemory
```

Comportamiento si falta:

- Si la variable no existe o está vacía, el código asume `Postgres`.

Restricciones:

- `InMemory` solo es válido cuando el entorno es `Development`.
- En `Production`, `Staging` u otro entorno distinto de `Development`, `InMemory` provoca un fallo de arranque.

Valores no válidos:

```txt
Persistence__Provider=SqlServer
Persistence__Provider=MongoDb
Persistence__Provider=Memory
Persistence__Provider=PostgreSQL
Persistence__Provider=none
```

Motivo: el enumerado soportado por el código solo contempla `Postgres` e `InMemory`.

### 5.2 `Persistence__InMemoryDatabaseName`

Define el nombre de la base de datos usada por EF Core InMemory.

Uso en el proyecto:

- Solo se usa cuando `Persistence__Provider=InMemory`.
- Permite aislar ejecuciones locales usando nombres distintos.

Valores aceptados:

- Cualquier texto no vacío razonable para identificar la base en memoria.

Valor por defecto en código:

```txt
Pokemon2-development
```

Si el proveedor es `InMemory` y la variable no existe o está vacía, el código usa `Pokemon2-development`.

Valores recomendados:

```txt
Persistence__InMemoryDatabaseName=Pokemon2-development
Persistence__InMemoryDatabaseName=Pokemon2-local
Persistence__InMemoryDatabaseName=Pokemon2-demo
```

Valores no recomendados:

```txt
Persistence__InMemoryDatabaseName=
Persistence__InMemoryDatabaseName=prod
```

Motivo:

- Vacío no aporta aislamiento y se sustituirá por el valor por defecto.
- Nombres como `prod` pueden inducir a error, porque InMemory no debe representar un entorno productivo.

## 6. Variables de observabilidad

### 6.1 `Seq__ServerUrl`

URL de Seq según el fichero `.env.example`.

Estado actual:

- Esta variable aparece en `.env.example`.
- El código actual no la lee.
- Serilog no se configura con esta variable.

Por tanto, con el código actual, esta variable no activa el envío de logs a Seq.

Ejemplo presente en el fichero:

```txt
Seq__ServerUrl=http://localhost:5341
```

Valor técnicamente razonable:

- Una URL HTTP o HTTPS válida de un servidor Seq.

Valores razonables si se mantuviera esta convención:

```txt
Seq__ServerUrl=http://localhost:5341
Seq__ServerUrl=http://visiotech-seq
Seq__ServerUrl=https://seq.example.com
```

Valores no válidos:

```txt
Seq__ServerUrl=localhost:5341
Seq__ServerUrl=not-a-url
Seq__ServerUrl=
```

Motivo:

- Debería ser una URL absoluta con esquema `http://` o `https://`.
- Vacío no aporta destino de observabilidad.

Nota técnica:

- La variable correcta para el código actual es `Observability__SeqUrl`.

### 6.2 `Seq__ApiKey`

Clave API de Seq según el fichero `.env.example`.

Estado actual:

- Esta variable aparece en `.env.example`.
- El código actual no la lee.
- El sink de Serilog a Seq se configura solo con URL, sin API key.

Valores aceptables si se implementara su uso:

- Vacío en desarrollo local si Seq no exige API key.
- Una clave generada por Seq si el servidor está protegido.

Ejemplos:

```txt
Seq__ApiKey=
Seq__ApiKey=mi-clave-local
```

Valores no recomendados:

```txt
Seq__ApiKey=admin
Seq__ApiKey=password
Seq__ApiKey=ChangeMe123!
```

Motivo:

- Una API key debe ser un secreto real y no una contraseña débil o compartida.

Nota técnica:

- Aunque se configure, no tendrá efecto mientras el código no la pase explícitamente a `WriteTo.Seq(...)`.

### 6.3 `Observability__SeqUrl`

Esta variable no aparece en `.env.example`, pero es la que consume realmente el código.

Uso en el proyecto:

- El Host lee `Observability:SeqUrl`.
- Si tiene valor, Serilog añade el sink de Seq.
- Si está vacía o no existe, la API sigue arrancando y escribe logs en consola.

Valores aceptados:

- URL absoluta de Seq con esquema `http://` o `https://`.

Ejemplos válidos:

```txt
Observability__SeqUrl=http://localhost:5341
Observability__SeqUrl=http://visiotech-seq
Observability__SeqUrl=https://seq.example.com
```

Valores no válidos o no recomendados:

```txt
Observability__SeqUrl=localhost:5341
Observability__SeqUrl=seq
Observability__SeqUrl=
```

Motivo:

- Sin esquema, Serilog no tiene una URL absoluta clara.
- Vacío desactiva el sink de Seq.

Recomendación:

- Sustituir en `.env.example` `Seq__ServerUrl` por `Observability__SeqUrl` o añadir `Observability__SeqUrl` explícitamente.

## 7. Variables PostgreSQL auxiliares

Las variables `POSTGRES_*` tienen dos usos distintos:

- Docker Compose las usa para configurar el contenedor `visiotech-postgres`.
- Los tests de integración las usan como piezas para construir una cadena de conexión si no se proporciona una cadena explícita.

La API, en cambio, no lee directamente `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DATABASE`, `POSTGRES_USERNAME`, `POSTGRES_PASSWORD`, `POSTGRES_POOLING`, `POSTGRES_MIN_POOL_SIZE` ni `POSTGRES_MAX_POOL_SIZE`. La API usa la cadena final `ConnectionStrings__Pokemon2Db`.

### 7.1 `POSTGRES_HOST`

Host donde está PostgreSQL.

Uso:

- Lo usan los tests de integración para construir la cadena de conexión.
- Puede usarse como variable auxiliar en `.env`.

Valores válidos:

```txt
POSTGRES_HOST=localhost
POSTGRES_HOST=127.0.0.1
POSTGRES_HOST=visiotech-postgres
POSTGRES_HOST=postgres.example.com
```

Cuándo usar cada uno:

- `localhost`: API o tests ejecutándose en el host, con PostgreSQL publicado por Docker.
- `visiotech-postgres`: API ejecutándose dentro de la misma red Docker Compose que el servicio PostgreSQL.
- Un DNS real: entornos remotos o compartidos.

Valores no válidos:

```txt
POSTGRES_HOST=
POSTGRES_HOST=http://localhost
POSTGRES_HOST=localhost:5432
```

Motivo:

- El host debe ser solo nombre o dirección, sin esquema HTTP.
- El puerto se configura por separado con `POSTGRES_PORT` o dentro de la cadena de conexión.

### 7.2 `POSTGRES_PORT`

Puerto de PostgreSQL.

Uso:

- Docker Compose lo usa para publicar el puerto del contenedor en el host.
- Los tests de integración lo usan para construir la cadena de conexión.

Valor por defecto habitual:

```txt
POSTGRES_PORT=5432
```

Valores aceptados:

- Enteros entre `1` y `65535`.
- En la práctica, `5432` es el valor estándar para PostgreSQL.

Ejemplos válidos:

```txt
POSTGRES_PORT=5432
POSTGRES_PORT=15432
```

Valores no válidos:

```txt
POSTGRES_PORT=
POSTGRES_PORT=postgres
POSTGRES_PORT=localhost:5432
POSTGRES_PORT=0
POSTGRES_PORT=70000
```

Motivo:

- Debe ser un puerto TCP válido.

### 7.3 `POSTGRES_DATABASE`

Nombre de la base de datos PostgreSQL.

Uso:

- Docker Compose lo pasa como `POSTGRES_DB` al contenedor.
- Los tests de integración lo usan para construir la cadena de conexión.
- Debe coincidir con la base indicada en `ConnectionStrings__Pokemon2Db` si se usa PostgreSQL.

Valor del ejemplo:

```txt
POSTGRES_DATABASE=Pokemon2
```

Valores aceptados:

- Nombres de base de datos válidos para PostgreSQL.

Ejemplos válidos:

```txt
POSTGRES_DATABASE=Pokemon2
POSTGRES_DATABASE=pokemon2
POSTGRES_DATABASE=pokemon2_local
```

Valores no recomendados:

```txt
POSTGRES_DATABASE=
POSTGRES_DATABASE=Pokemon 2
POSTGRES_DATABASE=postgres
```

Motivo:

- Vacío impide crear o conectar correctamente.
- Espacios complican la operación y no aportan valor.
- `postgres` es una base administrativa habitual; es mejor usar una base propia de aplicación.

### 7.4 `POSTGRES_USERNAME`

Usuario PostgreSQL.

Uso:

- Docker Compose lo pasa como `POSTGRES_USER`.
- Los tests de integración lo usan para construir la cadena de conexión.

Valor del ejemplo:

```txt
POSTGRES_USERNAME=postgres
```

Valores aceptados:

- Un usuario existente en PostgreSQL.
- En desarrollo local puede ser `postgres`.

Ejemplos válidos:

```txt
POSTGRES_USERNAME=postgres
POSTGRES_USERNAME=pokemon_app
```

Valores no recomendados:

```txt
POSTGRES_USERNAME=
POSTGRES_USERNAME=root
POSTGRES_USERNAME=admin
```

Motivo:

- Vacío no permite autenticación.
- Usuarios genéricos o privilegiados no son recomendables fuera de desarrollo.

### 7.5 `POSTGRES_PASSWORD`

Contraseña del usuario PostgreSQL.

Uso:

- Docker Compose lo pasa como `POSTGRES_PASSWORD`.
- Los tests de integración lo usan para construir la cadena de conexión.

Valor del ejemplo:

```txt
POSTGRES_PASSWORD=password
```

Valores aceptados:

- Cualquier contraseña válida para el usuario configurado.

Valores válidos para desarrollo local:

```txt
POSTGRES_PASSWORD=password
POSTGRES_PASSWORD=local-dev-password
```

Valores no recomendados:

```txt
POSTGRES_PASSWORD=
POSTGRES_PASSWORD=postgres
POSTGRES_PASSWORD=admin
POSTGRES_PASSWORD=123456
POSTGRES_PASSWORD=password
```

Matiz:

- `password` aparece en el ejemplo porque es cómodo para desarrollo local.
- No debe usarse en entornos compartidos, CI pública, preproducción ni producción.

### 7.6 `POSTGRES_POOLING`

Activa o desactiva el pool de conexiones de Npgsql.

Uso:

- Variable auxiliar para construir `ConnectionStrings__Pokemon2Db`.
- Los tests de integración también la usan si no hay cadena explícita.

Valor del ejemplo:

```txt
POSTGRES_POOLING=true
```

Valores recomendados:

```txt
POSTGRES_POOLING=true
POSTGRES_POOLING=false
```

Valores no válidos o no recomendados:

```txt
POSTGRES_POOLING=
POSTGRES_POOLING=yes
POSTGRES_POOLING=1
POSTGRES_POOLING=enabled
```

Motivo:

- Para evitar diferencias entre parsers, conviene usar `true` o `false`.
- En condiciones normales se recomienda `true`.

### 7.7 `POSTGRES_MIN_POOL_SIZE`

Tamaño mínimo del pool de conexiones.

Uso:

- Variable auxiliar para construir `ConnectionStrings__Pokemon2Db`.
- Solo tiene efecto si `Pooling=true`.

Valor del ejemplo:

```txt
POSTGRES_MIN_POOL_SIZE=1
```

Valores aceptados:

- Enteros mayores o iguales que `0`.
- Debe ser menor o igual que `POSTGRES_MAX_POOL_SIZE`.

Ejemplos válidos:

```txt
POSTGRES_MIN_POOL_SIZE=0
POSTGRES_MIN_POOL_SIZE=1
POSTGRES_MIN_POOL_SIZE=5
```

Valores no válidos:

```txt
POSTGRES_MIN_POOL_SIZE=
POSTGRES_MIN_POOL_SIZE=-1
POSTGRES_MIN_POOL_SIZE=uno
POSTGRES_MIN_POOL_SIZE=100
POSTGRES_MAX_POOL_SIZE=50
```

Motivo:

- Debe ser numérico.
- No debe superar el máximo del pool.

### 7.8 `POSTGRES_MAX_POOL_SIZE`

Tamaño máximo del pool de conexiones.

Uso:

- Variable auxiliar para construir `ConnectionStrings__Pokemon2Db`.
- Solo tiene efecto si `Pooling=true`.

Valor del ejemplo:

```txt
POSTGRES_MAX_POOL_SIZE=50
```

Valores aceptados:

- Enteros positivos.
- Debe ser mayor o igual que `POSTGRES_MIN_POOL_SIZE`.

Ejemplos válidos:

```txt
POSTGRES_MAX_POOL_SIZE=20
POSTGRES_MAX_POOL_SIZE=50
POSTGRES_MAX_POOL_SIZE=100
```

Valores no válidos:

```txt
POSTGRES_MAX_POOL_SIZE=
POSTGRES_MAX_POOL_SIZE=0
POSTGRES_MAX_POOL_SIZE=-1
POSTGRES_MAX_POOL_SIZE=cincuenta
```

Motivo:

- Debe permitir al menos una conexión.
- Debe ser un número que Npgsql pueda interpretar.

## 8. Cadena de conexión

### 8.1 `ConnectionStrings__Pokemon2Db`

Cadena de conexión usada por Entity Framework Core cuando el proveedor activo es PostgreSQL.

Uso en el proyecto:

- La lee `configuration.GetConnectionString("Pokemon2Db")`.
- Es obligatoria cuando `Persistence__Provider=Postgres`.
- Si falta o está vacía con proveedor PostgreSQL, el arranque falla.
- Se usa para ejecutar migraciones con `Database.MigrateAsync()`.

Valor del ejemplo:

```txt
ConnectionStrings__Pokemon2Db="Host=${POSTGRES_HOST};Port=${POSTGRES_PORT};Database=${POSTGRES_DATABASE};Username=${POSTGRES_USERNAME};Password=${POSTGRES_PASSWORD};Pooling=${POSTGRES_POOLING};MinPoolSize=${POSTGRES_MIN_POOL_SIZE};MaxPoolSize=${POSTGRES_MAX_POOL_SIZE}"
```

Formato esperado:

```txt
Host=localhost;Port=5432;Database=Pokemon2;Username=postgres;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=50
```

Ejemplo para ejecutar la API en el host:

```txt
ConnectionStrings__Pokemon2Db=Host=localhost;Port=5432;Database=Pokemon2;Username=postgres;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=50
```

Ejemplo para ejecutar la API dentro de Docker Compose:

```txt
ConnectionStrings__Pokemon2Db=Host=visiotech-postgres;Port=5432;Database=Pokemon2;Username=postgres;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=50
```

Valores válidos:

- Una cadena de conexión Npgsql válida.
- Debe apuntar a una base PostgreSQL accesible.
- Debe incluir credenciales correctas.

Valores no válidos:

```txt
ConnectionStrings__Pokemon2Db=
ConnectionStrings__Pokemon2Db=localhost
ConnectionStrings__Pokemon2Db=Server=localhost;Database=Pokemon2
ConnectionStrings__Pokemon2Db=Host=localhost;Database=Pokemon2
```

Motivo:

- Vacía no sirve para PostgreSQL.
- `Server=...` es habitual en SQL Server, no en Npgsql.
- Si faltan usuario, contraseña o puerto puede funcionar solo en configuraciones muy concretas, pero no es recomendable para este proyecto.

Consideración sobre interpolación:

- El valor del `.env.example` está escrito con referencias como `${POSTGRES_HOST}`.
- Docker Compose sí usa interpolación de variables en `docker-compose.yml`.
- Para la API, lo más seguro es dejar `ConnectionStrings__Pokemon2Db` expandida con valores concretos en el `.env` real.

Ejemplo recomendado para `.env` real:

```txt
ConnectionStrings__Pokemon2Db=Host=localhost;Port=5432;Database=Pokemon2;Username=postgres;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=50
```

## 9. Variables usadas por Docker Compose que no aparecen todas en `.env.example`

El `docker-compose.yml` usa algunas variables adicionales.

### 9.1 `SEQ_PORT`

Puerto del host donde se publica Seq.

Uso:

- Lo usa Docker Compose.
- No lo lee la API.

Valor por defecto en compose:

```txt
SEQ_PORT=5341
```

Valores aceptados:

- Enteros entre `1` y `65535`.

Ejemplos válidos:

```txt
SEQ_PORT=5341
SEQ_PORT=15341
```

Valores no válidos:

```txt
SEQ_PORT=
SEQ_PORT=seq
SEQ_PORT=0
SEQ_PORT=70000
```

### 9.2 `SEQ_FIRSTRUN_ADMINPASSWORD`

Contraseña inicial del administrador de Seq.

Uso:

- Lo usa Docker Compose para configurar el contenedor de Seq.
- No lo lee la API.

Valor por defecto en compose:

```txt
SEQ_FIRSTRUN_ADMINPASSWORD=ChangeMe123!
```

Valores aceptados:

- Una contraseña válida para Seq.

Valores no recomendados:

```txt
SEQ_FIRSTRUN_ADMINPASSWORD=ChangeMe123!
SEQ_FIRSTRUN_ADMINPASSWORD=password
SEQ_FIRSTRUN_ADMINPASSWORD=admin
```

Motivo:

- El valor por defecto es aceptable para desarrollo local, pero no para entornos compartidos.

## 10. Variables soportadas por el código pero ausentes en `.env.example`

### 10.1 `Seed__ApplyMvpRoster`

Activa la carga inicial de datos MVP.

Uso en el proyecto:

- Lo lee `PokemonSeedOptions`.
- Lo ejecuta `DatabaseInitializer` después de crear o migrar la base.
- Si es `true`, inserta especies, movimientos y relaciones aprendibles si las tablas correspondientes están vacías.

Valor por defecto en `appsettings.json`:

```txt
Seed__ApplyMvpRoster=false
```

Valores aceptados:

```txt
Seed__ApplyMvpRoster=true
Seed__ApplyMvpRoster=false
```

La lectura usa `bool.TryParse`, por lo que `True`, `False`, `TRUE` y `FALSE` también son válidos.

Valores no válidos:

```txt
Seed__ApplyMvpRoster=1
Seed__ApplyMvpRoster=0
Seed__ApplyMvpRoster=yes
Seed__ApplyMvpRoster=no
Seed__ApplyMvpRoster=
```

Comportamiento con valores no válidos:

- El código los interpreta como `false`, porque `bool.TryParse` falla.

### 10.2 `ASPNETCORE_URLS`

Define las URLs en las que escucha la API.

Estado:

- No aparece en `.env.example`.
- El Dockerfile la fija a `http://+:8080`.

Uso:

- La consume ASP.NET Core.
- Permite cambiar el puerto de escucha de Kestrel.

Ejemplos válidos:

```txt
ASPNETCORE_URLS=http://localhost:5000
ASPNETCORE_URLS=http://0.0.0.0:8080
ASPNETCORE_URLS=http://+:8080
```

Valores no recomendados:

```txt
ASPNETCORE_URLS=
ASPNETCORE_URLS=localhost:5000
```

Motivo:

- Debe ser una URL válida con esquema.

### 10.3 `IntegrationTests__Pokemon2Db`

Cadena de conexión específica para tests de integración.

Estado:

- No aparece en `.env.example`.
- La usan los tests de integración.
- Tiene prioridad sobre `ConnectionStrings__Pokemon2Db` dentro de `CustomWebApplicationFactory`.

Uso:

- Permite apuntar los tests a una base de integración distinta de la base local de la API.

Ejemplo válido:

```txt
IntegrationTests__Pokemon2Db=Host=localhost;Port=5432;Database=Pokemon2Tests;Username=postgres;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=20
```

Valores no válidos:

```txt
IntegrationTests__Pokemon2Db=
IntegrationTests__Pokemon2Db=localhost
```

## 11. Configuraciones recomendadas

### 11.1 Desarrollo local con PostgreSQL

```txt
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development

Persistence__Provider=Postgres
ConnectionStrings__Pokemon2Db=Host=localhost;Port=5432;Database=Pokemon2;Username=postgres;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=50
Observability__SeqUrl=http://localhost:5341
Seed__ApplyMvpRoster=false
```

### 11.2 Desarrollo local sin PostgreSQL

```txt
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development

Persistence__Provider=InMemory
Persistence__InMemoryDatabaseName=Pokemon2-development
Observability__SeqUrl=http://localhost:5341
Seed__ApplyMvpRoster=false
```

Consideración:

- Esta configuración permite arrancar la API sin PostgreSQL.
- No debe usarse para validar migraciones ni comportamiento real de persistencia.

### 11.3 API en Docker conectada al PostgreSQL del compose

```txt
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development

Persistence__Provider=Postgres
ConnectionStrings__Pokemon2Db=Host=visiotech-postgres;Port=5432;Database=Pokemon2;Username=postgres;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=50
Observability__SeqUrl=http://visiotech-seq
Seed__ApplyMvpRoster=false
```

### 11.4 Producción

```txt
ASPNETCORE_ENVIRONMENT=Production
DOTNET_ENVIRONMENT=Production

Persistence__Provider=Postgres
ConnectionStrings__Pokemon2Db=Host=postgres.example.internal;Port=5432;Database=Pokemon2;Username=pokemon_app;Password=<secret>;Pooling=true;MinPoolSize=1;MaxPoolSize=50
Observability__SeqUrl=https://seq.example.com
Seed__ApplyMvpRoster=false
```

Restricciones para producción:

- No usar `Persistence__Provider=InMemory`.
- No usar contraseñas por defecto.
- No guardar secretos reales en el repositorio.
- No depender de `.env.example` como configuración productiva.

## 12. Problemas habituales

### 12.1 La API arranca con InMemory aunque se esperaba PostgreSQL

Revisar:

```txt
Persistence__Provider
ASPNETCORE_ENVIRONMENT
DOTNET_ENVIRONMENT
```

Si `Persistence__Provider=InMemory` y `ASPNETCORE_ENVIRONMENT=Development`, la API usará memoria.

### 12.2 La API falla al arrancar con PostgreSQL

Revisar:

```txt
Persistence__Provider=Postgres
ConnectionStrings__Pokemon2Db
POSTGRES_HOST
POSTGRES_PORT
POSTGRES_DATABASE
POSTGRES_USERNAME
POSTGRES_PASSWORD
```

Causas frecuentes:

- PostgreSQL no está levantado.
- La cadena de conexión usa `localhost` desde dentro de un contenedor.
- La base de datos o las credenciales no coinciden con Docker Compose.
- `ConnectionStrings__Pokemon2Db` está vacía.

### 12.3 No llegan logs a Seq

Revisar:

```txt
Observability__SeqUrl
```

El código actual no usa:

```txt
Seq__ServerUrl
Seq__ApiKey
```

Por tanto, si solo se configura `Seq__ServerUrl`, la API seguirá escribiendo en consola, pero no añadirá el sink de Seq desde esa variable.

### 12.4 Docker Compose levanta PostgreSQL, pero la API no conecta desde Docker

Si la API corre dentro de Docker, no usar:

```txt
Host=localhost
```

Usar:

```txt
Host=visiotech-postgres
```

Motivo:

- Dentro de un contenedor, `localhost` apunta al propio contenedor.
- Para acceder a otro contenedor de la misma red Compose se usa el nombre del servicio.

## 13. Resumen de variables

| Variable | Consumidor | Obligatoria | Valores válidos principales |
| --- | --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core y código de aplicación | Recomendada | `Development`, `Staging`, `Production` |
| `DOTNET_ENVIRONMENT` | Host .NET | Recomendada | `Development`, `Staging`, `Production` |
| `Persistence__Provider` | Infraestructura | No, por defecto `Postgres` | `Postgres`, `InMemory` |
| `Persistence__InMemoryDatabaseName` | Infraestructura | No | Texto no vacío |
| `Seq__ServerUrl` | Nadie en el código actual | No | Sin efecto actual |
| `Seq__ApiKey` | Nadie en el código actual | No | Sin efecto actual |
| `Observability__SeqUrl` | Host / Serilog | No | URL absoluta HTTP/HTTPS |
| `POSTGRES_HOST` | Docker Compose auxiliar / tests | Sí para compose práctico | Host o DNS sin esquema |
| `POSTGRES_PORT` | Docker Compose / tests | No, compose tiene `5432` como fallback en puertos | Puerto TCP válido |
| `POSTGRES_DATABASE` | Docker Compose / tests | Sí para compose | Nombre de base PostgreSQL |
| `POSTGRES_USERNAME` | Docker Compose / tests | Sí para compose | Usuario PostgreSQL |
| `POSTGRES_PASSWORD` | Docker Compose / tests | Sí para compose | Contraseña PostgreSQL |
| `POSTGRES_POOLING` | Cadena de conexión / tests | No | `true`, `false` |
| `POSTGRES_MIN_POOL_SIZE` | Cadena de conexión / tests | No | Entero `>= 0` |
| `POSTGRES_MAX_POOL_SIZE` | Cadena de conexión / tests | No | Entero `> 0` y `>= MinPoolSize` |
| `ConnectionStrings__Pokemon2Db` | EF Core / Npgsql | Sí con `Postgres` | Cadena Npgsql válida |
| `Seed__ApplyMvpRoster` | Inicializador de base de datos | No | `true`, `false` |
| `SEQ_PORT` | Docker Compose | No | Puerto TCP válido |
| `SEQ_FIRSTRUN_ADMINPASSWORD` | Docker Compose | No | Contraseña válida para Seq |
| `ASPNETCORE_URLS` | ASP.NET Core / Kestrel | No | URL de escucha válida |
| `IntegrationTests__Pokemon2Db` | Tests de integración | No | Cadena Npgsql válida |

## 14. Recomendación sobre el `.env.example`

El ejemplo actual es válido como punto de partida, pero convendría ajustarlo para reflejar el código real.

Cambios recomendados:

- Añadir `Observability__SeqUrl=http://localhost:5341`.
- Indicar que `Seq__ServerUrl` y `Seq__ApiKey` no tienen efecto actualmente o eliminarlas hasta que se implementen.
- Añadir `Seed__ApplyMvpRoster=false`.
- Dejar `ConnectionStrings__Pokemon2Db` con valores concretos o documentar claramente si se espera interpolación.
- Mantener `Persistence__Provider=InMemory` solo si el objetivo del ejemplo es arrancar sin PostgreSQL; si el objetivo es reflejar infraestructura real, usar `Postgres`.

Una versión más alineada con el código actual para desarrollo local con PostgreSQL sería:

```txt
ASPNETCORE_ENVIRONMENT=Development
DOTNET_ENVIRONMENT=Development

Persistence__Provider=Postgres
Persistence__InMemoryDatabaseName=Pokemon2-development

Observability__SeqUrl=http://localhost:5341
Seed__ApplyMvpRoster=false

POSTGRES_HOST=localhost
POSTGRES_PORT=5432
POSTGRES_DATABASE=Pokemon2
POSTGRES_USERNAME=postgres
POSTGRES_PASSWORD=password
POSTGRES_POOLING=true
POSTGRES_MIN_POOL_SIZE=1
POSTGRES_MAX_POOL_SIZE=50
ConnectionStrings__Pokemon2Db=Host=localhost;Port=5432;Database=Pokemon2;Username=postgres;Password=password;Pooling=true;MinPoolSize=1;MaxPoolSize=50
```

