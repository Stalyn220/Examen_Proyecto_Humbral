# Proyecto Umbral - versión 2

## Jugar

- En Unity 6000.6.4f1, abre `Assets/Umbral/Scenes/Umbral.unity`, pulsa Play y después Enter.
- Sin Unity, ejecuta `Builds/Windows/Proyecto Umbral.exe`. Conserva el ejecutable, su carpeta de datos y las bibliotecas juntos.

## Cambios principales

El recorrido pasa de tres a seis sectores: Despertar, Conductos, Laboratorio criogénico, Núcleo de seguridad, Reactor y Hangar de evacuación. El mundo se amplía aproximadamente al doble y añade relés, un generador de seguridad, ocho Centinelas y cajas destructibles.

La marcha aumenta a 4,8 unidades/s y la carrera a 7,2. Se ajustan aceleración, frenado, salto variable, tolerancia de salto e interpolación; la cámara usa un seguimiento más rápido. La física del juego trabaja a 60 pasos/s.

J dispara proyectiles reales, también en el aire. Mantener J repite los disparos con 0,30 s de recuperación. Los Centinelas normales resisten un impacto, los reforzados dos y el generador tres. Los enemigos se destruyen con una explosión y sus colisiones se desactivan. Las paredes bloquean los proyectiles.

## Controles

| Tecla | Acción |
| --- | --- |
| A / D o flechas | Caminar |
| Shift + movimiento | Correr |
| Espacio | Saltar; soltar antes acorta el salto |
| Q | Invertir gravedad en zonas violetas |
| J, pulsado o mantenido | Disparar, incluso en el aire |
| E | Recoger módulos y usar mecanismos, consola y portal |
| R | Volver a la última baliza |
| Esc | Pausar o continuar |

## Manual completo

Lee `Documentacion/Manual_Umbral_Usuario_y_Edicion.pdf` o su versión editable `.md`. El manual contiene controles, recorrido, combate, creación de salas, importación de personajes, configuración de capas y colisiones, parámetros, compilación y resolución de problemas.

## Ampliar desde Unity

- **Umbral > Crear sala adicional...** crea un sector conectado, actualiza el límite de cámara y puede mover el portal al nuevo final.
- **Umbral > Importar personaje desde atlas 4x5...** genera veinte sprites y una definición asignable a Leo o Mara.
- **Umbral > Compilar Windows** actualiza el ejecutable después de guardar los cambios.
- `Assets/Umbral/Config/Balance.asset` configura movimiento y disparo desde el Inspector.
- `Assets/Umbral/Config/Mundo.asset` configura límites y sectores.
- `Assets/Umbral/Config/Leo.asset` y `Mara.asset` definen los sprites, el tamaño y los efectos.

**Reconstruir prototipo** puede reemplazar los cambios manuales de la escena. Para ampliar tu escena personalizada utiliza la herramienta de salas y guarda con Ctrl+S.

## Progreso y alcance

La misión conserva tres módulos. Las balizas mantienen los objetivos recogidos y los enemigos destruidos durante la partida. No hay guardado entre sesiones, cambio de personaje durante el juego, multijugador ni transición automática entre escenas de Unity. Las salas nuevas pertenecen a la escena Umbral.

Los atlas originales se mantienen sin alterar. El laboratorio y los robots usan arte sencillo del prototipo. La animación utiliza cuatro cuadros por acción y no requiere un Animator Controller.

Se conserva el respaldo de la versión anterior en `C:/Users/staly/OneDrive/Documentos/Videjuegos/Umbral_Respaldo_v1`.
