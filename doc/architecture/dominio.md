# Dominio DDD de Visiotech Pokemon

## 1. Objetivo del modelo

El objetivo del dominio actual es representar un `Pokemon` como concepto central del negocio. La intencion no es modelar una simple estructura de datos para exponer por HTTP, sino capturar las reglas, invariantes y decisiones semanticas que hacen que un Pokemon sea valido dentro del sistema.

En terminos de Domain-Driven Design:

- el dominio contiene conocimiento de negocio
- las invariantes viven dentro del modelo y no en la API
- la capa de aplicacion orquesta casos de uso, pero no define reglas del dominio
- la infraestructura persiste o recupera agregados, pero no altera su significado

## 2. Aggregate Root

El agregado raiz actual es `Pokemon`:

- fichero: `src/Api/Visiotech.Pokemon.Domain/Pokemons/Pokemon.cs`
- tipo base: `AggregateRoot<Guid>`

`Pokemon` es el Aggregate Root porque:

1. representa la unidad de consistencia transaccional del modelo
2. encapsula sus propios invariantes
3. controla la mutacion de sus componentes internos
4. evita que elementos internos se modifiquen libremente desde fuera

Todas las decisiones relevantes sobre nombre, nivel, estadisticas, movimientos y habilidades pasan por el agregado.

## 3. Identidad

`Pokemon` tiene identidad propia mediante `Guid`.

Esto es importante porque en DDD:

- una entidad se define por su identidad a lo largo del tiempo
- dos entidades con los mismos datos no son necesariamente la misma entidad
- la igualdad conceptual de `Pokemon` no depende de que sus atributos coincidan

Por eso `Pokemon` hereda de `Entity<TId>` y `AggregateRoot<TId>`, mientras que sus componentes internos de valor heredan de `ValueObject`.

## 4. Value Objects

La solucion define una abstraccion explicita:

- `src/Api/Visiotech.Pokemon.Domain/Abstractions/ValueObject.cs`

Esto refuerza una base DDD mas estricta por tres razones:

1. hace visible la intencion del modelo
2. distingue conceptualmente entidades de objetos de valor
3. permite automatizar validaciones arquitectonicas

Los Value Objects actuales son:

### 4.1 `Name`

Representa el nombre del Pokemon.

Responsabilidades:

- impedir nombres vacios o en blanco
- normalizar el valor de entrada
- limitar su longitud maxima

Razon para ser Value Object:

- no tiene identidad propia
- se compara por valor
- es intercambiable si el valor es igual

### 4.2 `Level`

Representa el nivel del Pokemon.

Responsabilidades:

- garantizar que el nivel esta entre 1 y 100

Razon para ser Value Object:

- el nivel no tiene ciclo de vida independiente
- el interes del negocio esta en el valor, no en una identidad

### 4.3 `BaseStats`

Representa las estadisticas del Pokemon:

- health
- attack
- defense
- special attack
- special defense
- speed

Responsabilidades:

- garantizar que todas las estadisticas sean mayores que cero
- agrupar coherentemente un conjunto de datos que se validan como unidad semantica

Razon para ser Value Object:

- las estadisticas carecen de identidad independiente
- se usan como composicion inmutable del agregado

### 4.4 `Move`

Representa un movimiento conocido por el Pokemon.

Responsabilidades:

- garantizar nombre valido
- garantizar potencia positiva
- conservar el tipo del movimiento

Razon para ser Value Object:

- dentro del modelo actual, el movimiento no se persiste como agregado independiente
- el sistema esta modelando el movimiento como parte del estado del Pokemon, no como catalogo autonomo

Nota de diseno:

Si en el futuro el dominio necesita un catalogo global de movimientos, efectos secundarios, precision, PP o reglas de aprendizaje, probablemente `Move` deba promocionarse a entidad o aggregate independiente, y `Pokemon` deberia referenciar una proyeccion o una relacion de “movimiento equipado” en vez de embutirlo como value object.

### 4.5 `Ability`

Representa una habilidad del Pokemon.

Responsabilidades:

- garantizar nombre valido

Razon para ser Value Object:

- actualmente no tiene identidad ni ciclo de vida fuera del agregado

Misma observacion evolutiva que en `Move`: si el dominio incorpora un catalogo global de habilidades con comportamiento propio, podria emerger una entidad especifica.

## 5. Enum de soporte

`PokemonType` no esta modelado como Value Object sino como `enum`.

Esto significa que:

- hoy se trata como conjunto cerrado de opciones
- el sistema asume una taxonomia estable y limitada

Es una decision aceptable para una primera fase, pero tiene implicaciones:

- un `enum` es menos expresivo que un Value Object rico
- si en el futuro se necesitan reglas por tipo, metadatos, traducciones o relaciones de efectividad, podria convenir refactorizarlo a un tipo mas expresivo

## 6. Invariantes del agregado

El agregado `Pokemon` protege explicitamente las siguientes invariantes:

### 6.1 Identidad obligatoria

- el `Guid` no puede ser vacio

### 6.2 Nivel valido

- el nivel debe estar entre 1 y 100

### 6.3 Estadisticas validas

- todas las estadisticas deben ser positivas

### 6.4 Limite de movimientos

- un Pokemon no puede tener mas de 4 movimientos

### 6.5 Limite de habilidades

- un Pokemon no puede tener mas de 4 habilidades

### 6.6 Unicidad de movimientos

- no puede haber dos movimientos con el mismo nombre dentro del mismo Pokemon

### 6.7 Unicidad de habilidades

- no puede haber dos habilidades con el mismo nombre dentro del mismo Pokemon

## 7. Encapsulacion del agregado

El agregado expone colecciones como `IReadOnlyCollection`, y la mutacion interna se realiza mediante metodos del propio agregado:

- `Rename`
- `ChangeLevel`
- `ReconfigureStats`
- `ReplaceMoveSet`
- `ReplaceAbilities`

Esta decision es clave en DDD porque:

- evita modificaciones arbitrarias desde fuera
- concentra la logica de consistencia
- preserva la validez del agregado tras cada cambio

## 8. Por que esta modelizacion es mas estricta que una aproximacion anemica

Un modelo anemico habria dejado:

- clases DTO sin comportamiento
- validaciones dispersas en API, handlers o servicios
- mutaciones directas de colecciones

La base actual evita eso porque:

1. la creacion de objetos pasa por factorias que validan
2. los objetos de valor encapsulan sus propias reglas
3. el agregado controla su estado interno
4. la capa de aplicacion solo consulta u orquesta

## 9. Relacion con la Clean Architecture

El dominio:

- no depende de Application
- no depende de Infrastructure
- no depende de Api
- no depende de Host

Esto garantiza que el modelo de negocio sea estable y portable.

En la solucion actual, esta restriccion no solo es conceptual: esta automatizada por tests arquitectonicos.

## 10. Validacion arquitectonica automatizada

El proyecto `tests/Visiotech.Pokemon.ArchitectureTests` contiene reglas que validan:

- que `Domain` no depende de capas externas
- que `Application` no depende de `Api`, `Host` o `Infrastructure`
- que `Infrastructure` no depende de `Api` ni `Host`
- que la capa `Api` no depende de `Host`
- que las abstracciones de persistencia sean interfaces
- que los Value Objects definidos hereden de `ValueObject`

Esto transforma decisiones de arquitectura en contratos ejecutables.

## 11. Estado actual del modelo y siguientes pasos recomendados

El modelo es correcto para una fase inicial, pero todavia es deliberadamente contenido. Desde una perspectiva DDD estricta, los siguientes pasos naturales serian:

### 11.1 Separar catalogos de conceptos reutilizables

Si el negocio crece, podria tener sentido elevar:

- `Move`
- `Ability`

a entidades o agregados propios, dejando dentro de `Pokemon` solo la relacion de posesion, aprendizaje o equipamiento.

### 11.2 Introducir especificaciones de negocio adicionales

Ejemplos:

- restricciones por tipo
- reglas de aprendizaje de movimientos
- compatibilidad de habilidades
- evolucion del Pokemon
- estado de combate frente a estado de catalogo

### 11.3 Diferenciar subdominios

Es probable que el problema evolucione en al menos estos subdominios:

- catalogo de Pokemon
- combate
- construccion de equipos
- reglas de aprendizaje y progresion

En ese escenario, `BuildingBlocks` deberia seguir siendo minimo y el dominio deberia dividirse por bounded contexts, no por utilidades tecnicas.

## 12. Conclusion

El modelo actual representa una base DDD tecnicamente solida porque:

- existe un Aggregate Root explicito: `Pokemon`
- existen Value Objects explicitos con abstraccion comun
- las invariantes viven dentro del dominio
- la mutacion esta encapsulada
- la arquitectura esta automatizada por tests

En otras palabras, el sistema no trata a `Pokemon` como un simple registro serializable, sino como una unidad de negocio consistente y protegida por el propio modelo.
