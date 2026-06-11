# Dockerización del backend

## 1. Propósito

Este documento explica la dockerización actual del proyecto `Visiotech.Pokemon` desde un punto de vista técnico. Describe el `Dockerfile` de la API, el `docker-compose.yml` existente, las variables de entorno implicadas, la topología de red, el arranque de persistencia y las limitaciones actuales que conviene conocer antes de usarlo en desarrollo o evolucionarlo hacia un entorno más completo.

## 2. Estado actual

El repositorio contiene dos piezas relacionadas con Docker:

- `src/Host/Visiotech.Pokemon.Host/Dockerfile`
- `docker-compose.yml`

La situación actual es importante:

- El `Dockerfile` permite construir una imagen Docker de la API.
- El `docker-compose.yml` no levanta la API.
- El `docker-compose.yml` levanta únicamente dependencias de soporte: PostgreSQL y Seq.
- La API se ejecuta normalmente desde el host con `dotnet run`, conectándose a PostgreSQL y Seq publicados por Docker.

Por tanto, el compose actual debe entenderse como un compose de infraestructura local, no como un stack completo de aplicación.

## 3. Dockerfile de la API

El Dockerfile está en:

```txt
src/Host/Visiotech.Pokemon.Host/Dockerfile
```

Define una construcción multi-stage:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
...
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
```

### 3.1 Fase `build`

La primera fase usa la imagen:

```txt
mcr.microsoft.com/dotnet/sdk:10.0
```

Esta imagen contiene el SDK completo de .NET y se usa para restaurar paquetes y publicar la aplicación.

El flujo de la fase de build es:

1. Establece `/src` como directorio de trabajo.
2. Copia ficheros globales de build:
   `Directory.Build.props`, `Directory.Packages.props`, `global.json` y `Visiotech.Pokemon.sln`.
3. Copia los `.csproj` de todos los proyectos necesarios.
4. Ejecuta `dotnet restore` sobre el proyecto Host.
5. Copia el código fuente de `src`.
6. Ejecuta `dotnet publish` en modo `Release`.

La copia previa de `.csproj` antes del código fuente permite aprovechar la caché de capas de Docker: si cambia código pero no cambian dependencias, Docker puede reutilizar la restauración de paquetes.

### 3.2 Fase `final`

La fase final usa:

```txt
mcr.microsoft.com/dotnet/aspnet:10.0
```

Esta imagen contiene solo el runtime ASP.NET Core, no el SDK. Es más pequeña y adecuada para ejecutar la API publicada.

Configuración relevante:

```dockerfile
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Visiotech.Pokemon.Host.dll"]
```

Implicaciones:

- La API escucha dentro del contenedor en el puerto `8080`.
- El puerto `8080` se documenta con `EXPOSE`, pero no se publica automáticamente al host.
- Para acceder desde fuera del contenedor hay que mapear un puerto, por ejemplo `-p 8080:8080`.
- El punto de entrada ejecuta el ensamblado publicado del proyecto Host.

### 3.3 Construcción manual de la imagen

Desde la raíz del repositorio:

```bash
docker build \
  -f src/Host/Visiotech.Pokemon.Host/Dockerfile \
  -t visiotech-pokemon-api .
```

El contexto de build es la raíz del repositorio. Actualmente no hay `.dockerignore`, por lo que Docker envía todo el contenido del repositorio al contexto de build. Aunque el Dockerfile copia rutas concretas, añadir un `.dockerignore` sería recomendable para excluir `.git`, `bin`, `obj`, `.nuget`, `.dotnet`, ficheros temporales y documentación pesada.

## 4. Docker Compose actual

El fichero `docker-compose.yml` declara dos servicios:

- `visiotech-postgres`
- `visiotech-seq`

También declara una red:

- `visiotech`

No declara volúmenes.

## 5. Servicio PostgreSQL

Servicio:

```yaml
visiotech-postgres:
  image: postgres:17
  container_name: visiotech-pokemon-postgres
  restart: unless-stopped
```

### 5.1 Imagen

Se usa la imagen oficial:

```txt
postgres:17
```

Esto da un PostgreSQL real para desarrollo local e integración manual con la API.

### 5.2 Variables de entorno

El servicio se configura con:

```yaml
POSTGRES_DB: ${POSTGRES_DATABASE}
POSTGRES_USER: ${POSTGRES_USERNAME}
POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
```

Estas variables se resuelven por Docker Compose desde:

- variables exportadas en la shell
- fichero `.env` en la raíz del proyecto

El `.env.example` define valores esperados como:

```txt
POSTGRES_DATABASE=Pokemon2
POSTGRES_USERNAME=postgres
POSTGRES_PASSWORD=password
POSTGRES_PORT=5432
```

### 5.3 Puerto

El compose publica:

```yaml
ports:
  - "${POSTGRES_PORT:-5432}:5432"
```

Esto significa:

- Dentro del contenedor PostgreSQL escucha en `5432`.
- En el host se publica en `POSTGRES_PORT`.
- Si `POSTGRES_PORT` no existe, se usa `5432`.

Cuando la API se ejecuta fuera de Docker con `dotnet run`, la cadena de conexión puede usar:

```txt
Host=localhost;Port=5432
```

Cuando la API se ejecuta dentro de Docker y comparte red con PostgreSQL, no debe usar `localhost`. Debe usar el nombre del servicio:

```txt
Host=visiotech-postgres;Port=5432
```

### 5.4 Healthcheck

El servicio tiene healthcheck:

```yaml
healthcheck:
  test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USERNAME} -d ${POSTGRES_DATABASE}"]
  interval: 10s
  timeout: 5s
  retries: 10
  start_period: 10s
```

Este healthcheck permite saber si PostgreSQL está listo para aceptar conexiones.

Limitación actual:

- Como la API no está en el compose, ningún servicio usa `depends_on` contra este healthcheck.
- El healthcheck es útil para diagnóstico con `docker compose ps`, pero no coordina el arranque de la API.

### 5.5 Persistencia de datos

El servicio PostgreSQL no declara volumen.

Implicaciones:

- Los datos sobreviven a reinicios normales del contenedor mientras el contenedor exista.
- Los datos pueden perderse si el contenedor se elimina y se recrea sin volumen persistente.
- Para un entorno local más robusto, convendría añadir un volumen, por ejemplo `postgres_data:/var/lib/postgresql/data`.

## 6. Servicio Seq

Servicio:

```yaml
visiotech-seq:
  image: datalust/seq:latest
  container_name: visiotech-pokemon-seq
  restart: unless-stopped
```

Seq se usa como visor y almacén local de logs estructurados.

### 6.1 Variables de entorno

El compose configura:

```yaml
ACCEPT_EULA: "Y"
SEQ_FIRSTRUN_ADMINPASSWORD: ${SEQ_FIRSTRUN_ADMINPASSWORD:-ChangeMe123!}
```

`ACCEPT_EULA=Y` es obligatorio para arrancar la imagen de Seq.

`SEQ_FIRSTRUN_ADMINPASSWORD` tiene un valor por defecto. Ese valor es suficiente para desarrollo local, pero no debe usarse en entornos compartidos o productivos.

### 6.2 Puerto

El compose publica:

```yaml
ports:
  - "${SEQ_PORT:-5341}:80"
```

Esto significa:

- Dentro del contenedor Seq escucha en `80`.
- En el host se publica en `SEQ_PORT`.
- Si `SEQ_PORT` no existe, se usa `5341`.

Cuando la API se ejecuta en local fuera de Docker, la URL correcta de Seq es:

```txt
http://localhost:5341
```

Cuando la API se ejecuta dentro de la misma red Docker que Seq, la URL correcta es:

```txt
http://visiotech-seq
```

### 6.3 Persistencia de logs

El servicio Seq no declara volumen.

Implicaciones:

- La configuración y los eventos pueden perderse si el contenedor se elimina.
- Para conservar histórico de logs, convendría añadir un volumen, por ejemplo `seq_data:/data`.

## 7. Red Docker

El compose declara:

```yaml
networks:
  visiotech:
    driver: bridge
```

Todos los servicios del compose se conectan a esa red.

Dentro de esa red, los contenedores se resuelven por nombre de servicio:

- `visiotech-postgres`
- `visiotech-seq`

Desde el host, en cambio, se accede por los puertos publicados:

- PostgreSQL: `localhost:${POSTGRES_PORT:-5432}`
- Seq: `http://localhost:${SEQ_PORT:-5341}`

La diferencia entre “dentro de Docker” y “fuera de Docker” es crítica. `localhost` dentro de un contenedor apunta al propio contenedor, no al host ni a otros servicios.

## 8. Arranque recomendado en desarrollo

### 8.1 Levantar dependencias

Desde la raíz del repositorio:

```bash
docker compose up -d
```

Comprobar estado:

```bash
docker compose ps
```

Ver logs de PostgreSQL:

```bash
docker compose logs -f visiotech-postgres
```

Ver logs de Seq:

```bash
docker compose logs -f visiotech-seq
```

### 8.2 Ejecutar la API fuera de Docker

Con las dependencias levantadas:

```bash
dotnet run --project src/Host/Visiotech.Pokemon.Host
```

En este modo, la API usa variables de entorno o `.env`.

La cadena de conexión esperada para PostgreSQL local es:

```txt
ConnectionStrings__Pokemon2Db=Host=localhost;Port=5432;Database=Pokemon2;Username=postgres;Password=password
```

El proveedor de persistencia se configura con:

```txt
Persistence__Provider=Postgres
```

Si se configura:

```txt
Persistence__Provider=InMemory
```

la API usará EF Core InMemory, pero solo en `Development`. En entornos distintos de `Development`, el arranque falla deliberadamente.

## 9. Ejecutar la API como contenedor

Aunque el compose no incluye la API, se puede ejecutar manualmente.

Primero, construir la imagen:

```bash
docker build \
  -f src/Host/Visiotech.Pokemon.Host/Dockerfile \
  -t visiotech-pokemon-api .
```

Después, ejecutar la API conectada a la red del compose. El nombre exacto de la red puede variar por el nombre del proyecto Compose. Se puede consultar con:

```bash
docker network ls
```

Ejemplo habitual:

```bash
docker run --rm \
  --network visiotech_visiotech \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e Persistence__Provider=Postgres \
  -e ConnectionStrings__Pokemon2Db="Host=visiotech-postgres;Port=5432;Database=Pokemon2;Username=postgres;Password=password" \
  -e Observability__SeqUrl="http://visiotech-seq" \
  visiotech-pokemon-api
```

Puntos importantes:

- `Host=visiotech-postgres`, no `localhost`.
- `Observability__SeqUrl=http://visiotech-seq`, no `http://localhost:5341`.
- Se publica `8080:8080` porque el Dockerfile expone la API en `8080`.

## 10. Variables que consume realmente la API

La API carga variables desde `.env` usando `DotNetEnv` antes de construir `WebApplicationBuilder`.

Regla importante:

- `Env.NoClobber()` no sobrescribe variables ya existentes en el proceso.

Variables relevantes para la API:

| Variable | Uso |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Define el entorno de ASP.NET Core. |
| `DOTNET_ENVIRONMENT` | Define el entorno genérico de .NET. |
| `ASPNETCORE_URLS` | Define URLs de escucha. En Dockerfile se fija a `http://+:8080`. |
| `Persistence__Provider` | `Postgres` o `InMemory`. |
| `Persistence__InMemoryDatabaseName` | Nombre de base EF InMemory. |
| `ConnectionStrings__Pokemon2Db` | Cadena de conexión PostgreSQL. |
| `Observability__SeqUrl` | URL de Seq para Serilog. |
| `Seed__ApplyMvpRoster` | Activa o desactiva seed inicial. |

### 10.1 Observabilidad y discrepancia actual

El código lee:

```txt
Observability:SeqUrl
```

Como variable de entorno, eso equivale a:

```txt
Observability__SeqUrl
```

Sin embargo, `.env.example` contiene:

```txt
Seq__ServerUrl=http://localhost:5341
Seq__ApiKey=
```

Estas variables `Seq__...` no configuran el sink de Seq en el código actual. Para que Serilog envíe logs a Seq, debe configurarse `Observability__SeqUrl`.

## 11. Migraciones y arranque de base de datos

Durante el arranque, el Host ejecuta:

```csharp
DatabaseInitializer.InitializeAsync(...)
```

Comportamiento:

- Si `Persistence__Provider=Postgres`, ejecuta `Database.MigrateAsync()`.
- Si `Persistence__Provider=InMemory`, ejecuta `Database.EnsureCreatedAsync()`.
- Si `Seed__ApplyMvpRoster=true`, inserta datos MVP si las tablas están vacías.

Implicaciones en Docker:

- PostgreSQL debe estar accesible cuando arranca la API.
- Si PostgreSQL no está listo, la API puede fallar durante el arranque.
- El healthcheck del compose ayuda a diagnosticar PostgreSQL, pero no retrasa la API porque la API no está declarada en compose.

## 12. Seguridad y configuración sensible

El compose actual está orientado a desarrollo local.

Riesgos o puntos a cuidar:

- `POSTGRES_PASSWORD=password` en `.env.example` es solo un valor local de ejemplo.
- `SEQ_FIRSTRUN_ADMINPASSWORD` tiene un valor por defecto débil para entornos compartidos.
- No hay gestión de secretos con Docker secrets.
- PostgreSQL publica un puerto en el host.
- Seq publica un puerto en el host.
- No hay TLS entre contenedores.

Para producción o entornos compartidos:

- no usar contraseñas por defecto
- inyectar secretos desde el entorno o un gestor seguro
- evitar publicar PostgreSQL si no es necesario
- proteger Seq con credenciales fuertes
- separar la ejecución de migraciones de la puesta en marcha de la API si se requiere control operacional estricto

## 13. Limitaciones técnicas actuales

Limitaciones observadas en el estado actual del repositorio:

- El compose no levanta la API.
- El compose no define volúmenes para PostgreSQL ni Seq.
- No existe `.dockerignore`.
- El healthcheck de PostgreSQL no coordina el arranque de la API.
- `.env.example` usa `Seq__ServerUrl`, pero el código espera `Observability__SeqUrl`.
- No hay perfiles de Docker Compose para elegir entre dependencias solo y stack completo.
- No hay servicio de migraciones separado.

Estas limitaciones no impiden el desarrollo local, pero conviene documentarlas porque afectan a reproducibilidad, persistencia de datos y observabilidad.

## 14. Evolución recomendada

Para convertir el compose en un stack completo de desarrollo, se podría añadir un servicio `api`:

```yaml
api:
  build:
    context: .
    dockerfile: src/Host/Visiotech.Pokemon.Host/Dockerfile
  environment:
    ASPNETCORE_ENVIRONMENT: Development
    Persistence__Provider: Postgres
    ConnectionStrings__Pokemon2Db: Host=visiotech-postgres;Port=5432;Database=${POSTGRES_DATABASE};Username=${POSTGRES_USERNAME};Password=${POSTGRES_PASSWORD}
    Observability__SeqUrl: http://visiotech-seq
  ports:
    - "8080:8080"
  depends_on:
    visiotech-postgres:
      condition: service_healthy
    visiotech-seq:
      condition: service_started
  networks:
    - visiotech
```

También sería recomendable añadir volúmenes:

```yaml
volumes:
  postgres_data:
  seq_data:
```

y montarlos:

```yaml
visiotech-postgres:
  volumes:
    - postgres_data:/var/lib/postgresql/data

visiotech-seq:
  volumes:
    - seq_data:/data
```

Finalmente, convendría añadir `.dockerignore` para reducir el contexto de build:

```txt
.git
.dotnet
.nuget
**/bin
**/obj
tmp
docs/requirements/*.pdf
```

## 15. Resumen técnico

La dockerización actual está parcialmente preparada:

- La API tiene un Dockerfile correcto con build multi-stage.
- PostgreSQL y Seq se levantan con Docker Compose como dependencias locales.
- La API no forma parte del compose actual.
- La persistencia principal es PostgreSQL y las migraciones se aplican al arrancar la API.
- Seq se configura desde `Observability__SeqUrl`, no desde `Seq__ServerUrl`.
- Para un entorno local más robusto faltan volúmenes, `.dockerignore` y, si se desea stack completo, un servicio `api` en compose.

En su forma actual, el diseño es adecuado para levantar dependencias rápidamente durante desarrollo y ejecutar la API desde el host. Para ejecutar todo en contenedores de forma reproducible, hay que añadir la API al compose y ajustar las cadenas de conexión a nombres de servicio Docker.
