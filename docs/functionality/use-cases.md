# Análisis funcional y casos de uso del MVP Pokémon

## 1. Objetivo del documento

Este documento describe los casos de uso funcionales implementados en la API del MVP Pokémon y mantiene la trazabilidad con los requisitos iniciales del fichero `docs/requirements/POKÉMON 2.pdf`.

Su objetivo es servir como contrato común para producto, backend, QA y documentación técnica. Por tanto, el contenido refleja el comportamiento actual del código, no solo la intención inicial del requisito.

El alcance se apoya en:

- el PDF de requisitos
- el catálogo MVP de 10 especies Pokémon
- el catálogo MVP de 27 movimientos
- la API HTTP implementada en `src/Api/Visiotech.Pokemon.Api`
- las reglas de dominio implementadas en `src/Api/Visiotech.Pokemon.Domain`
- los casos de aplicación implementados en `src/Api/Visiotech.Pokemon.Application`

## 2. Estado funcional actual

El MVP implementa tres bloques funcionales:

1. `Pokédex API`: CRUD y consultas sobre especies base, movimientos, relaciones aprendibles y la colección de "Mis Pokémon".
2. `Motor de daño`: cálculo de daño entre dos instancias jugables usando la fórmula del PDF, categoría del movimiento, tabla normativa de efectividad y factor aleatorio.
3. `Combate`: creación de partidas entre dos `Mis Pokémon`, ejecución de fases por turno, actualización de PS, finalización por KO e histórico trazable.

Todos los casos de uso `UC-01` a `UC-22` descritos en este documento están cubiertos por endpoints y handlers de aplicación.

## 3. Lectura funcional del requisito original

### 3.1 Cálculo de daño

El requisito exige calcular el daño de un movimiento usando:

- nivel del atacante
- estadísticas del atacante y del defensor
- poder del movimiento
- efectividad por tipo
- factor aleatorio entero entre `85` y `100`

La fórmula base del PDF es:

```txt
Daño(PS) = {[(2 * Nivel / 5 + 2) * AtaqueDelAtacante * PoderDelMovimiento / DefensaDelRival] / 50} * Efectividad * Random / 100
```

La implementación actual concreta esa fórmula así:

- si el movimiento es `Physical`, usa `Attack` del atacante y `Defense` del defensor
- si el movimiento es `Special`, usa `SpecialAttack` del atacante y `SpecialDefense` del defensor
- si el movimiento es `Status`, no se permite calcular daño
- la efectividad sale exclusivamente de la tabla normativa de la sección `6.1`
- si el defensor tiene dos tipos, se multiplican ambos coeficientes
- el daño bruto se redondea hacia abajo con `Floor`
- el daño aplicado se limita para no dejar los PS del defensor por debajo de `0`

### 3.2 API tipo Pokédex

La API cubre:

- especies base Pokémon
- movimientos
- movimientos aprendibles por especie
- instancias jugables de `Mi Pokémon`
- movimientos equipados de una instancia
- consulta de especies que pueden aprender un movimiento

### 3.3 Combate

La API mantiene partidas entre exactamente dos `Mis Pokémon`.

Una partida:

- nace con estado `Created`
- empieza en el turno `1`
- asigna como primer atacante al primer `Mi Pokémon` recibido
- avanza a `InProgress` tras la primera fase no finalizante
- pasa a `Finished` cuando un defensor queda a `0` PS
- registra ganador, perdedor e histórico de fases
- rechaza nuevas fases cuando está finalizada

## 4. Decisiones funcionales implementadas

### 4.1 Tipos simples y tipos duales

El dominio soporta especies con `1..2` tipos.

Esto permite modelar correctamente especies como:

- `Charizard`: `Fire`, `Flying`
- `Venusaur`: `Grass`, `Poison`
- `Gengar`: `Ghost`, `Poison`
- `Golem`: `Rock`, `Ground`
- `Dragonite`: `Dragon`, `Flying`

### 4.2 Tabla de efectividad normativa

La tabla de la sección `6.1` es un contrato funcional explícito.

La implementación la materializa en `PokemonTypeEffectivenessChart` y todos los cálculos de daño la usan como única fuente de verdad.

### 4.3 Categorías de movimiento

El sistema soporta tres categorías:

- `Physical`
- `Special`
- `Status`

Reglas actuales:

- `Physical` requiere poder mayor que `0` y usa estadísticas físicas.
- `Special` requiere poder mayor que `0` y usa estadísticas especiales.
- `Status` requiere poder `0` y no puede usarse para calcular daño ni ejecutar una fase de combate que requiera daño.

### 4.4 Alcance no implementado

Quedan fuera del MVP:

- PP de movimientos
- precisión y fallo
- golpes críticos
- STAB
- estados alterados
- objetos
- habilidades con efecto de combate
- cambios temporales de estadísticas
- equipos de más de un Pokémon por jugador

El combate implementado es determinista salvo por el factor aleatorio `85..100`.

## 5. Modelo funcional del dominio

### 5.1 Pokémon base

Representa una especie del catálogo.

Campos funcionales:

- `Id`
- `Name`
- `Types`
- `BaseStats.Health`
- `BaseStats.Attack`
- `BaseStats.Defense`
- `BaseStats.SpecialAttack`
- `BaseStats.SpecialDefense`
- `BaseStats.Speed`
- movimientos aprendibles asociados

Restricciones principales:

- nombre obligatorio y único
- entre uno y dos tipos
- tipos sin duplicados
- estadísticas base mayores que `0`

### 5.2 Movimiento

Representa una acción disponible en el catálogo.

Campos funcionales:

- `Id`
- `Name`
- `Type`
- `Category`
- `Power`

Restricciones principales:

- nombre obligatorio, único y con longitud máxima de `100`
- tipo válido
- categoría válida
- poder coherente con la categoría

### 5.3 Mi Pokémon

Representa una instancia jugable basada en una especie.

Campos funcionales:

- `Id`
- especie base
- nivel
- PS actuales
- PS totales
- movimientos equipados

Restricciones principales:

- referencia a especie existente
- nivel entre `1` y `100`
- PS actuales mayores o iguales que `0`
- PS totales mayores que `0`
- PS actuales menores o iguales que PS totales
- entre `1` y `4` movimientos equipados
- movimientos equipados sin duplicados
- movimientos equipados aprendibles por la especie

### 5.4 Partida de combate

Representa un enfrentamiento entre dos `Mis Pokémon`.

Campos funcionales:

- `Id`
- `Status`
- `CurrentTurnNumber`
- `NextAttackerMyPokemonId`
- `WinnerMyPokemonId`
- `LoserMyPokemonId`
- combatientes
- histórico de fases

Estados:

- `Created`
- `InProgress`
- `Finished`

### 5.5 Fase de combate

Representa una acción de combate registrada.

Campos funcionales:

- número de secuencia
- atacante
- defensor
- movimiento usado
- nombre del movimiento
- factor aleatorio
- desglose de efectividad por tipo defensor
- efectividad total
- daño aplicado
- PS restantes del atacante
- PS restantes del defensor

## 6. Reglas de negocio del MVP

### 6.1 Tabla de efectividad normativa

La siguiente matriz transcribe funcionalmente la tabla del apartado `Efectividad` del PDF.

- Las filas representan el tipo del movimiento atacante.
- Las columnas representan el tipo del Pokémon defensor.
- Los únicos coeficientes base válidos son `0`, `0.5`, `1` y `2`.
- Si el defensor tiene dos tipos, la efectividad total se calcula multiplicando ambos coeficientes base.

| Tipo atacante \ Tipo defensor | Acero | Agua | Bicho | Dragón | Eléctrico | Fantasma | Fuego | Hada | Hielo | Lucha | Normal | Planta | Psíquico | Roca | Siniestro | Tierra | Veneno | Volador |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Acero | 0.5 | 0.5 | 1 | 1 | 0.5 | 1 | 0.5 | 2 | 2 | 1 | 1 | 1 | 1 | 2 | 1 | 1 | 1 | 1 |
| Agua | 1 | 0.5 | 1 | 0.5 | 1 | 1 | 2 | 1 | 1 | 1 | 1 | 0.5 | 1 | 2 | 1 | 2 | 1 | 1 |
| Bicho | 0.5 | 1 | 1 | 1 | 1 | 0.5 | 0.5 | 0.5 | 1 | 0.5 | 1 | 2 | 2 | 1 | 2 | 1 | 0.5 | 0.5 |
| Dragón | 0.5 | 1 | 1 | 2 | 1 | 1 | 1 | 0 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 |
| Eléctrico | 1 | 2 | 1 | 0.5 | 0.5 | 1 | 1 | 1 | 1 | 1 | 1 | 0.5 | 1 | 1 | 1 | 0 | 1 | 2 |
| Fantasma | 1 | 1 | 1 | 1 | 1 | 2 | 1 | 1 | 1 | 1 | 0 | 1 | 2 | 1 | 0.5 | 1 | 1 | 1 |
| Fuego | 2 | 0.5 | 2 | 0.5 | 1 | 1 | 0.5 | 1 | 2 | 1 | 1 | 2 | 1 | 0.5 | 1 | 1 | 1 | 1 |
| Hada | 0.5 | 1 | 1 | 2 | 1 | 1 | 0.5 | 1 | 1 | 2 | 1 | 1 | 1 | 1 | 2 | 1 | 0.5 | 1 |
| Hielo | 0.5 | 0.5 | 1 | 2 | 1 | 1 | 0.5 | 1 | 0.5 | 1 | 1 | 2 | 1 | 1 | 1 | 2 | 1 | 2 |
| Lucha | 2 | 1 | 0.5 | 1 | 1 | 0 | 1 | 0.5 | 2 | 1 | 2 | 1 | 0.5 | 2 | 2 | 1 | 0.5 | 0.5 |
| Normal | 0.5 | 1 | 1 | 1 | 1 | 0 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 0.5 | 1 | 1 | 1 | 1 |
| Planta | 0.5 | 2 | 0.5 | 0.5 | 1 | 1 | 0.5 | 1 | 1 | 1 | 1 | 0.5 | 1 | 2 | 1 | 2 | 0.5 | 0.5 |
| Psíquico | 0.5 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 1 | 2 | 1 | 1 | 0.5 | 1 | 0 | 1 | 2 | 1 |
| Roca | 0.5 | 1 | 2 | 1 | 1 | 1 | 2 | 1 | 2 | 0.5 | 1 | 1 | 1 | 1 | 1 | 0.5 | 1 | 2 |
| Siniestro | 1 | 1 | 1 | 1 | 1 | 2 | 1 | 0.5 | 1 | 0.5 | 1 | 1 | 2 | 1 | 0.5 | 1 | 1 | 1 |
| Tierra | 2 | 1 | 0.5 | 1 | 2 | 1 | 2 | 1 | 1 | 1 | 1 | 0.5 | 1 | 2 | 1 | 1 | 2 | 0 |
| Veneno | 0 | 1 | 1 | 1 | 1 | 0.5 | 1 | 2 | 1 | 1 | 1 | 2 | 1 | 0.5 | 1 | 0.5 | 0.5 | 1 |
| Volador | 0.5 | 1 | 2 | 1 | 0.5 | 1 | 1 | 1 | 1 | 2 | 1 | 2 | 1 | 0.5 | 1 | 1 | 1 | 1 |

### 6.2 Valores HTTP reales para tipos y categorías

Aunque la tabla anterior está documentada en español para mantener la lectura funcional del PDF, la API recibe y devuelve los tipos con los nombres del enum en inglés.

Tipos aceptados por la API:

- `Bug`
- `Dark`
- `Dragon`
- `Electric`
- `Fairy`
- `Fighting`
- `Fire`
- `Flying`
- `Ghost`
- `Grass`
- `Ground`
- `Ice`
- `Normal`
- `Poison`
- `Psychic`
- `Rock`
- `Steel`
- `Water`

Categorías aceptadas por la API:

- `Physical`
- `Special`
- `Status`

La validación no distingue mayúsculas de minúsculas, pero las respuestas se devuelven con el nombre canónico del enum.

### 6.3 Reglas de catálogo

- `BR-01`: no puede existir más de un Pokémon base con el mismo nombre canónico.
- `BR-02`: no puede existir más de un movimiento con el mismo nombre canónico.
- `BR-03`: un Pokémon base debe tener las seis estadísticas base informadas y con valor mayor que `0`.
- `BR-04`: un Pokémon base debe tener al menos un tipo y como máximo dos; todos los tipos deben pertenecer al catálogo cerrado.
- `BR-05`: un movimiento debe tener tipo y categoría válidos. Si es `Physical` o `Special`, su poder debe ser mayor que `0`. Si es `Status`, su poder debe ser `0`.
- `BR-06`: la relación `Pokémon base -> movimientos aprendibles` no puede contener duplicados.

### 6.4 Reglas de Mis Pokémon

- `BR-07`: un `Mi Pokémon` debe referenciar una especie existente.
- `BR-08`: un `Mi Pokémon` debe tener nivel entre `1` y `100`.
- `BR-09`: un `Mi Pokémon` debe cumplir `0 <= PS actuales <= PS totales` y `PS totales > 0`.
- `BR-10`: un `Mi Pokémon` debe tener entre `1` y `4` movimientos equipados.
- `BR-11`: todo movimiento equipado debe pertenecer a la lista de movimientos aprendibles de su especie.
- `BR-12`: un `Mi Pokémon` no puede equipar el mismo movimiento dos veces.

### 6.5 Reglas de cálculo de daño

- `BR-13`: el factor `Random` es un entero entre `85` y `100`, ambos inclusive.
- `BR-14`: la efectividad base de un movimiento se obtiene exclusivamente de la matriz normativa.
- `BR-15`: si el defensor tiene dos tipos, la efectividad total se calcula multiplicando ambos coeficientes base.
- `BR-16`: si la efectividad total es `0`, el daño final es `0`.
- `BR-17`: el daño aplicado no puede reducir los PS por debajo de `0`.
- `BR-18`: para el MVP no se consideran críticos, STAB, precisión, evasión ni estados.

### 6.6 Reglas de combate

- `BR-19`: una partida solo puede crearse con exactamente dos `Mis Pokémon` distintos.
- `BR-20`: ninguno de los dos `Mis Pokémon` puede iniciar una partida con PS actuales en `0`.
- `BR-21`: una fase de combate debe dejar trazabilidad del atacante, defensor, movimiento, random, efectividad base, efectividad total, daño y PS restantes.
- `BR-22`: la partida termina cuando los PS actuales de uno de los combatientes llegan a `0`.
- `BR-23`: una vez finalizada la partida no se pueden ejecutar más fases.
- `BR-24`: el atacante de una fase debe pertenecer a la partida y coincidir con `NextAttackerMyPokemonId`.
- `BR-25`: el movimiento usado en una fase debe estar equipado por el atacante.

## 7. Catálogo MVP implementado

### 7.1 Especies

El seed MVP contiene 10 especies con identificadores estables:

| Especie | Tipos | HP | Attack | Defense | SpecialAttack | SpecialDefense | Speed |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `Charizard` | `Fire`, `Flying` | 78 | 84 | 78 | 109 | 85 | 100 |
| `Blastoise` | `Water` | 79 | 83 | 100 | 85 | 105 | 78 |
| `Venusaur` | `Grass`, `Poison` | 80 | 82 | 83 | 100 | 100 | 80 |
| `Pikachu` | `Electric` | 35 | 55 | 40 | 50 | 50 | 90 |
| `Gengar` | `Ghost`, `Poison` | 60 | 65 | 60 | 130 | 75 | 110 |
| `Golem` | `Rock`, `Ground` | 80 | 120 | 130 | 55 | 65 | 45 |
| `Alakazam` | `Psychic` | 55 | 50 | 45 | 135 | 95 | 120 |
| `Machamp` | `Fighting` | 90 | 130 | 80 | 65 | 85 | 55 |
| `Dragonite` | `Dragon`, `Flying` | 91 | 134 | 95 | 100 | 100 | 80 |
| `Snorlax` | `Normal` | 160 | 110 | 65 | 65 | 110 | 30 |

### 7.2 Movimientos

El seed MVP contiene 27 movimientos:

| Movimiento | Tipo | Categoría | Poder |
| --- | --- | --- | --- |
| `Flamethrower` | `Fire` | `Special` | 90 |
| `Fire Blast` | `Fire` | `Special` | 110 |
| `Fly` | `Flying` | `Physical` | 90 |
| `Hyper Beam` | `Normal` | `Special` | 150 |
| `Solar Beam` | `Grass` | `Special` | 120 |
| `Surf` | `Water` | `Special` | 90 |
| `Hydro Pump` | `Water` | `Special` | 110 |
| `Ice Beam` | `Ice` | `Special` | 90 |
| `Sleep Powder` | `Grass` | `Status` | 0 |
| `Seed Bomb` | `Grass` | `Physical` | 80 |
| `Sludge Wave` | `Poison` | `Special` | 95 |
| `Thunderbolt` | `Electric` | `Special` | 90 |
| `Discharge` | `Electric` | `Special` | 80 |
| `Shadow Ball` | `Ghost` | `Special` | 80 |
| `Dark Pulse` | `Dark` | `Special` | 80 |
| `Poison Jab` | `Poison` | `Physical` | 80 |
| `Psychic` | `Psychic` | `Special` | 90 |
| `Close Combat` | `Fighting` | `Physical` | 120 |
| `Drain Punch` | `Fighting` | `Physical` | 75 |
| `Earthquake` | `Ground` | `Physical` | 100 |
| `Bulldoze` | `Ground` | `Physical` | 60 |
| `Protect` | `Normal` | `Status` | 0 |
| `Air Slash` | `Flying` | `Special` | 75 |
| `Thunder Punch` | `Electric` | `Physical` | 75 |
| `Ice Punch` | `Ice` | `Physical` | 75 |
| `Body Slam` | `Normal` | `Physical` | 85 |
| `Rest` | `Psychic` | `Status` | 0 |

### 7.3 Carga del seed

La carga del roster MVP depende de `Seed__ApplyMvpRoster`.

Si está activada, el inicializador inserta:

- especies si el catálogo de especies está vacío
- movimientos si el catálogo de movimientos está vacío
- relaciones aprendibles si el catálogo de relaciones está vacío

## 8. Contratos HTTP implementados

### 8.1 Catálogo de Pokémon base

| Caso de uso | Método y ruta | Respuesta principal |
| --- | --- | --- |
| `UC-01` | `POST /api/v1/pokemons` | `201 Created` con `PokemonSpeciesContract` |
| `UC-02` | `GET /api/v1/pokemons` | `200 OK` con `PokemonSpeciesCatalogContract` |
| `UC-02` | `GET /api/v1/pokemons/{id}` | `200 OK` con `PokemonSpeciesContract` |
| `UC-03` | `PUT /api/v1/pokemons/{id}` | `200 OK` con `PokemonSpeciesContract` |
| `UC-04` | `DELETE /api/v1/pokemons/{id}` | `204 No Content` |
| `UC-09` | `PUT /api/v1/pokemons/{id}/learnable-moves` | `200 OK` con `PokemonLearnableMovesContract` |
| `UC-10` | `GET /api/v1/pokemons/{id}/learnable-moves` | `200 OK` con `PokemonLearnableMovesContract` |

Filtros de listado:

- `name`
- `type`
- `page`, por defecto `1`
- `pageSize`, por defecto `20`, máximo `100`

### 8.2 Catálogo de movimientos

| Caso de uso | Método y ruta | Respuesta principal |
| --- | --- | --- |
| `UC-05` | `POST /api/v1/moves` | `201 Created` con `PokemonMoveContract` |
| `UC-06` | `GET /api/v1/moves` | `200 OK` con `PokemonMoveCatalogContract` |
| `UC-06` | `GET /api/v1/moves/{id}` | `200 OK` con `PokemonMoveContract` |
| `UC-07` | `PUT /api/v1/moves/{id}` | `200 OK` con `PokemonMoveContract` |
| `UC-08` | `DELETE /api/v1/moves/{id}` | `204 No Content` |
| `UC-11` | `GET /api/v1/moves/{id}/pokemon-species` | `200 OK` con `PokemonMoveSharedSpeciesContract` |

Filtros de listado:

- `name`
- `type`
- `category`
- `page`, por defecto `1`
- `pageSize`, por defecto `20`, máximo `100`

### 8.3 Mis Pokémon

| Caso de uso | Método y ruta | Respuesta principal |
| --- | --- | --- |
| `UC-12` | `POST /api/v1/my-pokemons` | `201 Created` con `MyPokemonContract` |
| `UC-13` | `GET /api/v1/my-pokemons` | `200 OK` con `MyPokemonCatalogContract` |
| `UC-13` | `GET /api/v1/my-pokemons/{id}` | `200 OK` con `MyPokemonContract` |
| `UC-14` | `PUT /api/v1/my-pokemons/{id}` | `200 OK` con `MyPokemonContract` |
| `UC-15` | `DELETE /api/v1/my-pokemons/{id}` | `204 No Content` |
| `UC-16` | `GET /api/v1/my-pokemons/{id}/equipped-moves` | `200 OK` con `MyPokemonEquippedMovesContract` |

Filtros de listado:

- `page`, por defecto `1`
- `pageSize`, por defecto `20`, máximo `100`

### 8.4 Daño y combate

| Caso de uso | Método y ruta | Respuesta principal |
| --- | --- | --- |
| `UC-17` | `POST /api/v1/damage-calculations` | `200 OK` con `MoveDamageCalculationContract` |
| `UC-18` | `POST /api/v1/battles` | `201 Created` con `BattleContract` |
| `UC-19` | `GET /api/v1/battles/{id}` | `200 OK` con `BattleContract` |
| `UC-20` | `POST /api/v1/battles/{id}/phases` | `200 OK` con `BattlePhaseExecutionContract` |
| `UC-22` | `GET /api/v1/battles/{id}/phases` | `200 OK` con `BattleHistoryContract` |

`UC-21` no tiene endpoint propio porque se ejecuta automáticamente dentro de `UC-20`.

### 8.5 Errores HTTP

La API devuelve errores explícitos:

- `400 Bad Request` con `HttpValidationProblemDetails` para errores de validación funcional
- `404 Not Found` con `ProblemDetails` cuando el recurso no existe
- `409 Conflict` con `ProblemDetails` para conflictos de unicidad
- `500 Internal Server Error` para errores no controlados

Los errores de validación incluyen claves concretas como:

- `name`
- `types`
- `baseStats.health`
- `type`
- `category`
- `power`
- `equippedMoveIds`
- `attackerMyPokemonId`
- `moveId`
- `dependencies`

## 9. Casos de uso funcionales

### UC-01 Crear Pokémon base

Objetivo: dar de alta una especie del catálogo.

Endpoint:

```txt
POST /api/v1/pokemons
```

Entrada:

- `name`
- `types`
- `baseStats`

Reglas aplicables:

- `BR-01`
- `BR-03`
- `BR-04`

Errores esperados:

- nombre vacío
- nombre duplicado
- tipos vacíos, inválidos, duplicados o más de dos
- estadísticas menores o iguales que `0`

### UC-02 Consultar listado y detalle de Pokémon base

Objetivo: recuperar especies del catálogo.

Endpoints:

```txt
GET /api/v1/pokemons
GET /api/v1/pokemons/{id}
```

El listado soporta búsqueda por nombre, filtro por tipo y paginación.

El detalle devuelve:

- identificador estable
- nombre
- tipos
- estadísticas base

### UC-03 Actualizar Pokémon base

Objetivo: modificar datos canónicos de una especie.

Endpoint:

```txt
PUT /api/v1/pokemons/{id}
```

Reglas aplicables:

- `BR-01`
- `BR-03`
- `BR-04`

La actualización modifica la especie base, no las instancias ya existentes salvo por las lecturas que referencian la especie actual.

### UC-04 Eliminar Pokémon base

Objetivo: retirar una especie del catálogo cuando no tiene dependencias persistidas.

Endpoint:

```txt
DELETE /api/v1/pokemons/{id}
```

La estrategia actual es borrado físico protegido por comprobación de dependencias.

Se rechaza si la especie está referenciada por:

- instancias de `Mi Pokémon`
- relaciones aprendibles u otros datos persistidos que impidan mantener integridad

### UC-05 Crear movimiento

Objetivo: registrar un movimiento disponible para aprendizaje y combate.

Endpoint:

```txt
POST /api/v1/moves
```

Entrada:

- `name`
- `type`
- `category`
- `power`

Reglas aplicables:

- `BR-02`
- `BR-05`

### UC-06 Consultar listado y detalle de movimientos

Objetivo: consultar el catálogo de movimientos.

Endpoints:

```txt
GET /api/v1/moves
GET /api/v1/moves/{id}
```

El listado soporta búsqueda por nombre, filtro por tipo, filtro por categoría y paginación.

### UC-07 Actualizar movimiento

Objetivo: modificar un movimiento del catálogo.

Endpoint:

```txt
PUT /api/v1/moves/{id}
```

Reglas aplicables:

- `BR-02`
- `BR-05`

### UC-08 Eliminar movimiento

Objetivo: retirar un movimiento del catálogo cuando no tiene dependencias persistidas.

Endpoint:

```txt
DELETE /api/v1/moves/{id}
```

La estrategia actual es borrado físico protegido por comprobación de dependencias.

Se rechaza si el movimiento está referenciado por:

- movimientos aprendibles de especies
- movimientos equipados por `Mis Pokémon`
- fases o histórico de combate que dependan de él

### UC-09 Asociar movimientos aprendibles a un Pokémon base

Objetivo: definir qué movimientos puede aprender una especie.

Endpoint:

```txt
PUT /api/v1/pokemons/{id}/learnable-moves
```

Entrada:

- `addMoveIds`
- `removeMoveIds`

Reglas aplicables:

- `BR-06`

Validaciones:

- debe existir la especie
- debe indicarse al menos un movimiento para añadir o retirar
- los identificadores no pueden estar vacíos
- no puede haber duplicados en la misma colección
- no se puede añadir y retirar el mismo movimiento en la misma petición
- no se puede añadir una relación ya existente
- no se puede retirar una relación inexistente

### UC-10 Consultar movimientos posibles de un Pokémon base

Objetivo: recuperar el conjunto de movimientos que una especie puede aprender.

Endpoint:

```txt
GET /api/v1/pokemons/{id}/learnable-moves
```

La fuente de verdad es la relación `PokemonSpecies -> PokemonLearnableMove -> PokemonMove`.

### UC-11 Consultar Pokémon que comparten un mismo movimiento

Objetivo: obtener las especies que pueden aprender un movimiento determinado.

Endpoint:

```txt
GET /api/v1/moves/{id}/pokemon-species
```

La respuesta incluye:

- identificador del movimiento
- nombre del movimiento
- especies que lo tienen como aprendible

### UC-12 Crear Mi Pokémon

Objetivo: registrar una instancia jugable basada en una especie.

Endpoint:

```txt
POST /api/v1/my-pokemons
```

Entrada:

- `pokemonSpeciesId`
- `level`
- `currentHealthPoints`
- `totalHealthPoints`
- `equippedMoveIds`

Reglas aplicables:

- `BR-07`
- `BR-08`
- `BR-09`
- `BR-10`
- `BR-11`
- `BR-12`

La creación valida que todos los movimientos equipados existan y sean aprendibles por la especie.

### UC-13 Consultar Mis Pokémon

Objetivo: listar y consultar el detalle de las instancias jugables.

Endpoints:

```txt
GET /api/v1/my-pokemons
GET /api/v1/my-pokemons/{id}
```

El detalle devuelve:

- identificador de la instancia
- especie base completa
- nivel
- PS actuales
- PS totales
- movimientos equipados

### UC-14 Actualizar Mi Pokémon

Objetivo: modificar nivel, PS o movimientos equipados de una instancia jugable.

Endpoint:

```txt
PUT /api/v1/my-pokemons/{id}
```

Reglas aplicables:

- `BR-08`
- `BR-09`
- `BR-10`
- `BR-11`
- `BR-12`

Riesgo mitigado:

- la actualización afecta solo a la instancia jugable, no a la especie base.

### UC-15 Eliminar Mi Pokémon

Objetivo: retirar una instancia jugable cuando no compromete partidas ni histórico persistido.

Endpoint:

```txt
DELETE /api/v1/my-pokemons/{id}
```

La estrategia actual es borrado físico protegido por comprobación de dependencias.

Se rechaza si el `Mi Pokémon` participa en una partida activa o si cualquier referencia persistida, incluida una referencia de combate, impide eliminarlo con integridad.

### UC-16 Consultar movimientos equipados de Mi Pokémon

Objetivo: obtener los movimientos actualmente equipados por una instancia jugable.

Endpoint:

```txt
GET /api/v1/my-pokemons/{id}/equipped-moves
```

La respuesta devuelve los movimientos equipados de la instancia, no los movimientos aprendibles de la especie.

### UC-17 Calcular daño de un movimiento

Objetivo: calcular el daño que un movimiento causaría desde un atacante hacia un defensor.

Endpoint:

```txt
POST /api/v1/damage-calculations
```

Entrada:

- `attackerMyPokemonId`
- `defenderMyPokemonId`
- `moveId`

Reglas aplicables:

- `BR-13`
- `BR-14`
- `BR-15`
- `BR-16`
- `BR-17`
- `BR-18`
- `BR-25`

Validaciones:

- atacante existente
- defensor existente
- movimiento existente
- atacante con PS mayores que `0`
- defensor con PS mayores que `0`
- movimiento equipado por el atacante
- movimiento no `Status`

Salida trazable:

- atacante
- defensor
- movimiento
- tipo y categoría del movimiento
- nivel del atacante
- poder del movimiento
- estadística ofensiva usada
- estadística defensiva usada
- PS actuales del defensor
- factor aleatorio
- daño base
- desglose de efectividad por tipo defensor
- efectividad total
- daño bruto
- daño aplicado
- PS restantes del defensor

### UC-18 Crear partida de combate

Objetivo: crear una nueva partida entre exactamente dos `Mis Pokémon`.

Endpoint:

```txt
POST /api/v1/battles
```

Entrada:

- `firstMyPokemonId`
- `secondMyPokemonId`

Reglas aplicables:

- `BR-19`
- `BR-20`

Estado inicial:

- `Status = Created`
- `CurrentTurnNumber = 1`
- `NextAttackerMyPokemonId = firstMyPokemonId`
- dos combatientes con snapshot de PS iniciales
- histórico vacío

### UC-19 Consultar estado de la partida

Objetivo: recuperar el estado actual de un combate.

Endpoint:

```txt
GET /api/v1/battles/{id}
```

La respuesta incluye:

- identificador estable
- estado
- turno actual
- siguiente atacante, si aplica
- ganador, si aplica
- perdedor, si aplica
- combatientes
- histórico registrado

### UC-20 Ejecutar una fase de combate

Objetivo: avanzar una partida una fase aplicando un movimiento.

Endpoint:

```txt
POST /api/v1/battles/{id}/phases
```

Entrada:

- `attackerMyPokemonId`
- `moveId`

Reglas aplicables:

- `BR-17`
- `BR-21`
- `BR-22`
- `BR-23`
- `BR-24`
- `BR-25`

Validaciones:

- la partida existe
- la partida no está finalizada
- el atacante pertenece a la partida
- el atacante coincide con `NextAttackerMyPokemonId`
- el atacante tiene PS mayores que `0`
- el defensor tiene PS mayores que `0`
- el movimiento está equipado por el atacante

Efectos:

- invoca el servicio reutilizable de cálculo de daño
- registra una fase con el número `CurrentTurnNumber`
- actualiza PS del combatiente defensor dentro de la partida
- sincroniza los PS actuales de las instancias `Mi Pokémon`
- alterna el siguiente atacante si el combate continúa
- finaliza la partida si el defensor queda a `0` PS

### UC-21 Finalizar partida por KO

Objetivo: cerrar automáticamente la partida cuando un combatiente queda a `0` PS.

Este caso de uso se ejecuta dentro del flujo de `UC-20`.

Efectos:

- `Status = Finished`
- `NextAttackerMyPokemonId = null`
- `WinnerMyPokemonId = atacante`
- `LoserMyPokemonId = defensor`
- se impiden nuevas fases

### UC-22 Consultar histórico de fases de combate

Objetivo: reconstruir lo ocurrido en una partida.

Endpoint:

```txt
GET /api/v1/battles/{id}/phases
```

La respuesta devuelve fases ordenadas por `SequenceNumber`.

Cada fase incluye:

- atacante
- defensor
- movimiento
- nombre del movimiento
- random
- efectividad por tipo defensor
- efectividad total
- daño
- PS restantes del atacante
- PS restantes del defensor

## 10. Escenarios funcionales mínimos

### 10.1 Escenario de catálogo

1. Crear o sembrar los 10 Pokémon base del MVP.
2. Crear o sembrar los 27 movimientos del MVP.
3. Asociar movimientos aprendibles a cada especie.
4. Consultar qué Pokémon pueden aprender `Protect`.
5. Consultar movimientos posibles de `Blastoise`.

### 10.2 Escenario de Mis Pokémon

1. Crear un `Mi Pokémon` basado en `Charizard`.
2. Equiparle exactamente 4 movimientos válidos.
3. Consultar sus movimientos equipados con `GET /api/v1/my-pokemons/{id}/equipped-moves`.
4. Intentar equipar un quinto movimiento y recibir `400 Bad Request`.
5. Intentar equipar un movimiento no aprendible y recibir `400 Bad Request`.

### 10.3 Escenario de cálculo de daño con ventaja simple

1. Crear o usar `Pikachu` como atacante.
2. Equipar `Thunderbolt`.
3. Crear o usar `Blastoise` como defensor.
4. Calcular daño.
5. Verificar que `Electric -> Water = 2`.
6. Verificar que la respuesta incluye trazabilidad de random, estadísticas usadas y efectividad.

### 10.4 Escenario de resistencia doble e inmunidad

1. Calcular un movimiento `Grass` contra `Charizard`.
2. Verificar efectividad total `0.25`, resultado de `Grass -> Fire = 0.5` y `Grass -> Flying = 0.5`.
3. Calcular un movimiento `Electric` contra `Golem`.
4. Verificar que `Electric -> Ground = 0`.
5. Verificar daño `0` aunque el otro tipo del defensor no sea inmune.

### 10.5 Escenario de combate completo

1. Crear una partida entre `Machamp` y `Snorlax`.
2. Verificar `Status = Created`, turno `1` y primer atacante.
3. Ejecutar fases consecutivas con el atacante esperado.
4. Recalcular y persistir PS tras cada movimiento.
5. Finalizar cuando uno llegue a `0` PS.
6. Consultar estado final con ganador y perdedor.
7. Consultar histórico completo ordenado por secuencia.

## 11. Requisitos funcionales derivados reflejados en la API

- Identificadores estables para especies, movimientos, instancias jugables y partidas.
- Errores explícitos de validación.
- Rechazo de movimientos no aprendibles al crear o actualizar `Mi Pokémon`.
- Rechazo de movimientos no equipados al calcular daño o ejecutar una fase.
- Rechazo de acciones sobre partidas finalizadas.
- Resolución de efectividad basada en una tabla normativa materializada en código.
- Soporte real de defensores con uno o dos tipos.
- Trazabilidad del cálculo de daño.
- Histórico persistente de fases de combate.
- Protección de borrados cuando existen dependencias activas.

## 12. Riesgos funcionales mitigados

### Riesgo 1: modelo de un solo tipo

Estado: mitigado.

El dominio soporta `1..2` tipos por especie y el cálculo multiplica la efectividad contra cada tipo defensor.

### Riesgo 2: ignorar la categoría del movimiento

Estado: mitigado.

El cálculo usa `Attack/Defense` para `Physical` y `SpecialAttack/SpecialDefense` para `Special`.

### Riesgo 3: confundir especie con instancia jugable

Estado: mitigado.

El catálogo base se modela como `PokemonSpecies`; el estado mutable de juego se modela como `MyPokemon`; el combate usa `MyPokemon`, no especies base.

### Riesgo 4: importar demasiado catálogo en el MVP

Estado: mitigado.

El seed actual usa un conjunto curado de 10 especies y 27 movimientos.

### Riesgo 5: no materializar la tabla de efectividad

Estado: mitigado.

La tabla está implementada como contrato explícito en `PokemonTypeEffectivenessChart` y cubierta por pruebas de dominio.

### Riesgo 6: perder trazabilidad de combate

Estado: mitigado.

Cada fase persiste random, efectividad por tipo, efectividad total, daño y PS resultantes.

## 13. Trazabilidad requisito -> implementación

| Requisito | Casos de uso | Implementación principal |
| --- | --- | --- |
| Cálculo de daño del PDF | `UC-17` | `MoveDamageCalculationService`, `MoveDamageCalculator` |
| Tabla de efectividad | `UC-17`, `UC-20`, `UC-22` | `PokemonTypeEffectivenessChart` |
| CRUD Pokémon base | `UC-01` a `UC-04` | `PokemonEndpoints`, handlers de `Features/Pokemons` |
| CRUD movimientos | `UC-05` a `UC-08` | `MoveEndpoints`, handlers de `Features/Moves` |
| Movimientos posibles | `UC-09`, `UC-10`, `UC-11` | relación `PokemonLearnableMove` |
| Mis Pokémon | `UC-12` a `UC-16` | `MyPokemonEndpoints`, `MyPokemon` |
| Partida de combate | `UC-18` a `UC-22` | `BattleEndpoints`, `Battle`, `BattlePhase` |

## 14. Fuentes funcionales de referencia

- PDF de requisitos: `docs/requirements/POKÉMON 2.pdf`
- Catálogo general de Pokémon: `https://pokemondb.net/pokedex/all`
- Catálogo general de movimientos: `https://pokemondb.net/move/all`
- Implementación de endpoints: `src/Api/Visiotech.Pokemon.Api/Endpoints`
- Implementación de dominio: `src/Api/Visiotech.Pokemon.Domain`
- Implementación de casos de aplicación: `src/Api/Visiotech.Pokemon.Application`
