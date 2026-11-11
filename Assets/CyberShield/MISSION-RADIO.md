# Radio de TI y respuesta de misión

Escena: `Assets/CyberShield/Scenes/CyberShieldVR_ShieldOps.unity`.

Marta, la coordinadora ficticia de TI del escenario, emite seis comunicaciones breves en español: intrusión, fraude, contención, restauración, victoria y revisión de una decisión incorrecta. Los WAV se generaron localmente con la voz sintetizada Microsoft Helena Desktop del sistema y se guardan en `Assets/CyberShield/Audio/Radio`.

La radio tiene filtro de comunicación, prioridad propia y volumen separado. La música baja al 25% de su objetivo mientras se reproduce una comunicación; la alarma también se atenúa. El control general de silencio incluye la voz. RADIO: SÍ / NO permite desactivar solo la narración, conservando las confirmaciones visuales.

Los hitos muestran durante 2,2 segundos una tarjeta situada a un lado de la vista en el momento del evento: icono, tres palabras y puntos ganados o penalización. No modifica el seguimiento del visor ni bloquea los rayos de interacción. Los avisos de error por voz tienen un intervalo mínimo de diez segundos para evitar repeticiones continuas.

Durante la restauración, dos barridos luminosos recorren los racks. Al completarse, los LED de los servidores pasan a verde. Se solicitan impulsos hápticos breves en hitos si el runtime XR informa que hay un mando; su resultado físico queda pendiente del visor.

El panel de opciones se amplió a 600 × 390 unidades: sus tres botones tienen 110 unidades de altura y separación propia.

## Validación

- Compilación final correcta.
- Seis clips mono cargados, de 3,51 a 6,48 segundos. Se comprobó contenido no silencioso; picos entre 0,66 y 0,88 y RMS entre 0,065 y 0,087 antes del volumen del juego.
- Reproducción de voz, atenuación de música al 25%, silencio general, apagado individual de radio y recuperación de volumen comprobados.
- Secuencia con una decisión incorrecta: seis avisos de voz, seis confirmaciones y cierre con 90/100.
- Barridos de ambos racks observados durante restauración; desactivados al terminar. La velocidad se redujo únicamente para inspección y se restableció a 1.
- Partida limpia por eventos/APIs de Unity: 100/100, rango S y comunicación final Victoria. Reinicio sin voz pendiente y sin tarjeta de aviso residual.
- Última revisión sin errores nuevos del juego.

Pendientes: prueba manual integral, audición y balance en Quest 3, comodidad visual de los avisos, vibración física y rendimiento en el visor. Las confirmaciones cortas no son una transcripción completa de las comunicaciones de radio.
