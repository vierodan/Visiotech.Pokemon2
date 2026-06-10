# Prompts de IA para implementar los casos de uso del MVP Pokémon

## 1. Objetivo del fichero

Este documento contiene prompts reutilizables para pedir a una IA la implementación de cada caso de uso definido en la sección `8` de [docs/functionality/use-cases.md](./use-cases.md).

Cada prompt obliga a:

- leer `docs/functionality/use-cases.md` completo antes de tocar código
- implementar el caso de uso de extremo a extremo
- cubrir los escenarios funcionales mínimos del apartado `9` que apliquen
- reflejar los requisitos derivados del apartado `10`
- mitigar en lo posible los riesgos del apartado `11`

## 2. Contexto común obligatorio para todos los prompts

Antes de usar cualquiera de los prompts de este fichero, la IA debe asumir este contexto común:

- el documento fuente de verdad es `docs/functionality/use-cases.md`
- el alcance es el MVP descrito en ese documento y no debe ampliarse sin necesidad
- debe respetarse obligatoriamente `Clean Architecture` en la implementación, adaptándose a la arquitectura ya existente del proyecto y evitando atajos que rompan sus límites o responsabilidades
- deben respetarse obligatoriamente los principios `SOLID` en diseño, modelado, orquestación de casos de uso y construcción de componentes
- deben aplicarse obligatoriamente las mejores prácticas de software en código, validaciones, manejo de errores, pruebas, mantenibilidad, legibilidad, cohesión, bajo acoplamiento y evolución futura del sistema
- la persistencia en infraestructura debe implementarse con `Entity Framework Core`, usando migraciones como mecanismo oficial de evolución del esquema
- la base de datos objetivo es `PostgreSQL`, por lo que el modelado, mapeo, tipos de datos, constraints e índices deben pensarse para ese motor
- si existe conflicto entre ejemplos concretos y la tabla de efectividad de la sección `6.1`, prevalece la tabla
- el modelo debe soportar Pokémon base con `1..2` tipos
- el movimiento debe tener `tipo`, `categoría` (`Physical` o `Special`) y `poder`
- `Pokémon base` y `Mi Pokémon` son entidades distintas y no deben mezclarse
- el cálculo de daño debe usar la matriz normativa de efectividad, soportar multi-tipo y registrar trazabilidad
- no deben introducirse mecánicas fuera de alcance del MVP: `PP`, precisión, críticos, `STAB`, estados alterados, ítems, habilidades o buffs/debuffs
- el MVP debe trabajar con el roster curado de `10` Pokémon y el subconjunto recomendado de movimientos, pero sin impedir crecimiento futuro
- la API debe exponer identificadores estables
- la API debe devolver errores de validación explícitos
- la API no debe permitir equipar movimientos no aprendibles
- la API no debe permitir acciones de combate sobre partidas finalizadas
- toda implementación debe incluir pruebas automatizadas del comportamiento nuevo o modificado
- la IA debe inspeccionar primero el código existente y adaptarse a su arquitectura, estilo y stack sin vulnerar `Clean Architecture`, `SOLID` ni las mejores prácticas de software

### 2.1 Barra de calidad obligatoria para todos los prompts

Toda implementación pedida por cualquiera de los prompts de este fichero debe cumplir además este listón mínimo obligatorio de calidad:

- debe ser software apto para producción, no una demo frágil ni una prueba de concepto improvisada
- no se admiten parches rápidos, workarounds, deuda técnica evitable, comentarios `TODO`, código muerto, duplicaciones innecesarias ni soluciones a medio terminar
- toda regla de negocio debe quedar ubicada en la capa correcta y no repartida de forma arbitraria entre controladores, repositorios, servicios o mapeos
- el dominio debe proteger sus invariantes de forma explícita y no confiar en que otras capas validen siempre correctamente
- los contratos de entrada y salida deben ser claros, coherentes, versionables y no deben filtrar accidentalmente detalles internos de persistencia
- la implementación debe minimizar acoplamiento, maximizar cohesión y evitar dependencias circulares, lógica duplicada y conocimiento transversal innecesario
- toda validación debe ser explícita, mantenible y acompañada de errores comprensibles, deterministas y trazables
- la seguridad debe ser `secure by default`: no confiar en datos de entrada, no exponer más superficie de la necesaria, no aceptar estados inválidos y no filtrar información sensible en errores, logs o respuestas
- deben revisarse riesgos de `mass assignment`, validación insuficiente, referencias inexistentes, manipulación de identificadores, inconsistencias transaccionales y estados imposibles
- la persistencia debe preservar integridad, atomicidad lógica y consistencia; cuando aplique, deben contemplarse transacciones, concurrencia y condiciones de carrera
- la implementación debe favorecer observabilidad suficiente: trazabilidad útil, logging estructurado cuando tenga sentido y capacidad de diagnóstico sin ruido innecesario
- el código debe ser fácil de leer, razonar, probar y extender; nombres, responsabilidades y dependencias deben dejar clara la intención
- toda decisión técnica debe buscar el mínimo diseño que resuelva bien el caso de uso hoy sin hipotecar crecimiento razonable mañana
- deben evitarse regresiones funcionales sobre casos ya soportados y sobre los escenarios mínimos del documento fuente
- la cobertura de pruebas debe ser proporcional al riesgo: unitarias para reglas de negocio, integración para persistencia y flujos, y de contrato o API cuando el caso de uso lo justifique
- si el proyecto ya tiene patrones de autorización, validación, mediación, manejo de errores, mapeo, persistencia o testing, deben respetarse y reforzarse en lugar de crear variantes innecesarias

## 3. Forma de trabajar esperada para la IA

Todos los prompts de este fichero implican estas expectativas de ejecución:

1. leer de principio a fin `docs/functionality/use-cases.md`
2. inspeccionar el código existente y localizar las capas implicadas
3. diseñar la solución mínima completa respetando la arquitectura actual
4. implementar código, validaciones, persistencia, contratos y pruebas
5. verificar el resultado ejecutando las pruebas relevantes
6. resumir cambios, decisiones, supuestos y limitaciones reales

### 3.1 Definición de terminado obligatoria

Ningún prompt debe considerarse bien resuelto si no se cumple todo esto:

- la solución implementa el caso de uso completo de extremo a extremo
- la solución respeta `Clean Architecture`, `SOLID` y el estilo existente del proyecto
- la solución no introduce vulnerabilidades obvias ni degrada la postura de seguridad
- la solución no rompe escenarios mínimos ya cubiertos o derivados del documento funcional
- la solución incluye validaciones, manejo de errores y persistencia coherentes con el riesgo del caso
- la solución incluye pruebas automatizadas relevantes y esas pruebas se han ejecutado
- la solución evita deuda técnica innecesaria y no deja trabajo crítico sin cerrar
- la solución deja trazabilidad suficiente para depuración y mantenimiento
- la solución queda documentada en el resumen final con cambios, pruebas ejecutadas, supuestos y límites reales

### 3.2 Entregables obligatorios al cerrar cualquier prompt

La IA debe cerrar cada implementación aportando, como mínimo:

- un resumen breve de la solución arquitectónica adoptada
- una explicación de dónde quedó cada responsabilidad principal
- la lista real de validaciones y guardas de seguridad implementadas
- la lista real de pruebas añadidas o modificadas
- las pruebas ejecutadas y su resultado
- cualquier decisión relevante sobre persistencia, transacciones, concurrencia o consistencia
- cualquier limitación real, si existe, sin ocultarla ni minimizarla

## 4. Escenarios funcionales mínimos del apartado 9

Estos escenarios deben usarse como referencia obligatoria al redactar y ejecutar los prompts:

### 9.1 Escenario de catálogo

1. crear los `10` Pokémon base del MVP
2. crear el subconjunto curado de movimientos
3. asociar movimientos aprendibles a cada especie
4. consultar qué Pokémon comparten `Protect`
5. consultar movimientos posibles de `Blastoise`

### 9.2 Escenario de Mis Pokémon

1. crear un `Mi Pokémon` basado en `Charizard`
2. equiparle exactamente `4` movimientos válidos
3. consultar sus movimientos equipados
4. intentar equipar un quinto movimiento y recibir rechazo funcional

### 9.3 Escenario de cálculo de daño

1. seleccionar `Pikachu` atacante
2. seleccionar `Thunderbolt`
3. seleccionar `Blastoise` rival
4. verificar en la tabla que `Eléctrico -> Agua = 2`
5. calcular daño con ventaja de tipo
6. repetir con `random` diferente y observar variación del `15%`

### 9.4 Escenario de resistencia e inmunidad basado en la tabla

1. seleccionar un atacante de tipo `Planta`
2. calcular contra `Charizard` y verificar resistencia global `0.25`
3. seleccionar un atacante eléctrico
4. calcular contra `Golem` y verificar que `Eléctrico -> Tierra = 0`
5. obtener daño `0`

### 9.5 Escenario de combate completo

1. crear una partida entre `Machamp` y `Snorlax`
2. ejecutar fases consecutivas
3. recalcular y persistir `PS` tras cada movimiento
4. finalizar cuando uno llegue a `0` PS
5. consultar el histórico completo

## 5. Prompts de implementación por caso de uso

## 5.1 Catálogo de Pokémon base

### Prompt UC-01 Crear Pokémon base

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-01 Crear Pokémon base`.

Objetivo funcional:
- dar de alta una especie jugable del catálogo
- validar unicidad del nombre
- validar que tenga `1..2` tipos permitidos por la tabla normativa
- validar las seis estadísticas base con valor mayor que `0`

Reglas obligatorias:
- `BR-01`, `BR-03`, `BR-04`

Escenarios mínimos a cubrir:
- escenario `9.1`, al menos los pasos `1` y `3`, asegurando que la alta de especies permite cargar el roster MVP

Requisitos derivados a reflejar:
- identificadores estables
- errores de validación explícitos

Riesgos a mitigar:
- Riesgo 1: no reducir el modelo a un solo tipo
- Riesgo 3: no confundir especie con instancia
- Riesgo 4: no forzar un catálogo completo, solo el roster curado necesario

Implementa la solución end-to-end sobre la arquitectura existente. Incluye contrato de entrada/salida, validaciones, persistencia, pruebas automatizadas y, si existe seed del proyecto, déjalo preparado para crear las especies del MVP. Ejecuta las pruebas relevantes y resume los cambios y cualquier supuesto real.
```

### Prompt UC-02 Consultar listado y detalle de Pokémon base

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-02 Consultar listado y detalle de Pokémon base`.

Objetivo funcional:
- recuperar especies del catálogo
- permitir listado y detalle
- soportar, si encaja con la arquitectura actual, filtros por tipo y nombre y paginación

Escenarios mínimos a cubrir:
- escenario `9.1`, verificando que tras crear el roster se puede listar y consultar detalle de especies

Requisitos derivados a reflejar:
- identificadores estables
- respuestas consistentes para listado y detalle
- errores explícitos cuando la especie no exista

Riesgos a mitigar:
- Riesgo 1: exponer correctamente uno o dos tipos
- Riesgo 3: no mezclar datos de `Pokémon base` con `Mi Pokémon`

Implementa el caso de uso completo adaptándolo al stack existente. Añade pruebas de listado, detalle, filtro y caso no encontrado. Ejecuta las pruebas relevantes y documenta cualquier decisión no obvia.
```

### Prompt UC-03 Actualizar Pokémon base

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-03 Actualizar Pokémon base`.

Objetivo funcional:
- modificar datos canónicos de una especie existente
- mantener unicidad del nombre y validez de tipos y estadísticas

Reglas obligatorias:
- `BR-01`, `BR-03`, `BR-04`

Escenarios mínimos a cubrir:
- escenario `9.1`, manteniendo la coherencia del roster MVP tras cambios sobre especies

Requisitos derivados a reflejar:
- identificadores estables
- errores de validación explícitos

Riesgos a mitigar:
- Riesgo 1: preservar soporte multi-tipo
- Riesgo 3: no permitir mutaciones sobre instancias jugables al actualizar la especie

Implementa endpoint o flujo equivalente, validaciones, persistencia y pruebas para actualización válida, especie inexistente, nombre duplicado y tipos inválidos. Ejecuta las pruebas relevantes y resume impacto sobre datos ya existentes.
```

### Prompt UC-04 Eliminar Pokémon base

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-04 Eliminar Pokémon base`.

Objetivo funcional:
- retirar una especie del catálogo
- impedir borrado físico si existen referencias activas desde `Mi Pokémon` o relaciones necesarias

Escenarios mínimos a cubrir:
- escenario `9.1`, asegurando que la gestión del catálogo no rompe el roster mínimo ni las relaciones de aprendizaje

Requisitos derivados a reflejar:
- identificadores estables
- errores de validación explícitos cuando haya dependencias

Riesgos a mitigar:
- Riesgo 3: integridad entre especie e instancia
- Riesgo 4: evitar soluciones costosas o demasiado generales para el MVP

Implementa la estrategia más segura compatible con la arquitectura actual: bloqueo de borrado, soft delete o alternativa equivalente, pero documentando la elección. Añade pruebas para borrado permitido y borrado rechazado por dependencias. Ejecuta las pruebas relevantes y resume la decisión tomada.
```

## 5.2 Catálogo de movimientos

### Prompt UC-05 Crear movimiento

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-05 Crear movimiento`.

Objetivo funcional:
- registrar un movimiento para aprendizaje y combate
- validar unicidad del nombre
- validar `tipo`, `categoría` y `poder`

Reglas obligatorias:
- `BR-02`, `BR-05`

Escenarios mínimos a cubrir:
- escenario `9.1`, paso `2`, permitiendo cargar el subconjunto curado de movimientos

Requisitos derivados a reflejar:
- identificadores estables
- errores de validación explícitos

Riesgos a mitigar:
- Riesgo 2: no omitir la categoría `Physical` o `Special`
- Riesgo 4: implementar solo lo necesario para el subconjunto curado, sin cerrar crecimiento futuro
- Riesgo 5: asegurar que el tipo del movimiento pertenece al catálogo normativo

Implementa el caso de uso end-to-end con validaciones, persistencia y pruebas para altas correctas y rechazos por nombre duplicado, tipo inválido, categoría inválida o poder inconsistente. Ejecuta las pruebas relevantes y resume los cambios.
```

### Prompt UC-06 Consultar listado y detalle de movimientos

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-06 Consultar listado y detalle de movimientos`.

Objetivo funcional:
- consultar el catálogo de movimientos
- soportar listado y detalle
- habilitar filtros por tipo, categoría o nombre si encaja con los patrones del proyecto

Escenarios mínimos a cubrir:
- escenario `9.1`, paso `2`, verificando que el subconjunto curado es consultable

Requisitos derivados a reflejar:
- identificadores estables
- errores explícitos para movimiento inexistente

Riesgos a mitigar:
- Riesgo 2: asegurar que se expone categoría de movimiento
- Riesgo 5: mantener tipos alineados con la tabla normativa

Implementa el flujo completo con pruebas para listado, detalle y filtros básicos. Ejecuta las pruebas relevantes y documenta cualquier límite funcional real.
```

### Prompt UC-07 Actualizar movimiento

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-07 Actualizar movimiento`.

Objetivo funcional:
- corregir o enriquecer datos de un movimiento existente
- preservar consistencia de tipo, categoría y poder

Reglas obligatorias:
- `BR-02`, `BR-05`

Escenarios mínimos a cubrir:
- escenario `9.1`, manteniendo coherencia con el subconjunto curado de movimientos

Requisitos derivados a reflejar:
- identificadores estables
- errores de validación explícitos

Riesgos a mitigar:
- Riesgo 2: no perder la categoría del movimiento al actualizar
- Riesgo 5: no permitir tipos fuera de la tabla normativa

Implementa actualización completa, con pruebas de éxito y rechazo por nombre duplicado, movimiento inexistente o datos inválidos. Ejecuta las pruebas relevantes y resume el resultado.
```

### Prompt UC-08 Eliminar movimiento

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-08 Eliminar movimiento`.

Objetivo funcional:
- retirar un movimiento del catálogo
- impedir eliminaciones que rompan relaciones con `Mi Pokémon` o con catálogo aprendible, salvo que exista una estrategia controlada

Escenarios mínimos a cubrir:
- escenario `9.1`, preservando integridad del subconjunto curado y de las asociaciones aprendibles
- escenario `9.2`, evitando dejar instancias con movimientos colgantes

Requisitos derivados a reflejar:
- errores de validación explícitos
- identificadores estables

Riesgos a mitigar:
- Riesgo 2: no eliminar metadatos necesarios para combate
- Riesgo 3: preservar integridad entre catálogo y `Mi Pokémon`
- Riesgo 4: no introducir una solución demasiado amplia para el MVP

Implementa la estrategia más segura según el proyecto actual y añade pruebas para borrado permitido y denegado por dependencias. Ejecuta las pruebas relevantes y deja claras las reglas de integridad aplicadas.
```

## 5.3 Aprendizaje y relaciones de catálogo

### Prompt UC-09 Asociar movimientos aprendibles a un Pokémon base

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-09 Asociar movimientos aprendibles a un Pokémon base`.

Objetivo funcional:
- definir qué movimientos puede aprender cada especie
- permitir añadir y retirar relaciones sin duplicados

Reglas obligatorias:
- `BR-06`

Escenarios mínimos a cubrir:
- escenario `9.1`, paso `3`
- preparar el terreno para `9.2`, ya que `Mi Pokémon` solo puede equipar movimientos aprendibles

Requisitos derivados a reflejar:
- errores de validación explícitos
- integridad referencial entre especies y movimientos

Riesgos a mitigar:
- Riesgo 3: no mezclar relaciones de especie con movimientos equipados de instancia
- Riesgo 4: priorizar el roster y subconjunto curado del MVP

Implementa el caso de uso completo, con pruebas para asociar, desasociar y rechazar duplicados o referencias inexistentes. Ejecuta las pruebas relevantes y resume las decisiones de modelado.
```

### Prompt UC-10 Consultar movimientos posibles de un Pokémon base

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-10 Consultar movimientos posibles de un Pokémon base`.

Objetivo funcional:
- devolver la lista de movimientos aprendibles de una especie
- tomar la información solo de la relación `Pokémon base -> movimientos aprendibles`

Escenarios mínimos a cubrir:
- escenario `9.1`, paso `5`, consultando movimientos posibles de `Blastoise`

Requisitos derivados a reflejar:
- identificadores estables
- errores explícitos si la especie no existe

Riesgos a mitigar:
- Riesgo 3: no confundir movimientos posibles con movimientos equipados

Implementa la consulta completa con pruebas para especie existente, especie inexistente y lista vacía si aplica. Ejecuta las pruebas relevantes y documenta el contrato de salida.
```

### Prompt UC-11 Consultar Pokémon que comparten un mismo movimiento

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-11 Consultar Pokémon que comparten un mismo movimiento`.

Objetivo funcional:
- obtener las especies que pueden aprender un movimiento concreto
- resolver correctamente la relación muchos-a-muchos

Escenarios mínimos a cubrir:
- escenario `9.1`, paso `4`, consultando qué Pokémon comparten `Protect`

Requisitos derivados a reflejar:
- identificadores estables
- errores explícitos si el movimiento no existe

Riesgos a mitigar:
- Riesgo 3: devolver especies del catálogo, no instancias de `Mi Pokémon`

Implementa la consulta completa con pruebas para movimiento existente, movimiento inexistente y comportamiento con varias especies asociadas. Ejecuta las pruebas relevantes y resume el resultado.
```

## 5.4 Gestión de Mis Pokémon

### Prompt UC-12 Crear Mi Pokémon

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-12 Crear Mi Pokémon`.

Objetivo funcional:
- registrar una instancia jugable basada en una especie del catálogo
- permitir nivel, `PS` iniciales válidos y entre `1` y `4` movimientos equipados
- validar que todos los movimientos equipados son aprendibles por la especie

Reglas obligatorias:
- `BR-07`, `BR-08`, `BR-09`, `BR-10`, `BR-11`, `BR-12`

Escenarios mínimos a cubrir:
- escenario `9.2`, pasos `1` y `2`

Requisitos derivados a reflejar:
- identificadores estables
- errores de validación explícitos
- imposibilidad de equipar movimientos no aprendibles

Riesgos a mitigar:
- Riesgo 3: separar claramente `Pokémon base` y `Mi Pokémon`
- Riesgo 4: centrar la implementación en el MVP y en el roster curado

Implementa el flujo completo con persistencia, validaciones y pruebas para creación válida, especie inexistente, nivel inválido, `PS` inconsistentes, movimiento no aprendible, duplicado y más de cuatro movimientos. Ejecuta las pruebas relevantes y resume las decisiones tomadas.
```

### Prompt UC-13 Consultar Mis Pokémon

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-13 Consultar Mis Pokémon`.

Objetivo funcional:
- listar y consultar el detalle de las instancias jugables registradas
- devolver especie base, nivel, `PS` y movimientos equipados

Escenarios mínimos a cubrir:
- escenario `9.2`, paso `3`, y dejar visible la información necesaria para `9.5`

Requisitos derivados a reflejar:
- identificadores estables
- respuestas consistentes para listado y detalle

Riesgos a mitigar:
- Riesgo 3: no mezclar el estado mutable de `Mi Pokémon` con datos canónicos de especie

Implementa la consulta con pruebas de listado, detalle y caso no encontrado. Ejecuta las pruebas relevantes y documenta el shape del recurso devuelto.
```

### Prompt UC-14 Actualizar Mi Pokémon

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-14 Actualizar Mi Pokémon`.

Objetivo funcional:
- modificar nivel, `PS` o movimientos equipados de una instancia jugable
- mantener todas las restricciones de validación de `Mi Pokémon`

Reglas obligatorias:
- `BR-08`, `BR-09`, `BR-10`, `BR-11`, `BR-12`

Escenarios mínimos a cubrir:
- escenario `9.2`, validando reequipado y rechazo del quinto movimiento

Requisitos derivados a reflejar:
- errores de validación explícitos
- imposibilidad de equipar movimientos no aprendibles

Riesgos a mitigar:
- Riesgo 3: actualizar solo la instancia, no la especie base

Implementa la actualización completa con pruebas para cambios válidos y rechazos por reglas funcionales. Ejecuta las pruebas relevantes y resume la cobertura añadida.
```

### Prompt UC-15 Eliminar Mi Pokémon

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-15 Eliminar Mi Pokémon`.

Objetivo funcional:
- retirar una instancia jugable
- impedir el borrado si participa en una partida activa

Escenarios mínimos a cubrir:
- escenario `9.2`, permitiendo limpieza de instancias no usadas
- escenario `9.5`, protegiendo partidas activas

Requisitos derivados a reflejar:
- errores de validación explícitos
- identificadores estables

Riesgos a mitigar:
- Riesgo 3: preservar integridad entre instancias y combate

Implementa la estrategia de borrado más segura acorde al código actual. Añade pruebas para borrado permitido, no encontrado y rechazo por partida activa. Ejecuta las pruebas relevantes y resume el resultado.
```

### Prompt UC-16 Consultar movimientos equipados de Mi Pokémon

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-16 Consultar movimientos equipados de Mi Pokémon`.

Objetivo funcional:
- obtener los movimientos actualmente equipados por una instancia jugable

Escenarios mínimos a cubrir:
- escenario `9.2`, paso `3`

Requisitos derivados a reflejar:
- identificadores estables
- errores explícitos si el `Mi Pokémon` no existe

Riesgos a mitigar:
- Riesgo 3: no devolver los movimientos posibles de la especie en lugar de los equipados de la instancia

Implementa la consulta completa con pruebas para instancia existente, inexistente y cardinalidad máxima de cuatro movimientos. Ejecuta las pruebas relevantes y documenta claramente el contrato.
```

## 5.5 Cálculo de daño

### Prompt UC-17 Calcular daño de un movimiento

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-17 Calcular daño de un movimiento`.

Objetivo funcional:
- calcular el daño que un movimiento causaría desde un atacante hacia un rival
- usar la fórmula del PDF
- usar obligatoriamente la tabla normativa de efectividad
- soportar defensor de uno o dos tipos
- usar `Attack/Defense` o `SpecialAttack/SpecialDefense` según la categoría del movimiento
- devolver trazabilidad del cálculo

Reglas obligatorias:
- `BR-13`, `BR-14`, `BR-15`, `BR-16`, `BR-17`, `BR-18`

Escenarios mínimos a cubrir:
- escenario `9.3` completo
- escenario `9.4` completo

Requisitos derivados a reflejar:
- resolución de efectividad basada en la tabla normativa
- trazabilidad del cálculo de daño
- errores explícitos cuando atacante, rival o movimiento no sean válidos

Riesgos a mitigar:
- Riesgo 1: soportar multi-tipo real
- Riesgo 2: usar correctamente la categoría del movimiento
- Riesgo 5: materializar la tabla como contrato explícito en código y pruebas

Implementa el servicio end-to-end y los contratos necesarios. Añade pruebas de ventaja simple, resistencia doble, inmunidad, categoría `Physical`, categoría `Special`, `random` en rango `85..100` y daño final nunca negativo ni por debajo de `0` PS. Si existe un motor de combate reutilizable, diseña la solución para que este caso de uso se pueda invocar desde el combate. Ejecuta las pruebas relevantes y resume cualquier decisión matemática o de redondeo.
```

## 5.6 Partida y combate

### Prompt UC-18 Crear partida de combate

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-18 Crear partida de combate`.

Objetivo funcional:
- crear una partida entre exactamente `2` `Mis Pokémon`
- inicializar estado de combate válido
- impedir iniciar con `PS` en `0`

Reglas obligatorias:
- `BR-19`, `BR-20`

Escenarios mínimos a cubrir:
- escenario `9.5`, paso `1`

Requisitos derivados a reflejar:
- identificadores estables para partidas
- errores de validación explícitos

Riesgos a mitigar:
- Riesgo 3: usar instancias jugables, no especies base

Implementa la creación de partidas completa con persistencia, estado inicial claro y pruebas para creación válida, jugadores inexistentes, ids repetidos si no se permiten y `PS` iniciales inválidos. Ejecuta las pruebas relevantes y resume la estrategia de modelado del estado.
```

### Prompt UC-19 Consultar estado de la partida

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-19 Consultar estado de la partida`.

Objetivo funcional:
- recuperar el estado actual de un combate
- devolver combatientes, `PS`, fase o turno, estado e histórico si existe

Escenarios mínimos a cubrir:
- escenario `9.5`, pasos `1`, `2` y `5`

Requisitos derivados a reflejar:
- identificadores estables
- trazabilidad de estado y errores explícitos si la partida no existe

Riesgos a mitigar:
- Riesgo 3: no confundir la lectura de combate con datos estáticos del catálogo

Implementa la consulta de estado con pruebas para partida nueva, partida en progreso, partida finalizada y partida inexistente. Ejecuta las pruebas relevantes y documenta el contrato de respuesta.
```

### Prompt UC-20 Ejecutar una fase de combate

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-20 Ejecutar una fase de combate`.

Objetivo funcional:
- avanzar una partida una fase
- validar que el atacante pertenece a la partida
- validar que el movimiento está equipado
- invocar el cálculo de daño
- descontar `PS`
- registrar trazabilidad completa
- preparar el siguiente estado o finalizar si hay `KO`

Reglas obligatorias:
- `BR-17`, `BR-21`, `BR-22`, `BR-23`

Escenarios mínimos a cubrir:
- escenario `9.5`, pasos `2`, `3` y `4`
- reutilizar las garantías de `9.3` y `9.4` cuando el movimiento o los tipos aplicables entren en combate

Requisitos derivados a reflejar:
- imposibilidad de ejecutar acciones sobre partidas finalizadas
- trazabilidad del cálculo de daño
- errores de validación explícitos

Riesgos a mitigar:
- Riesgo 1: respetar multi-tipo si afecta al daño
- Riesgo 2: respetar categoría del movimiento
- Riesgo 3: preservar integridad del estado mutable de combate
- Riesgo 5: registrar coeficientes de efectividad usados

Implementa la fase de combate completa con pruebas para acción válida, movimiento no equipado, atacante inválido, partida finalizada y transición a `KO`. Reutiliza el servicio de cálculo de daño en lugar de duplicar lógica. Ejecuta las pruebas relevantes y resume la solución final.
```

### Prompt UC-21 Finalizar partida por KO

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-21 Finalizar partida por KO`.

Objetivo funcional:
- cerrar automáticamente la partida cuando uno de los Pokémon queda a `0` PS
- marcar estado `Finished`
- informar ganador y perdedor
- impedir nuevas fases

Escenarios mínimos a cubrir:
- escenario `9.5`, paso `4`

Requisitos derivados a reflejar:
- imposibilidad de ejecutar acciones sobre partidas finalizadas
- identificadores estables y estado consistente

Riesgos a mitigar:
- Riesgo 3: no dejar estados intermedios inconsistentes

Implementa la lógica de finalización donde mejor encaje en la arquitectura existente, idealmente como parte del flujo de combate y no como lógica duplicada. Añade pruebas para cierre por `KO` y bloqueo de nuevas acciones. Ejecuta las pruebas relevantes y resume el comportamiento final.
```

### Prompt UC-22 Consultar histórico de fases de combate

```text
Lee de principio a fin `docs/functionality/use-cases.md` y usa también el contexto común obligatorio de `docs/functionality/use-cases-implementation-prompting.md`.

Actúa simultáneamente como `Senior Architect`, `Senior Developer` en `.NET` y `Senior Security Engineer` durante todo el trabajo.

Aplica obligatoriamente la barra de calidad de las secciones `2.1` y `3.1`. No entregues soluciones parciales, frágiles, temporales ni con deuda técnica evitable.

En infraestructura, usa `Entity Framework Core` con migraciones y `PostgreSQL` como base de datos.

Quiero que implementes el caso de uso `UC-22 Consultar histórico de fases de combate`.

Objetivo funcional:
- reconstruir lo ocurrido en una partida
- devolver fases ordenadas con atacante, movimiento, `random`, efectividad base, efectividad total, daño y `PS` resultantes

Escenarios mínimos a cubrir:
- escenario `9.5`, paso `5`
- asegurar que si el combate usa casos como `9.3` o `9.4`, el histórico conserva también esos detalles de efectividad

Requisitos derivados a reflejar:
- trazabilidad del cálculo de daño
- errores explícitos si la partida no existe

Riesgos a mitigar:
- Riesgo 5: no perder el contrato explícito de efectividad usado en cada fase
- Riesgo 3: no mezclar snapshots de especie con estado histórico mutable

Implementa la consulta del histórico con persistencia adecuada y pruebas para combate sin fases, combate con varias fases y partida inexistente. Ejecuta las pruebas relevantes y documenta el orden y formato del histórico.
```

## 6. Recomendación de uso

Si el objetivo es avanzar por incrementos pequeños y verificables, conviene usar los prompts en este orden:

1. `UC-01` a `UC-11`
2. `UC-12` a `UC-16`
3. `UC-17`
4. `UC-18` a `UC-22`

Ese orden reduce riesgo funcional porque primero consolida catálogo, después instancias jugables, luego el motor de daño y por último el combate con estado.
