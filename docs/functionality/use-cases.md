# Análisis funcional y casos de uso del MVP Pokémon

## 1. Objetivo del documento

Este documento traduce el fichero de requisitos `docs/requirements/POKÉMON 2.pdf` a un análisis funcional implementable para el MVP de la API Pokémon. El objetivo es convertir un enunciado breve en un contrato funcional claro para producto, backend y QA.

El alcance se ha construido a partir de:

- el PDF de requisitos
- la restricción adicional del MVP: trabajar con 10 Pokémon
- la fuente de catálogo de Pokémon: `https://pokemondb.net/pokedex/all`
- la fuente de catálogo de movimientos: `https://pokemondb.net/move/all`

## 2. Resumen ejecutivo del alcance

El MVP tiene tres bloques funcionales:

1. `Motor de daño`: dado un Pokémon atacante, un movimiento escogido y un Pokémon rival, el sistema debe devolver el daño calculado.
2. `Pokédex API`: el sistema debe exponer CRUD y consultas sobre catálogo base de Pokémon, catálogo de movimientos y la colección de "Mis Pokémon" con hasta 4 movimientos equipados.
3. `Combate`: el sistema debe mantener el estado de una partida entre 2 Pokémon adversarios y permitir avanzar el combate por fases hasta que los PS de uno de ellos lleguen a 0.

## 3. Lectura funcional del requisito original

### 3.1 Parte 1 del PDF

El requisito exige un método de cálculo de daño basado en:

- nivel
- estadísticas del atacante y del rival
- poder del movimiento
- efectividad por tipo
- factor aleatorio entero entre 85 y 100

La fórmula mostrada en el PDF es:

```txt
Daño(PS) = {[(2 * Nivel / 5 + 2) * AtaqueDelAtacante * PoderDelMovimiento / DefensaDelRival] / 50} * Efectividad * Random / 100
```

Además, el PDF incluye una tabla completa de `Efectividad` por tipo. Este análisis la transcribe más adelante y la adopta como fuente normativa para cualquier decisión funcional sobre ventaja, resistencia o inmunidad.

### 3.2 Parte 2 del PDF

La API "estilo pokédex" debe cubrir al menos:

- Pokémon base `CRUD`
- Movimientos `CRUD`
- Mis Pokémon con sus 4 movimientos `CRUD`
- Consulta para obtener los movimientos de un Pokémon
- Consulta para obtener los movimientos posibles de un Pokémon
- Consulta para obtener la lista de Pokémon que comparten un mismo movimiento

### 3.3 Parte 3 del PDF

La API debe mantener el estado de una partida y representar un combate:

- se asignan 2 Pokémon adversarios
- el combate progresa por fases
- termina cuando los PS de un Pokémon llegan a 0

## 4. Ambigüedades del requisito y decisiones necesarias

El PDF es suficiente para definir el producto, pero no cierra todos los detalles de implementación funcional. Para evitar interpretaciones inconsistentes, este documento fija las siguientes decisiones para el MVP.

### 4.1 Tipo simple frente a tipo dual

El PDF habla de `Tipo` en singular, pero la fuente de catálogo de Pokémon incluye especies con 1 o 2 tipos. Además, la matriz de efectividad del PDF es plenamente compatible con escenarios de multi-tipo.

Decisión recomendada:

- soportar `1..2` tipos por Pokémon base

Impacto:

- permite modelar correctamente especies como `Charizard`, `Venusaur`, `Gengar`, `Golem` o `Dragonite`
- evita empobrecer el catálogo de referencia
- habilita casos de uso más ricos de combate, resistencias e inmunidades

Si el equipo decide mantener un solo tipo por restricciones técnicas del MVP, esa decisión debe quedar documentada como desviación funcional respecto a la fuente de catálogo.

### 4.2 La tabla de efectividad del PDF pasa a ser normativa

El apartado `Efectividad` del PDF no debe tratarse como una imagen orientativa, sino como una regla de negocio cerrada del MVP.

Decisiones recomendadas:

- transcribir la tabla completa dentro de este documento
- usar esa tabla como fuente de verdad funcional para backend, QA y casos de uso
- resolver toda ventaja, resistencia o inmunidad consultando primero la tabla
- multiplicar coeficientes cuando el defensor tenga dos tipos
- si hubiera conflicto entre ejemplos del dataset y la tabla, prevalece la tabla del PDF para el cálculo de daño

### 4.3 Categoría del movimiento: físico o especial

El PDF define en el Pokémon:

- ataque base
- defensa base
- ataque especial base
- defensa especial base

Sin embargo, la fórmula solo habla de `AtaqueDelAtacante` y `DefensaDelRival`, y el movimiento solo aparece con:

- nombre
- poder

Para que existan tanto estadísticas físicas como especiales con sentido funcional, el movimiento debe tener también:

- `tipo`
- `categoría`: `Physical` o `Special`

Decisión recomendada:

- si el movimiento es `Physical`, usar `Attack` del atacante y `Defense` del rival
- si el movimiento es `Special`, usar `SpecialAttack` del atacante y `SpecialDefense` del rival

### 4.4 Elementos de combate fuera de alcance del MVP

El PDF no exige:

- PP de movimientos
- precisión y fallo del movimiento
- críticos
- STAB
- estados alterados
- items
- habilidades
- cambios de estadísticas por buffs/debuffs
- equipos de más de un Pokémon por entrenador

Decisión de alcance:

- todos esos conceptos quedan `fuera del MVP`
- la API de combate se limita a aplicar daño directo por fase con el movimiento seleccionado

### 4.5 Naturaleza de "Mis Pokémon"

El requisito diferencia entre:

- `Pokémon base`
- `Mis Pokémon con sus 4 movimientos`

Interpretación funcional:

- `Pokémon base` representa una especie del catálogo con sus estadísticas y movimientos aprendibles
- `Mi Pokémon` representa una instancia propiedad del usuario o del sistema de prueba, basada en una especie, con nivel, PS actuales, PS totales y hasta 4 movimientos equipados

### 4.6 Consulta "movimientos de un Pokémon" frente a "movimientos posibles"

Para evitar ambigüedad, este documento separa:

- `movimientos equipados`: los que tiene actualmente un `Mi Pokémon`
- `movimientos posibles`: los que la especie puede aprender según el catálogo

## 5. Modelo funcional del dominio

## 5.1 Entidades funcionales principales

### Pokémon base

Representa una especie de referencia del catálogo.

Debe contener como mínimo:

- identificador
- nombre canónico
- uno o dos tipos dentro del catálogo cerrado definido por la tabla de efectividad de la sección `6.1`
- estadísticas base: HP, Attack, Defense, SpecialAttack, SpecialDefense, Speed
- lista de movimientos que puede aprender

### Movimiento

Representa una acción que puede ser aprendida y usada en combate.

Debe contener como mínimo:

- identificador
- nombre
- tipo dentro del catálogo cerrado definido por la tabla de efectividad de la sección `6.1`
- categoría `Physical` o `Special`
- poder

### Mi Pokémon

Representa una instancia jugable o seleccionable para combate.

Debe contener como mínimo:

- identificador
- referencia a Pokémon base
- nivel
- PS actuales
- PS totales
- hasta 4 movimientos equipados

### Partida / Combate

Representa el estado de un enfrentamiento entre dos Pokémon.

Debe contener como mínimo:

- identificador
- Pokémon A
- Pokémon B
- estado de la partida
- turno o fase actual
- histórico de acciones
- ganador cuando aplique

### Fase de combate

Representa una iteración del combate donde:

- se elige un atacante
- se elige un movimiento
- se calcula el daño
- se descuentan PS
- se registra el resultado

## 5.2 Relaciones funcionales

- un `Pokémon base` puede aprender muchos `Movimientos`
- un `Movimiento` puede ser compartido por muchos `Pokémon base`
- un `Mi Pokémon` referencia exactamente un `Pokémon base`
- un `Mi Pokémon` equipa entre `1` y `4` movimientos para jugar
- una `Partida` enfrenta exactamente `2` `Mis Pokémon`
- una `Partida` contiene muchas `Fases`

## 6. Reglas de negocio del MVP

### 6.1 Tabla de efectividad normativa

La siguiente matriz transcribe funcionalmente la tabla del apartado `Efectividad` del PDF.

- las filas representan el tipo del movimiento atacante
- las columnas representan el tipo del Pokémon defensor
- los únicos coeficientes base válidos son `0`, `0.5`, `1` y `2`
- si el defensor tiene dos tipos, la efectividad total se calcula multiplicando ambos coeficientes base

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

### 6.2 Catálogo

- `BR-01`: no puede existir más de un Pokémon base con el mismo nombre canónico.
- `BR-02`: no puede existir más de un movimiento con el mismo nombre canónico.
- `BR-03`: un Pokémon base debe tener las seis estadísticas base informadas y con valor mayor que 0.
- `BR-04`: un Pokémon base debe tener al menos un tipo y como máximo dos, y todos sus tipos deben pertenecer al catálogo cerrado de la sección `6.1`.
- `BR-05`: un movimiento debe tener un tipo perteneciente al catálogo cerrado de la sección `6.1` y poder mayor que 0 si no es de estado. Para este MVP, como el cálculo de daño usa poder, se recomienda limitar el catálogo jugable a movimientos con poder numérico.
- `BR-06`: la relación `Pokémon base -> movimientos aprendibles` no puede contener duplicados.

### 6.3 Mis Pokémon

- `BR-07`: un Mi Pokémon debe referenciar una especie existente del catálogo base.
- `BR-08`: un Mi Pokémon debe tener nivel válido.
- `BR-09`: un Mi Pokémon debe tener `PS actuales <= PS totales`.
- `BR-10`: un Mi Pokémon puede tener entre `1` y `4` movimientos equipados.
- `BR-11`: todo movimiento equipado debe pertenecer a la lista de movimientos posibles de su especie.
- `BR-12`: un Mi Pokémon no puede equipar el mismo movimiento dos veces.

### 6.4 Cálculo de daño

- `BR-13`: el factor `Random` es un entero entre `85` y `100`, ambos inclusive.
- `BR-14`: la efectividad base de un movimiento debe obtenerse exclusivamente de la matriz de la sección `6.1`, cruzando tipo atacante contra tipo defensor.
- `BR-15`: si el Pokémon defensor tiene dos tipos, la efectividad total debe calcularse multiplicando ambos coeficientes base.
- `BR-16`: si la efectividad total es `0`, el daño final debe ser `0`.
- `BR-17`: el daño no puede reducir los PS por debajo de `0`.
- `BR-18`: para el MVP no se consideran críticos, STAB, precisión, evasión ni estados.

### 6.5 Combate

- `BR-19`: una partida solo puede iniciarse con exactamente 2 Pokémon adversarios válidos.
- `BR-20`: ninguno de los 2 Pokémon puede iniciar una partida con PS actuales en `0`.
- `BR-21`: una fase de combate debe dejar trazabilidad del atacante, defensor, movimiento, random, efectividad base, efectividad total, daño y PS restantes.
- `BR-22`: la partida termina cuando los PS actuales de uno de los Pokémon llegan a `0`.
- `BR-23`: una vez finalizada la partida no se pueden ejecutar más fases.

## 7. Dataset funcional recomendado para el MVP

## 7.1 Objetivo del dataset

El conjunto de 10 Pokémon debe permitir:

- probar CRUD de especies
- probar CRUD de movimientos
- probar aprendizaje y equipamiento
- probar movimientos compartidos
- probar ventajas, resistencias e inmunidades
- probar atacantes físicos y especiales
- probar combates con resultados variados

## 7.2 Pokémon recomendados

Se recomienda este roster base:

1. `Charizard`
   Motivo: atacante especial, tipo dual, acceso a fuego, volador y movimientos compartidos como `Hyper Beam`.
2. `Blastoise`
   Motivo: tanque de agua, acceso a `Surf`, `Hydro Pump`, `Ice Beam` y `Earthquake`.
3. `Venusaur`
   Motivo: control y estado con `Sleep Powder`, acceso a hierba y veneno.
4. `Pikachu`
   Motivo: eléctrico icónico, fácil de entender y útil para casos de ventaja simple contra agua.
5. `Gengar`
   Motivo: introduce inmunidad y daño especial de tipo fantasma.
6. `Golem`
   Motivo: introduce tierra/roca, `Earthquake`, resistencia e inmunidad eléctrica como defensor.
7. `Alakazam`
   Motivo: caso claro de atacante especial de tipo psíquico.
8. `Machamp`
   Motivo: caso claro de atacante físico de tipo lucha.
9. `Dragonite`
   Motivo: dual type, gran variedad de movimientos y cobertura elemental.
10. `Snorlax`
    Motivo: alto HP, caso de tanque normal y fuerte candidato a compartir movimientos como `Hyper Beam`.

## 7.3 Cobertura funcional del roster

Este roster cubre:

- ventajas simples: `Electric > Water`, `Fire > Grass`, `Psychic > Fighting`
- resistencias: por ejemplo `Grass` resistido por `Charizard`
- inmunidades: por ejemplo ataques `Normal` contra `Gengar`, y ataques `Electric` contra `Golem` si se soportan tipos duales de forma completa
- movimientos compartidos: `Protect`, `Hyper Beam`, `Earthquake`, `Surf` en varios casos del roster o del catálogo asociado
- diversidad de velocidades, HP y daño

## 7.4 Catálogo de movimientos jugables recomendado

No es necesario importar todos los movimientos del sitio de referencia para el MVP. Es preferible cargar un subconjunto curado, suficiente para cubrir reglas y consultas.

Subconjunto recomendado:

- `Flamethrower`
- `Fire Blast`
- `Fly`
- `Hyper Beam`
- `Solar Beam`
- `Surf`
- `Hydro Pump`
- `Ice Beam`
- `Sleep Powder`
- `Seed Bomb`
- `Sludge Wave`
- `Thunderbolt`
- `Discharge`
- `Shadow Ball`
- `Dark Pulse`
- `Poison Jab`
- `Psychic`
- `Close Combat`
- `Drain Punch`
- `Earthquake`
- `Bulldoze`
- `Protect`
- `Air Slash`
- `Thunder Punch`
- `Ice Punch`
- `Body Slam`
- `Rest`

Este conjunto es suficiente para:

- demostrar aprendizaje posible
- demostrar equipamiento de hasta 4 movimientos
- demostrar movimientos compartidos
- demostrar daño físico y especial
- demostrar efectos de tipo

## 8. Casos de uso funcionales

## 8.1 Catálogo de Pokémon base

### UC-01 Crear Pokémon base

- Objetivo: dar de alta una especie jugable del catálogo.
- Actor principal: administrador del catálogo o proceso de seed.
- Precondiciones:
  - el nombre no existe previamente
  - los tipos existen en el sistema o pertenecen al catálogo cerrado de la sección `6.1`
- Flujo principal:
  1. el actor informa nombre, tipo o tipos y estadísticas base
  2. el sistema valida unicidad y rango de datos
  3. el sistema crea la especie
  4. el sistema devuelve el identificador y el recurso creado
- Reglas aplicables: `BR-01`, `BR-03`, `BR-04`
- Alternativas y errores:
  - nombre duplicado
  - estadísticas no válidas
  - más de dos tipos

### UC-02 Consultar listado y detalle de Pokémon base

- Objetivo: recuperar especies del catálogo.
- Actor principal: consumidor de API.
- Precondiciones: ninguna.
- Flujo principal:
  1. el actor solicita listado o detalle
  2. el sistema devuelve especies con sus datos base
- Variantes útiles:
  - filtrar por tipo
  - filtrar por nombre
  - paginar

### UC-03 Actualizar Pokémon base

- Objetivo: modificar datos canónicos de una especie.
- Actor principal: administrador del catálogo.
- Precondiciones:
  - la especie existe
- Flujo principal:
  1. el actor envía cambios
  2. el sistema valida consistencia
  3. el sistema persiste los cambios
- Reglas aplicables: `BR-01`, `BR-03`, `BR-04`

### UC-04 Eliminar Pokémon base

- Objetivo: retirar una especie del catálogo.
- Actor principal: administrador del catálogo.
- Precondiciones:
  - la especie existe
  - no está siendo usada por Mis Pokémon o el sistema define una estrategia de borrado controlado
- Flujo principal:
  1. el actor solicita borrado
  2. el sistema valida dependencias
  3. el sistema elimina o rechaza la operación
- Regla recomendada:
  - impedir borrado físico si existen referencias activas

## 8.2 Catálogo de movimientos

### UC-05 Crear movimiento

- Objetivo: registrar un movimiento disponible para aprendizaje y combate.
- Actor principal: administrador del catálogo.
- Precondiciones:
  - el nombre no existe previamente
- Flujo principal:
  1. el actor informa nombre, tipo, categoría y poder
  2. el sistema valida los datos
  3. el sistema registra el movimiento
- Reglas aplicables: `BR-02`, `BR-05`

### UC-06 Consultar listado y detalle de movimientos

- Objetivo: consultar el catálogo de movimientos.
- Actor principal: consumidor de API.
- Precondiciones: ninguna.
- Flujo principal:
  1. el actor solicita el listado o un detalle
  2. el sistema devuelve los datos del movimiento
- Variantes útiles:
  - filtrar por tipo
  - filtrar por categoría
  - buscar por nombre

### UC-07 Actualizar movimiento

- Objetivo: corregir o enriquecer datos de un movimiento.
- Actor principal: administrador del catálogo.
- Precondiciones:
  - el movimiento existe
- Flujo principal:
  1. el actor envía cambios
  2. el sistema valida consistencia
  3. el sistema actualiza el movimiento

### UC-08 Eliminar movimiento

- Objetivo: retirar un movimiento del catálogo.
- Actor principal: administrador del catálogo.
- Precondiciones:
  - el movimiento existe
  - no está asignado a Mis Pokémon activos o el sistema resuelve la dependencia
- Flujo principal:
  1. el actor solicita borrado
  2. el sistema valida referencias
  3. el sistema elimina o bloquea la operación

## 8.3 Aprendizaje y relaciones de catálogo

### UC-09 Asociar movimientos aprendibles a un Pokémon base

- Objetivo: definir qué movimientos puede aprender cada especie.
- Actor principal: administrador del catálogo o proceso de importación inicial.
- Precondiciones:
  - existen la especie y los movimientos
- Flujo principal:
  1. el actor selecciona una especie
  2. el actor añade o retira movimientos aprendibles
  3. el sistema valida que no haya duplicados
  4. el sistema guarda la relación
- Reglas aplicables: `BR-06`
- Observación:
  - este caso de uso es imprescindible para soportar las consultas pedidas en el PDF

### UC-10 Consultar movimientos posibles de un Pokémon base

- Objetivo: recuperar el conjunto de movimientos que una especie puede aprender.
- Actor principal: consumidor de API.
- Precondiciones:
  - la especie existe
- Flujo principal:
  1. el actor solicita los movimientos posibles de una especie
  2. el sistema devuelve la lista de movimientos aprendibles
- Regla aplicable:
  - la fuente del resultado es la relación `Pokémon base -> movimientos aprendibles`

### UC-11 Consultar Pokémon que comparten un mismo movimiento

- Objetivo: obtener las especies que pueden aprender o comparten un movimiento determinado.
- Actor principal: consumidor de API.
- Precondiciones:
  - el movimiento existe
- Flujo principal:
  1. el actor selecciona un movimiento
  2. el sistema busca todas las especies asociadas
  3. el sistema devuelve la lista de Pokémon base
- Valor funcional:
  - cubre la consulta explícita del PDF
  - permite probar correctamente la relación muchos-a-muchos

## 8.4 Gestión de Mis Pokémon

### UC-12 Crear Mi Pokémon

- Objetivo: registrar una instancia jugable basada en una especie del catálogo.
- Actor principal: usuario de la API o proceso de prueba.
- Precondiciones:
  - existe la especie base
  - existe al menos un movimiento aprendible para esa especie
- Flujo principal:
  1. el actor selecciona la especie
  2. informa nivel y, si procede, PS iniciales
  3. selecciona entre 1 y 4 movimientos aprendibles
  4. el sistema valida y crea la instancia
- Reglas aplicables: `BR-07`, `BR-08`, `BR-09`, `BR-10`, `BR-11`, `BR-12`

### UC-13 Consultar Mis Pokémon

- Objetivo: listar y consultar el detalle de las instancias jugables registradas.
- Actor principal: consumidor de API.
- Precondiciones: ninguna.
- Flujo principal:
  1. el actor solicita listado o detalle
  2. el sistema devuelve especie, nivel, PS y movimientos equipados

### UC-14 Actualizar Mi Pokémon

- Objetivo: modificar una instancia jugable.
- Actor principal: usuario de la API.
- Flujo principal:
  1. el actor cambia nivel, PS o movimientos equipados
  2. el sistema valida consistencia
  3. el sistema actualiza el recurso
- Reglas aplicables: `BR-08`, `BR-09`, `BR-10`, `BR-11`, `BR-12`

### UC-15 Eliminar Mi Pokémon

- Objetivo: retirar una instancia jugable.
- Actor principal: usuario de la API.
- Precondiciones:
  - el Mi Pokémon existe
  - no participa en una partida activa
- Flujo principal:
  1. el actor solicita borrado
  2. el sistema valida si está en uso
  3. el sistema elimina o bloquea

### UC-16 Consultar movimientos equipados de Mi Pokémon

- Objetivo: obtener los movimientos actuales de una instancia jugable.
- Actor principal: consumidor de API.
- Precondiciones:
  - el Mi Pokémon existe
- Flujo principal:
  1. el actor solicita los movimientos del Mi Pokémon
  2. el sistema devuelve los movimientos equipados

## 8.5 Cálculo de daño

### UC-17 Calcular daño de un movimiento

- Objetivo: devolver el daño que causaría un movimiento elegido desde un atacante a un rival.
- Actor principal: consumidor de API o motor de combate.
- Precondiciones:
  - existen atacante y rival
  - el atacante tiene equipado el movimiento o el sistema permite calcular en modo simulación
  - ambos tienen PS mayores que 0
- Flujo principal:
  1. el actor informa atacante, rival y movimiento
  2. el sistema identifica nivel, estadística ofensiva, estadística defensiva, poder, tipo del movimiento y tipo o tipos del defensor
  3. el sistema consulta la matriz normativa de efectividad
  4. si el defensor tiene dos tipos, el sistema multiplica ambos coeficientes
  5. el sistema genera el factor random `85..100`
  6. el sistema aplica la fórmula del PDF
  7. el sistema devuelve el daño y el detalle del cálculo, incluidos coeficientes base y efectividad total
- Reglas aplicables: `BR-13`, `BR-14`, `BR-15`, `BR-16`, `BR-17`, `BR-18`
- Resultado esperado:
  - no altera estado si se usa en modo simulación
  - puede ser reutilizado por el motor de combate real

## 8.6 Partida y combate

### UC-18 Crear partida de combate

- Objetivo: crear una nueva partida entre 2 Pokémon adversarios.
- Actor principal: consumidor de API.
- Precondiciones:
  - existen 2 Mis Pokémon válidos
  - ambos tienen PS mayores que 0
- Flujo principal:
  1. el actor selecciona Pokémon A y Pokémon B
  2. el sistema crea la partida con estado inicial
  3. el sistema fija el primer estado del combate
- Reglas aplicables: `BR-19`, `BR-20`

### UC-19 Consultar estado de la partida

- Objetivo: recuperar el estado actual de un combate.
- Actor principal: consumidor de API.
- Precondiciones:
  - la partida existe
- Flujo principal:
  1. el actor consulta la partida
  2. el sistema devuelve:
     - combatientes
     - PS actuales
     - fase o turno
     - estado `Created`, `InProgress`, `Finished`
     - histórico de acciones si existe

### UC-20 Ejecutar una fase de combate

- Objetivo: avanzar la partida una fase aplicando un movimiento.
- Actor principal: consumidor de API o cliente de juego.
- Precondiciones:
  - la partida existe
  - la partida no está finalizada
  - el atacante seleccionado pertenece a la partida
  - el movimiento pertenece al set equipado del atacante
- Flujo principal:
  1. el actor indica qué Pokémon ataca y con qué movimiento
  2. el sistema valida la acción
  3. el sistema invoca el cálculo de daño
  4. el sistema descuenta PS al defensor
  5. el sistema registra la fase con todos los datos del cálculo, incluidos los coeficientes de efectividad usados
  6. el sistema comprueba si el defensor ha llegado a 0 PS
  7. si no termina la partida, el sistema deja lista la siguiente fase
- Reglas aplicables: `BR-17`, `BR-21`, `BR-22`, `BR-23`

### UC-21 Finalizar partida por KO

- Objetivo: cerrar la partida cuando uno de los Pokémon queda a 0 PS.
- Actor principal: sistema.
- Disparador:
  - ocurre al final de una fase de combate
- Flujo principal:
  1. el sistema detecta que uno de los Pokémon ha quedado con `0` PS
  2. marca la partida como `Finished`
  3. informa ganador y perdedor
  4. impide nuevas fases

### UC-22 Consultar histórico de fases de combate

- Objetivo: reconstruir lo ocurrido en la partida.
- Actor principal: consumidor de API, QA o cliente visual.
- Precondiciones:
  - la partida existe
- Flujo principal:
  1. el actor solicita el histórico
  2. el sistema devuelve una secuencia ordenada de fases
  3. cada fase incluye atacante, movimiento, random, efectividad base, efectividad total, daño y PS resultantes

## 9. Escenarios funcionales mínimos que el MVP debe soportar

## 9.1 Escenario de catálogo

1. crear los 10 Pokémon base del MVP
2. crear el subconjunto curado de movimientos
3. asociar movimientos aprendibles a cada especie
4. consultar qué Pokémon comparten `Protect`
5. consultar movimientos posibles de `Blastoise`

## 9.2 Escenario de Mis Pokémon

1. crear un `Mi Pokémon` basado en `Charizard`
2. equiparle exactamente 4 movimientos válidos
3. consultar sus movimientos equipados
4. intentar equipar un quinto movimiento y recibir rechazo funcional

## 9.3 Escenario de cálculo de daño

1. seleccionar `Pikachu` atacante
2. seleccionar `Thunderbolt`
3. seleccionar `Blastoise` rival
4. verificar en la tabla de la sección `6.1` que `Eléctrico -> Agua = 2`
5. calcular daño con ventaja de tipo
6. repetir con random diferente y observar variación del 15%

## 9.4 Escenario de resistencia e inmunidad basado en la tabla

1. seleccionar un atacante de tipo `Planta`
2. calcular contra `Charizard` y verificar una resistencia global de `0.25`, resultado de `Planta -> Fuego = 0.5` y `Planta -> Volador = 0.5`
3. seleccionar un atacante eléctrico
4. calcular contra `Golem` y verificar en la tabla de la sección `6.1` que `Eléctrico -> Tierra = 0`
5. obtener daño `0`, aunque el segundo tipo del defensor no sea inmune

## 9.5 Escenario de combate completo

1. crear una partida entre `Machamp` y `Snorlax`
2. ejecutar fases consecutivas
3. recalcular y persistir PS tras cada movimiento
4. finalizar cuando uno llegue a `0` PS
5. consultar el histórico completo

## 10. Requisitos funcionales derivados que conviene reflejar en la API

Aunque el PDF no entra en detalle HTTP, para que la API sea usable el MVP necesita además:

- identificadores estables para Pokémon base, movimientos, Mis Pokémon y partidas
- respuestas de error de validación explícitas
- imposibilidad de equipar movimientos no aprendibles
- imposibilidad de ejecutar acciones de combate sobre partidas finalizadas
- resolución de efectividad basada en la tabla normativa del PDF
- trazabilidad del cálculo de daño

## 11. Riesgos funcionales y puntos a confirmar

### Riesgo 1: el modelo actual solo admita un tipo

Impacto:

- se pierde fidelidad respecto a la fuente de catálogo
- algunas efectividades e inmunidades del roster propuesto no se comportarán como se espera

### Riesgo 2: no modelar categoría del movimiento

Impacto:

- las estadísticas especiales del Pokémon quedarían sin uso real
- el cálculo de daño sería inconsistente con el modelo de datos del PDF

### Riesgo 3: confundir especie con instancia

Impacto:

- se mezclaría catálogo base con estado mutable
- sería difícil mantener combates y CRUD con integridad

### Riesgo 4: querer importar todo el catálogo desde el primer día

Impacto:

- aumenta mucho el esfuerzo de seed y QA
- no aporta valor al MVP

Decisión recomendada:

- catálogo base de 10 Pokémon
- subconjunto curado de movimientos
- arquitectura preparada para crecer después

### Riesgo 5: no materializar la tabla de efectividad como contrato explícito

Impacto:

- backend, QA y documentación podrían tomar decisiones distintas para una misma combinación de tipos
- las regresiones de daño serían difíciles de detectar al ampliar catálogo o añadir más escenarios

## 12. Priorización recomendada de implementación

### Fase A

- CRUD de Pokémon base
- CRUD de movimientos
- asociación de movimientos aprendibles por especie

### Fase B

- CRUD de Mis Pokémon
- consulta de movimientos equipados
- consulta de movimientos posibles
- consulta de Pokémon que comparten movimiento

### Fase C

- servicio de cálculo de daño
- implementación de la matriz de efectividad y la resolución multi-tipo
- creación de partidas
- ejecución de fases
- consulta de estado e histórico

## 13. Trazabilidad resumida requisito -> casos de uso

- `Parte 1 PDF` -> `UC-17` + matriz normativa de la sección `6.1`
- `Parte 2 PDF` -> `UC-01` a `UC-16`
- `Parte 3 PDF` -> `UC-18` a `UC-22`

## 14. Fuentes funcionales de referencia

- PDF de requisitos: `docs/requirements/POKÉMON 2.pdf`
- Catálogo general de Pokémon: `https://pokemondb.net/pokedex/all`
- Catálogo general de movimientos: `https://pokemondb.net/move/all`
- Páginas individuales verificadas durante el análisis:
  - `https://pokemondb.net/pokedex/charizard`
  - `https://pokemondb.net/pokedex/blastoise`
  - `https://pokemondb.net/pokedex/venusaur`
  - `https://pokemondb.net/pokedex/pikachu`
  - `https://pokemondb.net/pokedex/gengar`
  - `https://pokemondb.net/pokedex/golem`
  - `https://pokemondb.net/pokedex/alakazam`
  - `https://pokemondb.net/pokedex/machamp`
  - `https://pokemondb.net/pokedex/dragonite`
  - `https://pokemondb.net/pokedex/snorlax`
