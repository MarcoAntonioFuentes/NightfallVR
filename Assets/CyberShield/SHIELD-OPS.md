# CyberShield VR · Shield Ops

Escena: `Assets/CyberShield/Scenes/CyberShieldVR_ShieldOps.unity`.
Versión independiente de Night Breach; las escenas anteriores se conservan.

## Experiencia

- Bandeja de correo corporativa con tres mensajes y selección naranja.
- Holograma de red: señalar un equipo y confirmar AISLAR. Los interruptores físicos siguen disponibles.
- Tarjetas físicas que se deben analizar antes de usarlas. Panel de selección y confirmación RESTAURAR, además del lector físico.
- Recuperación con progreso, escudo, lista de objetivos, puntuación y nueva partida.
- Mandos oscuros y etiquetas del tutorial ocultas durante la misión para despejar la vista.
- Perfil de iluminación propio con dos luces adicionales por objeto, sin sombras adicionales. Se aplica durante la escena y restaura el perfil anterior al salir.

## Validación en el editor — 26 octubre 2026

- Compilación de scripts correcta.
- Pruebas mediante eventos de botones e interactuables en Play: correo → aislamiento → análisis físico de tarjeta → restauración → victoria con 100/100.
- Selección incorrecta de equipo: penalización y permanencia en aislamiento.
- Restaurar sin analizar: bloqueado.
- Reinicio: puntuación a cero, etapa Correo y análisis borrado.
- Consola sin nuevos errores durante la última secuencia de comprobación.

Estas comprobaciones invocan los eventos dentro de Unity; no equivalen a una nueva prueba manual completa con ratón ni a una prueba en Meta Quest 3. Quedan pendientes el APK, la interacción y el rendimiento en el visor. El aspecto sigue siendo estilizado, no una reproducción fotorrealista del arte conceptual.

Capturas reales del editor en `Assets/Screenshots/ShieldOps-*`.

## Ampliación táctica

- La red plana se sustituye por una maqueta con ocho ordenadores 3D, servidor, retícula y tráfico animado. Cada ordenador admite selección XR y un botón asociado.
- Selección de equipo con información de proceso, conexión y archivos. PC-02 sigue siendo el equipo comprometido de la misión original.
- Un error de aislamiento muestra una pista específica durante cinco segundos y mantiene el reto activo.
- Cartel de misión cambia entre alerta, contención, recuperación y finalización.
- Dos puestos de trabajo adicionales reutilizan los modelos Kenney ya importados. Plantas, agendas, tazas, carteles y luminarias completan la oficina.
- Entrada desde el pasillo central; altura de cámara configurada a 1,65 m cuando se utiliza el desplazamiento del origen XR.

Comprobación mediante eventos en Play: consulta de PC-08, penalización por aislamiento incorrecto, selección mediante evento XR de PC-02, paso al respaldo y secuencia completa limpia con 100/100 y cartel de victoria correcto. El único error nuevo registrado durante la instalación fue un tiempo de espera de Pipeline; la instalación terminó y se comprobó guardada. No se observaron excepciones del juego en la secuencia comprobada. Estas pruebas no sustituyen una prueba manual del nuevo mapa con ratón ni una prueba en Quest 3.

## Investigación del correo

Abrir un mensaje ya no lo reporta automáticamente. El visor muestra el mensaje y tres controles para examinar remitente, destino real y contexto. El destino se presenta como texto sin ejecutar navegación externa. Se requieren al menos dos pistas del mensaje seleccionado y un motivo para reportarlo. El motivo correcto del mensaje malicioso es la solicitud de claves fuera del dominio corporativo. Una elección incorrecta penaliza y muestra una pista.

Se comprobaron los bloqueos por falta de pistas y motivo, la penalización por motivo incorrecto y el recorrido por los eventos de los botones hasta aislamiento, respaldo y victoria con 100/100. La sala inicia con luces azules y pasa a alerta roja al comenzar. Solo en el simulador de escritorio, la malla decorativa de los mandos se reduce al 58%; no se alteran el seguimiento, los rayos ni los volúmenes de agarre, y ese ajuste se excluye de las compilaciones para Quest.

Durante una recarga de scripts dentro de Play se registraron excepciones en el registro interno de `TrackedDeviceGraphicRaycaster` de XRI. Tras detener Play, completar la compilación y comenzar una sesión limpia, la secuencia completa no produjo errores nuevos. Las comprobaciones se realizan invocando eventos; aún falta la prueba manual integral del correo nuevo y la prueba en el visor.

## Presentación visual con menos texto

- Correo resumido, pistas con iconos vectoriales y marcas al inspeccionar. Se conserva el dominio real y la solicitud de contraseña para que la decisión tenga evidencia.
- Panel de equipo con tres indicadores animados: proceso, conexión y archivos. Las explicaciones extensas anteriores permanecen en el proyecto pero sus paneles redundantes se ocultan en Shield Ops.
- Guía superior DETECTA / AÍSLA / RECUPERA, puntuación y checks de progreso.
- Copias con icono de almacenamiento, fecha/origen y estados SIN ANALIZAR, LIMPIO o NO SEGURO. Barra visual de restauración.
- Ruta luminosa en el suelo que cambia de estación y se apaga al finalizar. Escudos y señal de alerta en los equipos de la maqueta.
- Comprobación mediante eventos: dos evidencias, reporte, aislamiento, bloqueo de copia sin analizar, análisis de copia A, restauración y victoria 100/100. Barra llena, tres checks activos y ruta apagada al finalizar.

Se detectó y corrigió la ausencia de CanvasRenderer en los iconos durante la primera revisión. El componente Glyph ahora lo exige. La validación sigue siendo en el editor mediante eventos, no una nueva prueba manual completa ni una medición de rendimiento en Quest 3.
