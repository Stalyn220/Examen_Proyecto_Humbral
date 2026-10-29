# Proyecto Umbral

Videojuego de plataformas 2D desarrollado con Unity. El jugador controla a Leo, explora una estación dividida en seis sectores, invierte la gravedad, combate Centinelas y reúne tres módulos para rescatar a Mara y escapar por el portal.

## Contenido

- [Requisitos](#requisitos)
- [Abrir y jugar](#abrir-y-jugar)
- [Controles](#controles)
- [Objetivo y recorrido](#objetivo-y-recorrido)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Scripts principales](#scripts-principales)
- [Editar y ampliar el juego](#editar-y-ampliar-el-juego)
- [Pruebas y compilación](#pruebas-y-compilación)
- [Limitaciones conocidas](#limitaciones-conocidas)
- [Documentación adicional](#documentación-adicional)

## Requisitos

- Unity **6000.6.4f1**. La versión exacta del proyecto está registrada en `ProjectSettings/ProjectVersion.txt`.
- Windows 64 bits para ejecutar la compilación incluida o generar una nueva compilación de Windows.
- Los paquetes del proyecto se restauran desde Unity Package Manager, según `Packages/manifest.json`.

No se necesitan paquetes externos de NuGet para abrir o ejecutar el juego en Unity. Los archivos `.csproj` y la solución `.slnx` permiten trabajar con el código desde un IDE, pero Unity es quien importa y ejecuta el proyecto.

## Abrir y jugar

### Desde Unity

1. Abre Unity Hub y agrega la carpeta raíz del proyecto.
2. Ábrelo con Unity 6000.6.4f1 y espera a que termine la importación.
3. Abre `Assets/Umbral/Scenes/Umbral.unity`.
4. Pulsa **Play** y luego **Enter** o el botón **INICIAR**.

Si los controles no responden en el Editor, haz clic en la ventana **Game** para darle el foco.

### Ejecutable de Windows

La compilación está en `Builds/Windows/Proyecto Umbral.exe`. Conserva el ejecutable junto con `Proyecto Umbral_Data`, `UnityPlayer.dll` y los demás archivos de esa carpeta; el ejecutable depende de ellos.

## Controles

| Tecla | Acción |
|---|---|
| A / D o flechas izquierda / derecha | Moverse |
| Shift izquierdo o derecho | Correr mientras se mueve |
| Espacio | Saltar; soltar la tecla reduce la altura del salto |
| Q | Invertir la gravedad dentro de una zona habilitada |
| J | Disparar en la dirección hacia la que mira Leo; mantener para repetir |
| E | Interactuar con módulos, palancas, relés, consola y portal |
| R | Volver a la última baliza |
| Esc | Pausar o continuar |
| Enter | Iniciar desde la pantalla de título |

Las teclas de movimiento y acciones del jugador se leen en `Assets/Umbral/Scripts/UmbralPlayer.cs`. El inicio y la pausa se controlan en `Assets/Umbral/Scripts/UmbralGame.cs`. Si cambias una tecla en el código, actualiza también el texto de controles dibujado por `UmbralGame` para que la interfaz coincida.

## Objetivo y recorrido

Recoge los tres módulos, habilita los mecanismos necesarios, rescata a Mara en el reactor y llega al portal del hangar.

| Sector | Resumen |
|---|---|
| 01 · Despertar | Aprende los controles y recupera el primer módulo. |
| 02 · Conductos | Cruza plataformas, recupera el segundo módulo y activa la palanca. |
| 03 · Laboratorio criogénico | Activa el relé para desbloquear la siguiente puerta. |
| 04 · Núcleo de seguridad | Destruye el generador de seguridad. |
| 05 · Reactor | Recupera el tercer módulo y rescata a Mara en la consola. |
| 06 · Hangar de evacuación | Despeja la ruta y usa el portal. |

El juego conserva el progreso durante la partida al reaparecer en una baliza. No guarda el progreso al cerrar o reiniciar el juego.

## Estructura del proyecto

| Ruta | Contenido |
|---|---|
| `Assets/Umbral/Scenes/` | Escena principal `Umbral.unity`. |
| `Assets/Umbral/Scripts/` | Código de juego y sus componentes. |
| `Assets/Umbral/Editor/` | Herramientas de creación de salas, importación de personajes, validación y compilación. |
| `Assets/Umbral/Prefabs/` | Prefabs reutilizables para personajes, enemigos y objetos destructibles. |
| `Assets/Umbral/Config/` | Assets editables de balance, mundo y definiciones de personajes. |
| `Assets/Umbral/Art/` | Atlas PNG originales de personajes y efectos. |
| `Assets/Umbral/Generated/` | Sprites recortados, tiles, texturas y recursos generados. |
| `Assets/Umbral/Tests/PlayMode/` | Pruebas de juego ejecutadas dentro de Unity. |
| `Assets/Umbral/Tests/Editor/` | Pruebas de herramientas y creación de salas. |
| `Packages/manifest.json` | Paquetes de Unity requeridos por el proyecto. |
| `ProjectSettings/` | Configuración del proyecto y versión de Unity. |
| `Builds/Windows/` | Compilación ejecutable para Windows. |
| `QA_Umbral/` | Resultados de pruebas y capturas utilizadas para revisión. |
| `Documentacion/` | Manual de uso y edición en Markdown y PDF. |

## Scripts principales

Los scripts están en `Assets/Umbral/Scripts/` y pertenecen al espacio de nombres `Umbral`.

| Script | Responsabilidad |
|---|---|
| `UmbralGame.cs` | Estado global de la partida, objetivos, interacción, audio, efectos y HUD. |
| `UmbralPlayer.cs` | Controles, movimiento, salto, gravedad, ataque, animación y reaparición de Leo. |
| `UmbralProp.cs` | Comportamiento de módulos, puertas, interruptores, balizas, zonas de gravedad y peligros. |
| `UmbralDrone.cs` | Patrulla del Centinela y contacto con el jugador. |
| `UmbralDamageable.cs` | Salud, daño y destrucción de enemigos y blancos destructibles. |
| `UmbralProjectile.cs` | Movimiento, colisiones y daño del proyectil. |
| `UmbralCamera.cs` | Seguimiento del jugador y límites de la cámara. |
| `UmbralCompanion.cs` | Seguimiento del aliado y movimiento de Mara después del rescate. |
| `UmbralEffect.cs` | Animación y duración de destellos y fragmentos. |
| `UmbralMovementSettings.cs` | Ajustes reutilizables de movimiento, salto, gravedad y combate. |
| `UmbralWorldConfig.cs` | Límites del mundo y definición de sectores. |
| `UmbralCharacterDefinition.cs` | Sprites, escala, collider y color de energía de un personaje. |
| `UmbralCapture.cs` | Secuencia automatizada de capturas para revisión de QA. |

## Editar y ampliar el juego

### Ajustes de juego

Los siguientes assets se pueden seleccionar y editar desde el Inspector:

- `Assets/Umbral/Config/Balance.asset`: velocidades, aceleración, salto, gravedad, enfriamiento, alcance y daño.
- `Assets/Umbral/Config/Mundo.asset`: límites de cámara, altura, límites verticales seguros y sectores.
- `Assets/Umbral/Config/Leo.asset` y `Mara.asset`: sprites, escala, tamaño del collider y efectos del personaje.

Haz los cambios fuera del modo **Play** para que se conserven.

### Añadir un enemigo

Para colocar un enemigo en la escena actual, arrastra un prefab desde `Assets/Umbral/Prefabs/` a la escena `Umbral.unity`. Para que un enemigo tenga la lógica estándar de los Centinelas, la raíz del prefab necesita:

- `SpriteRenderer` (puede estar en un hijo).
- Collider 2D con **Is Trigger** activado.
- `UmbralDamageable` para que los disparos puedan hacerle daño.
- `UmbralDrone` para patrullar y hacer que el jugador reaparezca al tocarlo.
- La capa `UmbralDrone` (capa 7), consultada por los proyectiles.

Configura `Max Health` en `UmbralDamageable` y los puntos `Left`, `Right` y la `Speed` en `UmbralDrone`. Si usas `Eye`, asígnale el `SpriteRenderer` del objeto visual.

El prefab personalizado `ENEMIGO.prefab`, si quieres conservarlo como parte de la escena, puede colocarse igual. Comprueba en él los componentes, la capa y las referencias anteriores. Para que el enemigo se vuelva a crear automáticamente al reconstruir la escena, añade una llamada al método `Drone(...)` en `Layout()` de `Assets/Umbral/Editor/UmbralBuilder.cs`; la firma de ese método permite indicar posición, extremos de patrulla, velocidad y salud.

### Herramientas del menú Umbral

En la barra de menús del Editor de Unity aparecen estas herramientas:

- **Umbral > Crear sala adicional...**: añade un sector conectado, suelo, techo, baliza y opcionalmente una zona de gravedad; actualiza el mundo y puede mover el portal.
- **Umbral > Importar personaje desde atlas 4x5...**: crea sprites y una definición a partir de un atlas PNG de veinte cuadros.
- **Umbral > Comprobar escena**: valida elementos esenciales de la escena.
- **Umbral > Generar o reconstruir prototipo**: genera nuevamente la escena y los recursos del prototipo. Puede reemplazar cambios manuales; úsalo con cuidado y guarda una copia antes.
- **Umbral > Compilar Windows**: crea una compilación actualizada para Windows.

Para ampliar una escena que ya editaste, usa **Crear sala adicional...** en vez de reconstruir el prototipo. Después de editar, guarda la escena con **Ctrl+S**.

### Colisiones y puertas

- El terreno sólido usa la capa `UmbralSolid` (capa 6).
- Los enemigos usan `UmbralDrone` (capa 7).
- Los proyectiles solo consultan esas dos capas; un objeto que no esté en una de ellas no será detectado por el disparo.
- Para recibir daño, el objetivo necesita `UmbralDamageable`.
- Una puerta y su interruptor deben usar el mismo nombre de circuito. Para una puerta que se abre al destruir un generador, configura `Circuit On Destroy` con ese nombre.

## Pruebas y compilación

### Ejecutar pruebas

En Unity, abre **Window > General > Test Runner** y ejecuta:

- **PlayMode** para las pruebas de controles, movimiento, progresión, combate y victoria.
- **EditMode** para las pruebas de herramientas y creación de salas.

Los archivos de pruebas se encuentran en `Assets/Umbral/Tests/`. Los resultados de ejecuciones anteriores, cuando existen, se guardan en `QA_Umbral/`; vuelve a ejecutar las pruebas después de hacer cambios para validar el estado actual.

### Crear una compilación de Windows

1. Guarda los cambios y la escena `Assets/Umbral/Scenes/Umbral.unity`.
2. En Unity, selecciona **Umbral > Compilar Windows**.
3. Espera el resultado del proceso y revisa `Builds/Windows/`.
4. Distribuye junta la aplicación `.exe` y todos los archivos y carpetas que Unity genera a su lado.

La compilación de Windows incluida está en `Builds/Windows/Proyecto Umbral.exe`.

### Capturas de QA

`UmbralCapture` se activa en una compilación cuando se ejecuta con el argumento `-umbral-qa` seguido de una carpeta de salida. Automatiza una secuencia de estados, guarda capturas y finaliza el proceso; está destinado a revisión visual, no a probar un recorrido de juego real.

## Limitaciones conocidas

- El progreso no se guarda entre sesiones.
- El proyecto no implementa multijugador ni selección de personaje durante la partida.
- Las salas adicionales se crean dentro de la escena `Umbral.unity`; no son transiciones entre escenas distintas.
- La herramienta de reconstrucción puede reemplazar modificaciones hechas directamente en la escena.
- El texto de controles mostrado en el HUD es texto dibujado por código; cambiar una tecla no actualiza automáticamente ese texto.

## Documentación adicional

- [Manual de usuario y edición (Markdown)](Documentacion/Manual_Umbral_Usuario_y_Edicion.md)
- [Manual de usuario y edición (PDF)](Documentacion/Manual_Umbral_Usuario_y_Edicion.pdf)
- [LEEME_UMBRAL.md](LEEME_UMBRAL.md): guía breve para jugar y editar.
- [INFORME_IMPLEMENTACION.md](INFORME_IMPLEMENTACION.md): cambios implementados y validaciones registradas.
