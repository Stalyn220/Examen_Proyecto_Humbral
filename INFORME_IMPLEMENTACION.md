# Proyecto Umbral - actualización 2

## Cambios implementados

- Seis sectores conectados y mundo ampliado de unas 112 a 222 unidades: Despertar, Conductos, Laboratorio criogénico, Núcleo de seguridad, Reactor y Hangar de evacuación.
- Marcha de 4,8 unidades/s y carrera de 7,2; aceleración, frenado, salto variable, tolerancias de salto, física a 60 Hz e interpolación del cuerpo. Cámara con seguimiento más rápido y límites configurables.
- Proyectiles visibles con colisión continua mediante barrido, alcance de 7 unidades, daño real y recuperación de 0,30 s. Ataque en suelo, techo y aire; mantener J repite el disparo.
- Ocho Centinelas, normales de un impacto y reforzados de dos. Destrucción con explosión y fragmentos. Cinco cajas y un generador destructibles.
- Relé criogénico y generador de seguridad que desbloquean puertas por circuitos. Tres módulos, consola, rescate de Mara, once balizas y portal final.
- Ajustes de movimiento, combate, mundo y personajes en ScriptableObjects editables desde el Inspector.
- Herramienta Umbral > Crear sala adicional para conectar una sala, ampliar cámara y trasladar opcionalmente el portal y el punto de Mara.
- Herramienta Umbral > Importar personaje desde atlas 4x5 para generar veinte sprites y una definición reutilizable.
- Menú Umbral > Compilar Windows.
- Manual de usuario y edición de doce páginas en PDF y versión editable Markdown, en Documentacion.

## Validación

Las ocho pruebas PlayMode y la prueba EditMode finalizaron aprobadas: **9 aprobadas y 0 fallidas**. La prueba principal recorre los seis sectores mediante teclado virtual, sin teletransportar al personaje, resuelve ambos circuitos, obtiene los tres módulos, rescata a Mara y termina en el portal sin reinicios. También se comprueban salto en techo, pausa, perfiles de personaje, ataque aéreo, bloqueo de proyectiles por paredes, enemigos de dos impactos y conservación del progreso al reaparecer.

La prueba de edición crea una sala temporal y verifica el suelo, la apertura de la pared anterior, los límites del mundo, la lista de sectores y la nueva posición del portal y su punto de canalización. La sala de prueba no se guarda en el proyecto.

El teclado de las pruebas se aísla de otros dispositivos para que escribir en otra aplicación no cambie el dispositivo virtual durante una ejecución en segundo plano.

Resultados: QA_Umbral/PlayModeResults.xml y QA_Umbral/AuthoringResults.xml. Las seis capturas Sector_*.png son renderizados de cámara en el Editor. El ejecutable Windows compiló con resultado Success. El manual se renderizó a imágenes y se revisaron sus doce páginas.

## Límites prácticos

La mejora de respuesta no implica una tasa de FPS medida o garantizada en todos los equipos. Los sprites conservan la calidad y los bordes del material original; el escenario y los robots mantienen arte sencillo del prototipo. La interfaz no se ha verificado mediante captura de una ventana visible. No hay guardado permanente, selección de personaje durante la partida ni cambio automático entre escenas separadas de Unity. Las nuevas salas se conectan dentro de Umbral.unity y los tres módulos siguen siendo el objetivo principal.

## Respaldo

La versión anterior se conserva en C:/Users/staly/OneDrive/Documentos/Videjuegos/Umbral_Respaldo_v1. No se modifica Assets/Scenes/SampleScene.unity.
