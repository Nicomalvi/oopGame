# Resumen general
Pequeño motor de platformer 2D creado para analizar programación orientada a objetos en C#.

## Arquitectura

![Diagrama de clases del proyecto](docs/diagramaOopProject.png)

## Clases para FÍSICAS
### PhysicalEntity
Representa una entidad física del mundo. Contiene su posición, velocidad y una hitbox. 
No conoce el mapa ni otras entidades con las que puede chocar.

### Hitbox
Por ahora basicamente un rectángulo con un offset.

### MapGrid
Contiene una matriz de celdas con physEnts y cierta data adicional (tamaño de cada "celda", etc...).

Si A quiere moverse a un punto P no necesito pasar por toda PhysEnt a ver si hay colisión,
analizo las celdas que ocupará A si pisa P en O(1) con la matriz. 

Mientras más pequeñas sean las celdas, mayor será el consumo de memoria, pero menor será la cantidad de entidades candidatas a colisionar.

### PhysicsSystem
Centraliza toda la lógica de movimiento y colisiones.

Conoce el MapGrid y las PhysicalEntity, aplica gravedad, mueve las entidades resolviendo colisiones y actualiza la estructura espacial del mapa.

Actúa como mediador entre PhysicalEntity y MapGrid, evitando dependencias directas entre ambas clases y concentrando toda la lógica física en un único lugar.

## Clases para ACTORES
### Actor
Un actor tiene una PhysEnt asociada, datos relevantes para sus acciones (movespeed, cuanto tiempo dura mi salto...)
y una Behavior. 

Cada tick, el actor le ordena a la PhysEnt cómo moverse según lo dice su behavior. 

Ciertas acciones cambian el State del actor, por ejemplo atacar causa que durante 2 segundos uno pueda saltar o moverme.

Dicha implementación de estados facilita muchísimo agregar sprites y animaciones en el futuro.

### Behavior
Una behavior realiza ciertos chequeos en base a lo que conoce del actor y le ordena al mismo qué acción realizar.
Por ejemplo, la behavior del player simplemente lee el teclado para ver hacia dónde moverse o cuándo atacar, mientras que
la behavior de un enemigo ordenaría constantemente moverse hacia cierto punto P (donde estaría parado el player).

### Ability
Clase base abstracta que representa una capacidad que un Actor puede tener o no (moverse, saltar, atacar, etc.), en vez de que el Actor implemente todo ese comportamiento directamente.

## Clases para GRÁFICOS
### EntitySprite
Representa la parte puramente visual de una entidad: su textura, posición en pantalla y qué porción del spritesheet (source) hay que dibujar.

### SpritePositionSystem
Sincroniza la posición de cada EntitySprite con la de su PhysicalEntity correspondiente, para que el sprite siempre se dibuje donde está realmente la entidad física.

### AnimationPlayer
Conoce como actualizar a lo largo del tiempo un EntitySprite para lograr una animación. Mueve la fuente del sprite en la sprite sheet según los timers que le pasen.

### AnimationTimer
La unidad más chica del sistema de animación: sabe cuánto dura cada frame de una animación puntual y cuál es el frame actual.

### AnimationController
Actúa como mediador entre Actor y AnimationPlayer, similar a como PhysicsSystem media entre PhysicalEntity y MapGrid.
Cada tick, se fija si el ActorState cambió desde el último frame; si cambió, le avisa al AnimationPlayer para que cambie de animación, y si no, simplemente lo deja avanzar.
También le pasa el facing del actor para que el player sepa si tiene que dibujar el sprite espejado.

## Clases misceláneas
### LevelLoader
Se encarga de traducir un mapa en texto (una grilla de caracteres) a entidades reales del juego. Facilita la creación de niveles y el testing.

## Filosofía de diseño
PhysicalEntity únicamente conoce física.

Actor únicamente conoce reglas de juego y comportamiento.

PhysicsSystem resuelve movimientos y colisiones físicas.

Behavior decide qué quiere hacer un actor.

Ability resuelve cómo se ejecuta cada acción concreta del actor.

EntitySprite únicamente conoce su representación visual.

AnimationController resuelve la sincronización entre estado del actor y animación.

Cada clase tiene una única responsabilidad, lo que facilita extender el motor con nuevas entidades, IA y mecánicas sin modificar el resto del sistema.

