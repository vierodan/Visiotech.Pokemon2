# Backend Components

## 1. Objetivo del documento

Este documento describe la estructura del backend de `Visiotech.Pokemon` desde el punto de vista de despliegue, infraestructura operativa y observabilidad.

El backend quedara compuesto por tres contenedores coordinados mediante `Docker Compose`:

- una `API` .NET dockerizada
- una base de datos `PostgreSQL`
- una instancia de `Seq` para observabilidad

La API utilizara:

- `Entity Framework Core` para persistencia
- `PostgreSQL` como motor de base de datos
- `Serilog` como pipeline de logging estructurado
- `Seq` como destino central para consulta y analisis de logs

## 2. Vision general

La arquitectura de despliegue propuesta es una arquitectura de backend compacta, orientada a MVP, pero con una separacion clara de responsabilidades:

- la `API` expone contratos HTTP, ejecuta casos de uso y orquesta acceso a persistencia
- `PostgreSQL` almacena el estado del sistema de forma transaccional
- `Seq` recibe y visualiza logs estructurados emitidos por la API

En terminos de ejecucion, el sistema queda asi:

1. un cliente HTTP llama a la API
2. la API procesa la peticion
3. la API consulta o modifica datos en PostgreSQL
4. la API emite eventos de log estructurados con Serilog
5. Serilog escribe los logs en consola y los envia a Seq
6. el equipo consulta Seq para diagnostico, auditoria operativa y troubleshooting

## 3. Componentes del backend

## 3.1 API .NET

La API es el punto de entrada del sistema.

Responsabilidades principales:

- exponer endpoints HTTP del dominio Pokemon
- validar contratos de entrada
- ejecutar comandos y queries de la capa de aplicacion
- persistir informacion mediante EF Core
- aplicar migraciones al arrancar cuando corresponda
- emitir logs estructurados sobre flujo funcional y errores

Responsabilidades que no debe asumir:

- no debe almacenar estado fuera de la base de datos
- no debe actuar como visor de logs
- no debe mezclar persistencia con configuracion de observabilidad

Aspectos operativos relevantes:

- se ejecuta dentro de un contenedor dedicado
- se conecta por red interna de Docker con PostgreSQL y Seq
- recibe configuracion por variables de entorno
- expone un puerto HTTP hacia el host

Configuracion esperable de la API:

- `ASPNETCORE_ENVIRONMENT`
- `ASPNETCORE_URLS`
- `ConnectionStrings__Pokemon2Db`
- `Persistence__Provider`
- `Serilog__SeqUrl`

## 3.2 PostgreSQL

`PostgreSQL` es el componente de persistencia del backend.

Responsabilidades principales:

- almacenar entidades y relaciones del dominio
- aplicar constraints, indices y garantias de integridad
- soportar transacciones y consistencia para la API

Motivos para aislarlo en su propio contenedor:

- separacion clara entre aplicacion y datos
- facilidad de reemplazo, backup y mantenimiento
- configuracion independiente de volumenes y credenciales

Aspectos operativos recomendados:

- volumen persistente para no perder datos al reiniciar
- credenciales definidas por variables de entorno
- puerto publicado solo cuando sea necesario para acceso local

Variables de entorno habituales:

- `POSTGRES_DB`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`

## 3.3 Seq

`Seq` es la consola de observabilidad del backend.

No es un simple visor de texto. Su funcion es centralizar logs estructurados y permitir:

- filtrar por nivel de severidad
- buscar por propiedades enriquecidas
- inspeccionar excepciones
- analizar comportamiento funcional y tecnico

En esta arquitectura:

- `Serilog` produce eventos estructurados
- `Seq` los recibe por HTTP
- el equipo consulta Seq desde navegador

Motivos para incluir Seq desde el backend base:

- facilita diagnostico rapido del MVP
- reduce friccion para analizar errores y flujos
- deja una senda clara para observabilidad mas avanzada en el futuro

Variables de entorno habituales:

- `ACCEPT_EULA=Y`
- `SEQ_FIRSTRUN_ADMINPASSWORD`

## 4. Relacion entre componentes

La relacion entre los tres contenedores debe ser directa, simple y desacoplada:

- la API depende funcionalmente de PostgreSQL
- la API depende operativamente de Seq para observabilidad
- PostgreSQL no depende de Seq
- Seq no depende de PostgreSQL

Esto implica una topologia tipo estrella:

- `api -> postgres`
- `api -> seq`

No deben existir integraciones innecesarias entre `postgres` y `seq`.

## 5. Flujo de una peticion

## 5.1 Flujo funcional

1. el cliente envia una peticion HTTP a la API
2. la API resuelve el endpoint correspondiente
3. la capa de aplicacion ejecuta el caso de uso
4. la infraestructura usa EF Core para acceder a PostgreSQL
5. PostgreSQL confirma lectura o escritura
6. la API devuelve la respuesta HTTP

## 5.2 Flujo de observabilidad

En paralelo al flujo funcional:

1. la API genera logs de inicio, fin, warning o error
2. `Serilog` enriquece el evento con metadata relevante
3. el evento se escribe en consola
4. el mismo evento se envia a `Seq`
5. `Seq` lo indexa y lo deja disponible para consulta

Esto permite tener dos niveles de diagnostico:

- consola del contenedor para inspeccion rapida
- Seq para analisis estructurado y persistente de eventos

## 6. Docker Compose como orquestador local

`Docker Compose` es la pieza que une todo el backend.

Su papel en esta arquitectura es:

- levantar los tres contenedores con un solo comando
- definir la red interna entre servicios
- configurar variables de entorno
- montar volumenes persistentes
- publicar puertos al host
- expresar dependencias de arranque

Para el MVP, `Docker Compose` es suficiente y adecuado porque:

- simplifica el entorno local
- reduce configuracion manual
- aproxima el comportamiento real de despliegue
- mantiene una topologia clara y repetible

## 7. Propuesta de docker compose

El siguiente ejemplo describe la forma esperada de componer el backend:

```yaml
version: "3.9"

services:
  api:
    build:
      context: .
      dockerfile: src/Host/Visiotech.Pokemon.Host/Dockerfile
    container_name: visiotech-pokemon-api
    depends_on:
      - postgres
      - seq
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ASPNETCORE_URLS: http://+:8080
      Persistence__Provider: PostgreSql
      ConnectionStrings__Pokemon2Db: Host=postgres;Port=5432;Database=visiotech_pokemon;Username=postgres;Password=postgres
      Serilog__SeqUrl: http://seq:5341
    ports:
      - "8080:8080"
    networks:
      - backend

  postgres:
    image: postgres:17
    container_name: visiotech-pokemon-postgres
    environment:
      POSTGRES_DB: visiotech_pokemon
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - backend

  seq:
    image: datalust/seq:latest
    container_name: visiotech-pokemon-seq
    environment:
      ACCEPT_EULA: "Y"
      SEQ_FIRSTRUN_ADMINPASSWORD: "ChangeMe123!"
    ports:
      - "5341:80"
    volumes:
      - seq_data:/data
    networks:
      - backend

networks:
  backend:
    driver: bridge

volumes:
  postgres_data:
  seq_data:
```

## 8. Explicacion detallada del compose

## 8.1 Servicio `api`

El servicio `api` construye la aplicacion desde el codigo fuente y la ejecuta en un contenedor propio.

Puntos clave:

- usa un `Dockerfile` del proyecto host
- publica el puerto `8080`
- se conecta a `postgres` usando el nombre del servicio como hostname
- envia logs a `seq` usando el nombre del servicio como hostname

La cadena de conexion no usa `localhost` dentro del contenedor, porque en Docker cada servicio resuelve al otro por nombre de red.

Correcto:

- `Host=postgres`
- `Serilog__SeqUrl=http://seq:5341`

Incorrecto dentro del contenedor:

- `Host=localhost`
- `http://localhost:5341`

## 8.2 Servicio `postgres`

El servicio `postgres` ejecuta el motor de base de datos del sistema.

Puntos clave:

- expone el puerto `5432`
- persiste datos en el volumen `postgres_data`
- no depende de la API para arrancar

El volumen es obligatorio para evitar perdida de datos cuando el contenedor se recrea.

## 8.3 Servicio `seq`

El servicio `seq` proporciona la interfaz web de consulta de logs.

Puntos clave:

- expone su interfaz en el host
- persiste informacion en `seq_data`
- recibe eventos de log estructurados por HTTP

En el ejemplo:

- dentro de la red interna la API habla con `http://seq:5341`
- desde el navegador del desarrollador se accede a Seq por el puerto publicado del host

## 8.4 Red `backend`

La red `backend` permite que los tres servicios se descubran entre si sin exponer trafico interno de forma innecesaria.

Beneficios:

- aislamiento respecto a otros stacks Docker
- resolucion de nombres entre contenedores
- topologia repetible y facil de entender

## 8.5 Volumenes

La propuesta incluye dos volumenes persistentes:

- `postgres_data`
- `seq_data`

Su objetivo es mantener estado entre reinicios:

- `postgres_data` conserva base de datos
- `seq_data` conserva configuracion e historial de Seq

## 9. Logging con Serilog y Seq

La estrategia de logging recomendada es:

- `Serilog` como proveedor principal
- salida a consola para consumo inmediato del runtime
- salida a `Seq` para exploracion estructurada

La configuracion esperada debe cubrir:

- `MinimumLevel`
- enriquecimiento con contexto
- `Console` sink
- `Seq` sink

Ejemplo conceptual de configuracion:

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(configuration["Serilog:SeqUrl"]!)
    .CreateLogger();
```

Informacion valiosa para enviar a logs:

- nombre del endpoint
- identificador de correlacion
- duracion de la peticion
- excepciones
- resultados de validacion cuando tenga sentido
- operaciones criticas de negocio

No deben registrarse en claro:

- passwords
- secretos
- connection strings completas
- datos sensibles innecesarios

## 10. Orden de arranque y dependencias

Aunque `depends_on` ayuda a expresar relacion entre servicios, no garantiza por si solo que PostgreSQL o Seq ya esten listos para uso funcional.

Por eso la API debe ser robusta frente a arranque parcial:

- reintentos razonables de conexion si se incorporan
- migraciones controladas al inicio
- logs claros cuando una dependencia aun no responde

En el estado actual del proyecto, el comportamiento correcto es:

- la API intenta iniciar
- la base de datos debe estar disponible para que las migraciones o `EnsureCreated` funcionen
- los fallos de arranque deben quedar reflejados en logs

## 11. Seguridad y endurecimiento minimo

Para un entorno local o MVP, el compose puede usar credenciales simples. Aun asi, la documentacion del backend debe dejar claro que en entornos compartidos o productivos se recomienda:

- no dejar passwords por defecto
- mover secretos a variables de entorno seguras o gestores de secretos
- no publicar puertos que no hagan falta
- proteger Seq con password fuerte
- limitar el acceso externo a PostgreSQL

Practicas recomendadas:

- publicar `5432` solo si el equipo necesita conectarse desde el host
- cambiar `SEQ_FIRSTRUN_ADMINPASSWORD`
- no hardcodear credenciales de produccion en el repositorio

## 12. Beneficios de esta composicion

Esta estructura de backend aporta varias ventajas:

- separa claramente aplicacion, datos y observabilidad
- facilita desarrollo local y debugging
- deja una base limpia para evolucion futura
- permite probar la API en un entorno muy cercano al real
- refuerza buenas practicas operativas sin sobredisenar el MVP

## 13. Evolucion futura razonable

Esta composicion no cierra crecimiento futuro. Al contrario, deja una base limpia para evolucionar hacia:

- reverse proxy delante de la API
- multiples instancias de API
- health checks dedicados
- pipelines de migracion separados
- exportacion de logs a otras plataformas
- despliegues en Kubernetes o entornos gestionados

La clave es que el MVP ya nace con fronteras correctas:

- API como servicio aislado
- base de datos como dependencia externa
- observabilidad como componente explicito

## 14. Resumen

El backend quedara conformado por:

- un contenedor de `API` .NET
- un contenedor de `PostgreSQL`
- un contenedor de `Seq`
- un `docker compose` que los orquesta

La API persistira en PostgreSQL mediante EF Core y emitira logs estructurados con Serilog hacia consola y Seq.

Esta topologia es adecuada para el MVP porque mantiene simplicidad operativa, buena separacion de responsabilidades y una base solida para evolucionar el sistema sin rehacer su estructura de despliegue.
