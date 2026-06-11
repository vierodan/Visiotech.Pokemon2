# Dominio de Visiotech Pokémon

## 1. Propósito del documento

Este documento describe el dominio actual de `Visiotech.Pokemon` según el código existente en `src/Api/Visiotech.Pokemon.Domain` y su uso desde la capa de aplicación, catálogo de especies, catálogo de movimientos, instancias jugables, cálculo de daño y combate con estado.

El objetivo no es describir DTOs ni tablas, sino explicar el modelo de negocio que protege las reglas principales del sistema:

- qué es una especie base de Pokémon
- qué es un movimiento de catálogo
- qué es un “Mi Pokémon” como instancia jugable
- cómo se calcula el daño
- cómo se mantiene una partida de combate
- qué invariantes viven en dominio y cuáles se validan en aplicación por depender de persistencia

## 2. Fuente funcional y alcance

El modelo actual responde a los requisitos iniciales del PDF `docs/requirements/POKÉMON 2.pdf`:

- La Parte 1 pide calcular el daño de un movimiento entre un atacante y un rival usando nivel, tipos, PS, estadísticas base, movimiento, efectividad y un factor aleatorio entre `85` y `100`.
- La Parte 2 pide una API tipo Pokédex con Pokémon base, movimientos, Mis Pokémon con hasta 4 movimientos, consulta de movimientos equipados, consulta de movimientos posibles y relación entre movimientos, tipos y Pokémon.
- La Parte 3 pide mantener el estado de una partida, asignar 2 Pokémon adversarios y representar fases de combate hasta que los PS de uno lleguen a `0`.

Por eso el dominio no se modela como una única entidad `Pokemon`. El código separa explícitamente:

- datos estáticos de catálogo
- datos mutables de una instancia jugable
- reglas matemáticas de daño
- estado mutable e histórico de combate

Esta separación es la decisión estructural más importante del dominio actual.

## 3. Organización del dominio

El proyecto de dominio se divide en dos áreas principales:

```txt
src/Api/Visiotech.Pokemon.Domain/
  Abstractions/
  Pokemons/
  Battles/
```

`Abstractions` contiene las bases DDD comunes:

- `Entity<TId>`
- `AggregateRoot<TId>`
- `ValueObject`
- `DomainException`

`Pokemons` contiene el modelo de catálogo, instancia jugable, tipos, movimientos y cálculo de daño.

`Battles` contiene el modelo de partida, combatientes, fases, estado e histórico de combate.

El dominio no depende de Application, Infrastructure, Api, Contracts ni Host. Esa regla está protegida por tests de arquitectura.

## 4. Abstracciones DDD

### 4.1 `Entity<TId>`

`Entity<TId>` representa objetos con identidad estable. Dos entidades son iguales si comparten el mismo identificador.

Esta base se usa para agregados con `Guid`, como:

- `PokemonSpecies`
- `PokemonMove`
- `MyPokemon`
- `Battle`

La identidad no depende de los atributos mutables. Esto es esencial porque una especie puede renombrarse, una instancia puede cambiar de PS o una batalla puede avanzar de turno sin dejar de ser la misma entidad.

### 4.2 `AggregateRoot<TId>`

`AggregateRoot<TId>` marca la raíz de consistencia de un conjunto de entidades relacionadas. En el código actual no añade comportamiento extra sobre `Entity<TId>`, pero deja explícita la intención arquitectónica.

Agregados actuales:

- `PokemonSpecies`
- `PokemonMove`
- `MyPokemon`
- `Battle`

### 4.3 `ValueObject`

`ValueObject` marca objetos definidos por su valor, no por una identidad propia.

Value objects actuales:

- `Name`
- `Level`
- `BaseStats`
- `Move`
- `Ability`
- `PokemonTyping`

`Ability` existe como value object, pero actualmente no participa en los agregados ni en la persistencia del flujo funcional implementado.

`Ability` valida su nombre reutilizando `Name`. Se mantiene en el dominio como concepto preparado para evolución futura, aunque los casos de uso implementados todavía no gestionan habilidades.

### 4.4 `DomainException`

`DomainException` expresa violaciones de reglas propias del dominio. La capa de aplicación captura estas excepciones cuando corresponde y las traduce a errores funcionales explícitos.

## 5. Lenguaje del dominio

El modelo diferencia cuatro conceptos que en una implementación anémica podrían confundirse:

- `PokemonSpecies`: especie base o entrada de Pokédex.
- `PokemonMove`: movimiento reutilizable del catálogo.
- `MyPokemon`: instancia jugable concreta basada en una especie.
- `Battle`: partida de combate entre dos instancias jugables.

Esta distinción evita un riesgo central del sistema: confundir datos estáticos de catálogo con estado mutable de juego. Una especie define tipos y estadísticas base; una instancia jugable tiene nivel, PS actuales, PS totales y movimientos equipados; una batalla mantiene su propia instantánea de combatientes e histórico.

## 6. Catálogo de especies: `PokemonSpecies`

`PokemonSpecies` es un aggregate root que representa una especie base de Pokémon.

Responsabilidades:

- mantener identidad estable de especie
- encapsular nombre normalizado
- encapsular tipos de la especie
- encapsular estadísticas base
- mantener la relación de movimientos aprendibles

Propiedades principales:

- `Id`
- `Name`
- `NormalizedName`
- `Typing`
- `BaseStats`
- `LearnableMoves`

Operaciones principales:

- `Create`
- `Rename`
- `ReconfigureTyping`
- `ReconfigureBaseStats`
- `AddLearnableMove`
- `RemoveLearnableMove`

Invariantes:

- el identificador de especie no puede ser `Guid.Empty`
- el nombre debe ser válido mediante `Name`
- la especie debe tener uno o dos tipos mediante `PokemonTyping`
- las estadísticas base deben ser positivas mediante `BaseStats`
- un movimiento aprendible no puede tener identificador vacío
- una especie no puede registrar dos veces el mismo movimiento aprendible
- no se puede retirar un movimiento aprendible que la especie no contiene

### 6.1 Por qué es un agregado separado

El PDF pide recursos de “Pokémon base (CRUD)” y consultas de movimientos posibles. Eso implica que una especie debe existir como concepto estable del catálogo y no como parte embebida de cada instancia jugable.

Separar `PokemonSpecies` permite:

- consultar catálogo sin depender de instancias jugables
- asociar movimientos aprendibles a una especie
- reutilizar estadísticas base en cálculo de daño
- impedir que actualizar una instancia modifique la especie base

### 6.2 Relación aprendible: `PokemonLearnableMove`

`PokemonLearnableMove` representa la relación entre una especie y un movimiento que puede aprender.

Propiedades:

- `PokemonSpeciesId`
- `PokemonMoveId`

Invariantes:

- `PokemonSpeciesId` no puede ser `Guid.Empty`
- `PokemonMoveId` no puede ser `Guid.Empty`

No se modela como aggregate root porque no tiene ciclo de vida independiente: existe dentro del contexto de una especie y sirve para expresar la lista de movimientos posibles.

## 7. Catálogo de movimientos: `PokemonMove`

`PokemonMove` es un aggregate root que representa un movimiento reutilizable del catálogo.

Responsabilidades:

- mantener identidad estable del movimiento
- encapsular nombre normalizado
- definir tipo elemental del movimiento
- definir categoría
- definir potencia

Propiedades principales:

- `Id`
- `Name`
- `NormalizedName`
- `Type`
- `Category`
- `Power`

Operaciones principales:

- `Create`
- `Reconfigure`
- `ToValueObject`

Invariantes:

- el identificador no puede ser `Guid.Empty`
- el nombre debe ser válido mediante `Name`
- la potencia debe ser coherente con la categoría mediante `Move`
- un movimiento `Status` debe tener potencia `0`
- un movimiento `Physical` o `Special` debe tener potencia mayor que `0`

### 7.1 Por qué es un agregado separado

El PDF pide “Movimientos (CRUD)” y consultas de Pokémon que comparten un mismo movimiento. Eso exige que el movimiento tenga identidad propia y pueda ser referenciado por especies, instancias jugables y fases de combate.

El value object `Move` contiene la forma validada del movimiento, mientras que `PokemonMove` añade identidad de catálogo y normalización.

## 8. Instancia jugable: `MyPokemon`

`MyPokemon` es un aggregate root que representa un Pokémon jugable concreto creado a partir de una especie base.

Responsabilidades:

- referenciar la especie base
- mantener nivel
- mantener PS actuales y PS totales
- mantener movimientos equipados en slots ordenados
- proteger la cardinalidad máxima de movimientos equipados

Propiedades principales:

- `Id`
- `PokemonSpeciesId`
- `Level`
- `CurrentHealthPoints`
- `TotalHealthPoints`
- `EquippedMoves`
- `EquippedMoveIds`

Operaciones principales:

- `Create`
- `Reconfigure`
- `UpdateCurrentHealthPoints`

Invariantes:

- el identificador no puede ser `Guid.Empty`
- `PokemonSpeciesId` no puede ser `Guid.Empty`
- el nivel debe estar entre `1` y `100`
- los PS totales deben ser mayores que `0`
- los PS actuales no pueden ser negativos
- los PS actuales no pueden superar los PS totales
- debe equipar entre `1` y `4` movimientos
- no puede equipar movimientos con identificador vacío
- no puede equipar el mismo movimiento más de una vez

### 8.1 Slots de movimiento: `MyPokemonMoveSlot`

`MyPokemonMoveSlot` representa la posición equipada de un movimiento dentro de una instancia jugable.

Responsabilidades:

- asociar `MyPokemonId`
- mantener `SlotNumber`
- referenciar `PokemonMoveId`

Invariantes:

- `MyPokemonId` no puede ser `Guid.Empty`
- `PokemonMoveId` no puede ser `Guid.Empty`
- `SlotNumber` debe estar entre `1` y `4`

### 8.2 Reglas que no viven directamente en `MyPokemon`

El dominio valida que la instancia equipe entre 1 y 4 movimientos y que no haya duplicados. Sin embargo, la regla “solo puede equipar movimientos aprendibles por su especie” se valida en aplicación mediante `MyPokemonCommandGuard`.

Esto es deliberado: para saber si un movimiento es aprendible se necesita consultar:

- la especie con sus movimientos aprendibles
- el catálogo de movimientos existentes

El agregado `MyPokemon` no accede a repositorios porque el dominio se mantiene libre de infraestructura. Por eso la regla cruzada se orquesta en aplicación y el agregado conserva las invariantes locales que sí puede garantizar por sí mismo.

## 9. Tipos Pokémon

### 9.1 `PokemonType`

`PokemonType` es un enum cerrado con los tipos soportados:

- `Normal`
- `Fire`
- `Water`
- `Grass`
- `Electric`
- `Flying`
- `Steel`
- `Bug`
- `Dragon`
- `Ghost`
- `Fairy`
- `Ice`
- `Fighting`
- `Psychic`
- `Rock`
- `Dark`
- `Ground`
- `Poison`

Se modela como enum porque el PDF remite a una taxonomía conocida y estable. Además, la tabla de efectividad necesita claves cerradas y completas.

### 9.2 `PokemonTyping`

`PokemonTyping` es un value object que representa el tipo o los tipos de una especie.

Invariantes:

- debe contener uno o dos tipos
- no puede contener tipos duplicados

Esta decisión permite soportar defensores de uno o dos tipos en el cálculo de daño y evita modelar el segundo tipo como una cadena opcional sin reglas.

### 9.3 `PokemonTypeEffectivenessChart`

`PokemonTypeEffectivenessChart` materializa la tabla normativa de efectividad.

Responsabilidades:

- devolver el multiplicador para un tipo atacante y un tipo defensor
- fallar con `DomainException` si falta una combinación
- hacer explícita en código la matriz de efectividad usada por cálculo de daño y combate

El resultado se usa por tipo defensor. En defensores de dos tipos, el cálculo multiplica ambos coeficientes.

### 9.4 Catálogo de conversión: `PokemonTypeCatalog`

`PokemonTypeCatalog` expone los nombres permitidos de `PokemonType` y permite convertir texto de entrada a enum de forma controlada.

Responsabilidades:

- publicar `AllowedNames`
- convertir texto sin distinguir mayúsculas y minúsculas
- rechazar valores nulos, vacíos o desconocidos

Esta clase facilita que la capa de aplicación valide contratos externos sin duplicar la lista normativa de tipos.

## 10. Categoría y potencia de movimientos

`MoveCategory` define tres categorías:

- `Physical`
- `Special`
- `Status`

La categoría afecta directamente al cálculo de daño:

- `Physical` usa `Attack` del atacante y `Defense` del defensor
- `Special` usa `SpecialAttack` del atacante y `SpecialDefense` del defensor
- `Status` no puede calcular daño

El value object `Move` valida la coherencia entre categoría y potencia:

- `Status` exige potencia `0`
- `Physical` y `Special` exigen potencia mayor que `0`

Esta regla evita movimientos inconsistentes en el catálogo antes de llegar al cálculo.

### 10.1 Catálogo de conversión: `MoveCategoryCatalog`

`MoveCategoryCatalog` expone las categorías permitidas y permite convertir texto externo a `MoveCategory`.

Responsabilidades:

- publicar `AllowedNames`
- convertir texto sin distinguir mayúsculas y minúsculas
- asegurar que el valor convertido existe realmente en el enum

Igual que `PokemonTypeCatalog`, evita duplicar listas de valores válidos fuera del dominio.

## 11. Estadísticas y nivel

### 11.1 `BaseStats`

`BaseStats` agrupa las estadísticas base indicadas por los requisitos:

- `Health`
- `Attack`
- `Defense`
- `SpecialAttack`
- `SpecialDefense`
- `Speed`

Todas deben ser mayores que `0`.

El cálculo de daño usa:

- `Attack` y `Defense` para movimientos físicos
- `SpecialAttack` y `SpecialDefense` para movimientos especiales

### 11.2 `Level`

`Level` representa el nivel de una instancia jugable.

Invariante:

- debe estar entre `1` y `100`

El nivel participa directamente en la fórmula de daño.

## 12. Cálculo de daño

El cálculo de daño está centralizado en:

- `DamageCalculationInput`
- `MoveDamageCalculator`
- `DamageCalculationResult`
- `DamageCalculationEffectiveness`

### 12.1 Entrada de cálculo

`DamageCalculationInput` valida:

- nivel del atacante mayor que `0`
- estadísticas de atacante y defensor presentes
- PS actuales del defensor mayores que `0`
- movimiento no `Status`
- potencia mayor que `0`
- defensor con uno o dos tipos
- random entre `85` y `100`

### 12.2 Fórmula implementada

`MoveDamageCalculator` implementa la fórmula actual del código:

```txt
baseDamage = ((((2 * level) / 5) + 2) * offensiveStat * movePower / defensiveStat) / 50
rawDamage = floor(baseDamage * totalEffectiveness * random / 100)
damage = min(rawDamage, defenderCurrentHealthPoints)
defenderRemainingHealthPoints = max(0, defenderCurrentHealthPoints - damage)
```

Decisiones relevantes:

- el redondeo del daño bruto se hace con `Floor`
- si la efectividad total es `0`, el daño bruto es `0`
- el daño aplicado nunca baja de `0`
- el daño aplicado nunca supera los PS actuales del defensor
- el resultado conserva trazabilidad completa

### 12.3 Resultado de cálculo

`DamageCalculationResult` devuelve:

- estadística ofensiva usada
- valor ofensivo usado
- estadística defensiva usada
- valor defensivo usado
- random
- daño base
- desglose de efectividad por tipo defensor
- efectividad total
- daño bruto
- daño aplicado
- PS restantes del defensor

Esta trazabilidad es necesaria porque los requisitos no solo piden un número final: el combate y el histórico deben poder explicar qué ocurrió.

## 13. Partida de combate: `Battle`

`Battle` es un aggregate root que mantiene el estado de una partida entre exactamente dos instancias jugables.

Responsabilidades:

- crear una partida con dos combatientes distintos
- mantener estado `Created`, `InProgress` o `Finished`
- mantener turno actual
- mantener próximo atacante
- registrar fases de combate
- actualizar PS de combatientes
- cerrar la partida por KO
- informar ganador y perdedor
- impedir fases posteriores al final

Propiedades principales:

- `Id`
- `Status`
- `CurrentTurnNumber`
- `NextAttackerMyPokemonId`
- `WinnerMyPokemonId`
- `LoserMyPokemonId`
- `Combatants`
- `Phases`

### 13.1 Estado de partida: `BattleStatus`

`BattleStatus` es un enum cerrado con tres estados:

- `Created`
- `InProgress`
- `Finished`

El agregado `Battle` controla las transiciones entre estos estados. El exterior no puede marcar una batalla como finalizada arbitrariamente sin pasar por `RecordPhase`.

### 13.2 Creación de partida

`Battle.Create` valida:

- `BattleId` no vacío
- ambos `MyPokemonId` no vacíos
- dos combatientes distintos
- PS iniciales de ambos combatientes mayores que `0`

Estado inicial:

- `Status = Created`
- `CurrentTurnNumber = 1`
- `NextAttackerMyPokemonId = firstMyPokemonId`
- dos combatientes en slots `1` y `2`

Esta modelización responde directamente a la Parte 3 del PDF: asignar dos Pokémon adversarios y representar una partida.

### 13.3 Registro de fase

`Battle.RecordPhase` valida:

- la partida no puede estar finalizada
- el número de secuencia debe coincidir con el turno actual
- el atacante debe ser el próximo atacante configurado
- atacante y defensor deben pertenecer a la partida
- si el defensor queda con `0` PS, la partida debe finalizar
- si la fase se marca como final, el defensor debe quedar con `0` PS
- si la partida continúa, debe existir próximo atacante y pertenecer a la partida

Cuando registra una fase:

- crea `BattlePhase`
- crea desglose de efectividad con `BattlePhaseEffectiveness`
- actualiza PS del atacante y defensor dentro de la instantánea de combate
- avanza a `InProgress` si continúa
- marca `Finished` si hay KO
- informa ganador y perdedor si finaliza

### 13.4 Comando de registro: `BattlePhaseRegistration`

`BattlePhaseRegistration` es un record de dominio que agrupa todos los datos necesarios para registrar una fase en `Battle.RecordPhase`.

Contiene:

- secuencia
- atacante
- defensor
- movimiento
- nombre del movimiento
- random
- desglose de efectividad
- efectividad total
- daño
- PS restantes
- próximo atacante
- indicador de finalización

No es una entidad persistente; es un objeto de entrada para mantener la firma de `RecordPhase` cohesionada y explícita.

`BattlePhaseEffectivenessInput` cumple el mismo papel para cada elemento del desglose de efectividad antes de materializarlo como `BattlePhaseEffectiveness`.

## 14. Combatientes y fases

### 14.1 `BattleCombatant`

`BattleCombatant` representa la instantánea mutable de una instancia jugable dentro de una partida.

Propiedades:

- `BattleId`
- `SlotNumber`
- `MyPokemonId`
- `CurrentHealthPoints`
- `TotalHealthPoints`

Invariantes:

- `BattleId` no vacío
- slot entre `1` y `2`
- `MyPokemonId` no vacío
- PS actuales no negativos
- PS totales mayores que `0`
- PS actuales no superiores a PS totales

La instantánea es importante porque una partida no debe leerse solo desde el catálogo estático ni reconstruirse únicamente desde la instancia base sin considerar el estado del combate.

### 14.2 `BattlePhase`

`BattlePhase` representa una acción registrada dentro del combate.

Propiedades:

- `BattleId`
- `SequenceNumber`
- `AttackerMyPokemonId`
- `DefenderMyPokemonId`
- `MoveId`
- `MoveName`
- `RandomFactor`
- `TotalEffectiveness`
- `Damage`
- `AttackerRemainingHealthPoints`
- `DefenderRemainingHealthPoints`
- `EffectivenessBreakdown`

Invariantes:

- identificadores obligatorios
- atacante y defensor distintos
- nombre de movimiento obligatorio
- secuencia mayor que `0`
- random entre `85` y `100`
- daño no negativo
- PS restantes no negativos

### 14.3 `BattlePhaseEffectiveness`

`BattlePhaseEffectiveness` conserva el multiplicador usado para cada tipo defensor en una fase.

Propiedades:

- `BattleId`
- `BattlePhaseSequenceNumber`
- `DefenderType`
- `Multiplier`

Invariantes:

- `BattleId` no vacío
- secuencia mayor que `0`
- multiplicador no negativo

Este detalle evita perder el contrato explícito de efectividad cuando se consulta el histórico.

## 15. Frontera entre dominio y aplicación

El dominio protege invariantes locales y reglas puras. La aplicación protege reglas que necesitan consultar persistencia, coordinar agregados o decidir respuestas funcionales.

Ejemplos de reglas en dominio:

- una especie debe tener uno o dos tipos
- un movimiento `Status` debe tener potencia `0`
- un `MyPokemon` no puede tener cinco movimientos equipados
- una batalla finalizada no puede registrar fases
- el cálculo de daño debe respetar categoría, tipos, random y PS restantes

Ejemplos de reglas en aplicación:

- comprobar que una especie existe antes de crear un `MyPokemon`
- comprobar que los movimientos equipados existen
- comprobar que los movimientos equipados son aprendibles por la especie
- comprobar que el movimiento usado está equipado por el atacante
- cargar especies y movimientos para alimentar el cálculo de daño
- traducir errores de dominio a errores de validación HTTP

Esta frontera mantiene el dominio puro, testeable y sin dependencias de EF Core ni repositorios.

## 16. Integridad frente a persistencia

Aunque este documento se centra en dominio, el modelo está diseñado para ser persistido de forma segura con EF Core:

- las entidades usan constructores privados para EF y factorías públicas para negocio
- las colecciones internas se exponen como `IReadOnlyCollection`
- las mutaciones pasan por métodos del agregado
- las claves son `Guid` estables
- las relaciones críticas se expresan mediante identificadores entre agregados
- las instantáneas de combate evitan depender solo de datos estáticos

La base de datos refuerza varias invariantes mediante restricciones, pero la fuente primaria de significado del negocio sigue estando en el dominio y en los servicios de aplicación que coordinan agregados.

## 17. Relación con Clean Architecture

El dominio actual:

- no depende de ASP.NET Core
- no depende de Entity Framework Core
- no depende de contratos HTTP
- no depende de repositorios
- no depende del Host
- no conoce PostgreSQL ni InMemory

Esto permite que las reglas de negocio se prueben sin infraestructura y que la API sea solo un adaptador de entrada.

Las reglas arquitectónicas se validan con `NetArchTest.Rules`, incluyendo:

- `Domain` no depende de capas externas
- `Application` no depende de `Api` ni de `Infrastructure`
- los contratos públicos no dependen del dominio
- los value objects esperados heredan de `ValueObject`

## 18. Justificación técnica del modelo

El modelo se ha diseñado de esta forma porque los requisitos iniciales mezclan tres naturalezas distintas de datos:

- catálogo estático tipo Pokédex
- estado mutable de instancias jugables
- estado mutable e histórico de combate

Modelar todo como una única entidad `Pokemon` habría generado un modelo frágil. Por ejemplo, actualizar los PS de un combatiente podría terminar modificando accidentalmente la especie base; equipar movimientos podría confundirse con alterar el catálogo global; y consultar el histórico de una partida podría depender de datos actuales que ya no representan lo ocurrido en el turno original.

La separación actual mitiga esos riesgos:

- `PokemonSpecies` representa la definición base, estable y reutilizable.
- `PokemonMove` representa movimientos como recursos de catálogo con identidad propia.
- `PokemonLearnableMove` expresa qué movimientos puede aprender una especie.
- `MyPokemon` representa una instancia jugable con nivel, PS y movimientos equipados.
- `Battle` representa una partida con su propio estado de turno y resultado.
- `BattlePhase` y `BattlePhaseEffectiveness` preservan trazabilidad histórica.
- `MoveDamageCalculator` concentra la matemática de daño para que consulta y combate usen el mismo contrato.

Esta estructura sigue una regla práctica de arquitectura de dominio: separar lo que cambia por motivos diferentes. El catálogo cambia cuando se administran especies o movimientos; una instancia cambia cuando el jugador configura sus Pokémon; una batalla cambia cuando se ejecutan fases; la tabla de efectividad cambia solo si cambia la norma del juego.

El PDF justifica esta separación. La Parte 2 pide recursos CRUD separados para Pokémon base, movimientos y Mis Pokémon. Eso descarta un agregado único para todo. La Parte 1 pide cálculo de daño con nivel, estadísticas, tipos, movimiento, efectividad y random; por eso la fórmula se modela como lógica pura de dominio y no como código de endpoint. La Parte 3 pide estado de partida hasta PS `0`; por eso existe `Battle` con combatientes, fases, KO, ganador, perdedor e histórico.

También hay una decisión deliberada sobre los límites del dominio: las reglas que requieren mirar varios agregados se resuelven en aplicación. Por ejemplo, “este movimiento es aprendible por esta especie” necesita consultar especie y movimiento; “este movimiento está equipado por el atacante” necesita cargar la instancia jugable; “esta partida puede avanzar” necesita cargar el agregado `Battle`. Mantener esas coordinaciones fuera de las entidades evita acoplar el dominio a persistencia y conserva una Clean Architecture estricta.

Técnicamente, el modelo actual es más robusto que una solución anémica porque:

- las entidades no exponen setters públicos arbitrarios
- los value objects validan datos semánticos en su creación
- los agregados controlan sus colecciones internas
- el cálculo de daño es determinista salvo por el proveedor externo de random
- el histórico conserva los coeficientes de efectividad usados
- las reglas de finalización por KO viven en el flujo del combate y no en la API
- la persistencia refuerza, pero no sustituye, las invariantes del modelo

En resumen, el dominio actual está alineado con los requisitos del PDF y con una arquitectura .NET mantenible: separa catálogo, configuración jugable, cálculo y combate; conserva invariantes dentro del modelo; deja las coordinaciones multiagregado en aplicación; y mantiene la infraestructura fuera de las reglas de negocio.
