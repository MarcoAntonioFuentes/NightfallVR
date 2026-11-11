# CyberShield VR — prototipo

## Versión actual: Nexora / Incidente 1042

Abre `Assets/CyberShield/Scenes/CyberShieldVR_Nexora.unity` y pulsa Play. Nexora es una empresa ficticia; correos, enlaces, comunicaciones y servicios son simulados y no se conectan a sistemas reales.

La oficina incorpora mesas de madera, moqueta, techo acústico, ventanas con perfil urbano, puestos de soporte y contabilidad, sillas, plantas, impresora, archivador y señalética corporativa. La geometría sigue siendo estilizada. Los materiales tienen detalle procedural y 209 renderizadores decorativos se agruparon por material, manteniendo sus colisiones. El rendimiento todavía no está medido en Quest 3.

### Recorrido

1. En el expediente de correo, inspecciona al menos dos pistas distintas. Repetir una no añade evidencia. Luego reporta el correo sospechoso.
2. Desconecta Ethernet y Wi-Fi en PC-02. La disponibilidad simulada deja de descender cuando se contiene el incidente.
3. Toma la tarjeta y suéltala en la bandeja ámbar de análisis. Lee el diagnóstico. Después, mueve la copia apta al lector cian para restaurarla.
4. La restauración tarda cuatro segundos. El equipo permanece aislado; el informe recuerda la validación antes de reconectarlo.

Los mensajes simulados de Marta (TI) y Lucía (Contabilidad) cambian con el estado del incidente. La disponibilidad es un indicador narrativo y no añade penalizaciones por tiempo. Reiniciar borra evidencias y análisis para una nueva ronda.

Verificado en editor: bloqueo sin evidencias, prevención de pistas duplicadas, bloqueo sin análisis, detección física de tarjeta en escáner, transferencia al lector, finalización 100/100, ausencia de texto desbordado y compilación correcta. La creación de la escena superó el tiempo de espera de Pipeline, pero se comprobó que terminó y se guardó. Pendientes: prueba física de mandos, legibilidad y comodidad en visor, perfil de rendimiento Android y APK.

## Versión más reciente: interacción con objetos

Abre `Assets/CyberShield/Scenes/CyberShieldVR_HandsOn.unity`. Conserva las versiones anteriores y añade:

- Módulo PC-02 con controles seleccionables XR para Ethernet y Wi-Fi. Ambos deben desconectarse después de identificar el phishing.
- Tres tarjetas agarrables mediante XRGrabInteractable. Toma una con el botón de agarre del mando y suéltala sobre el lector cian. El lector solo procesa tarjetas sueltas en su zona y durante el reto de respaldo.
- Las tarjetas incorrectas descuentan puntos y regresan a su posición. Las que caen al suelo regresan automáticamente. El botón RECUPERAR TARJETAS devuelve las que no estén sujetas.
- SOLICITAR PISTA muestra una explicación contextual sin penalización.
- Reiniciar restaura conexiones y tarjetas. El respaldo limpio inicia la restauración animada.

Pruebas realizadas en el editor: eventos XR de ambos controles, bloqueo con una sola conexión desconectada, rechazo por proximidad de una tarjeta incorrecta, aceptación de la correcta, resultado de 90 puntos con un error y reinicio de conexiones. No aparecieron errores nuevos durante esas interacciones; Pipeline registró un tiempo de espera durante la creación, que sí terminó. Sigue pendiente probar agarre, alcance, teletransporte y comodidad con mandos físicos de Quest 3.

## Versión recomendada: Protocolo Blackout

Abre `Assets/CyberShield/Scenes/CyberShieldVR_Operations.unity` y pulsa Play. Pulsa **INICIAR OPERACIÓN** en el briefing. La escena base sigue disponible.

Esta versión añade sala cerrada de operaciones, racks de servidores, iluminación emisiva, señalética, líneas de red y nodos con estado, briefing narrativo, cronómetro sin límite, restauración animada de cuatro segundos, rango final, récord local y control de audio. La red de PC-02 permanece desconectada tras restaurar. El sonido final y el informe aparecen después de la restauración.

Los errores se descuentan del total acumulado, incluidos los cometidos cuando el marcador estaba en cero. El rango es S/Centinela sin errores, A/Defensor con al menos 70 puntos y B/Aprendiz en los demás casos. El récord se guarda en PlayerPrefs en el dispositivo local.

Verificado en Play: bloqueo inicial, inicio por evento del botón, penalización temprana, aislamiento, silencio, bloqueo durante restauración, informe de 90/100 con un error, barra al 100%, reinicio y cancelación de una restauración al reiniciar. La última comprobación de consola no mostró errores. Las pruebas invocan eventos de los botones; el apuntado con mandos y la comodidad todavía requieren validación en Quest 3. La geometría decorativa fija se agrupa por material; no se ha medido rendimiento en el visor.

Ambas escenas se agregaron al final de la lista de compilación. Para distribuir Blackout, prepara un perfil con Operations como escena inicial; no se ha generado un APK.

Abre `Assets/CyberShield/Scenes/CyberShieldVR.unity` y pulsa Play.

Sala informática con tres retos secuenciales en español: reportar phishing, aislar PC-02 y elegir un respaldo limpio. Las acciones simulan un incidente: no acceden a correos, redes ni archivos reales.

## Uso

- Apunta a los botones con el mando VR y pulsa el gatillo.
- Utiliza el teletransporte del rig de la plantilla para acercarte a las estaciones.
- Resuelve los retos en orden. Los siguientes permanecen bloqueados.
- Aciertos: 30, 30 y 40 puntos. Errores: -10 puntos, mínimo 0.
- El marcador muestra puntuación, errores y explicación. Reiniciar vuelve al primer reto.
- Sonidos sintetizados de acierto, error y finalización; no necesitan archivos externos.

## Contenido y conservación

Los scripts, materiales y escena están en `Assets/CyberShield`. Las escenas SampleScene y BasicScene y los prefabs originales se conservan. Se añadió la nueva escena al final de la lista de compilación, sin cambiar el orden anterior. Para un APK de CyberShield hay que seleccionar esta escena como inicio en un perfil de compilación específico.

Se reutiliza `Complete XR Origin Set Up Variant` de la plantilla y su sistema de entrada. El suelo usa la capa de interacción 31, la misma que los rayos de teletransporte del rig. Hay un EventSystem con XRUIInputModule y un XRInteractionManager. Los paneles son Canvas World Space con TrackedDeviceGraphicRaycaster y TextMeshPro.

## Verificación realizada

- Unity 6000.3.23f1: compilación de scripts sin errores.
- Pruebas de lógica: bloqueo de retos futuros, puntuación mínima, penalización, protección contra puntuación duplicada, 100 puntos y reinicio.
- Prueba en Play: eventos de botones recorren los tres retos, alcanzan 100/100, reproducen audio y reinician correctamente.
- Inspección visual mediante captura de cámara.

## Pendiente en dispositivo

No se ha generado ni instalado un APK ni probado físicamente en Quest 3. Hay que validar apuntado, teletransporte, comodidad, tamaño de texto, rendimiento y configuración OpenXR de Android en el visor. La configuración Android existente incluye IL2CPP, ARM64, API mínima 34 y perfiles Meta Quest/Touch Plus; también contiene funciones Android XR de la plantilla que deben revisarse al preparar el perfil de compilación.

En Play sin dispositivo de seguimiento ocular, el rig original emite una advertencia de GazeInputManager; no es un error de compilación ni impide las pruebas de lógica.
