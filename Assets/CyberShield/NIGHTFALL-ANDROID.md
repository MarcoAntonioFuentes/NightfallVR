# NIGHTFALL 3.0 · Contraataque

Una misión de ciberseguridad para Android, con juego en pantalla y opción de visor Google Cardboard. La escena nueva es `Assets/CyberShield/Scenes/DataCenter_Nightfall_Android.unity`.

## Cómo jugar

En celular, gira el teléfono para mirar y toca los objetos. Si el equipo no ofrece sensor de orientación, arrastra para mirar. En visor Cardboard, mira una acción durante 1,35 segundos o pulsa el botón del visor. Un aro indica el progreso de selección. Aparta la mirada para repetir una acción. Los desplazamientos son por teletransporte con transición a negro.

La app empieza en pantalla normal. **MODO VISOR** inicia Cardboard. En la primera entrada se abre el escáner QR para configurar las lentes de tu visor. La X nativa vuelve a pantalla normal. El engranaje de Cardboard permite volver a escanear el QR. **CENTRAR** reorienta la vista. **AUDIO** silencia/restaura música y voz. **SUBTÍTULOS** activa las transcripciones completas; por defecto se muestran instrucciones breves.

En el editor: ratón para seleccionar, botón derecho mantenido para mirar, E para activar, 1/2/3/4 para cambiar de estación, R para reiniciar. El modo visor se prueba en Android.

## Una presentación que muestra lo que el jugador hace

1. **Ataque:** observa durante 10 segundos la salida de paquetes rojos y el contador hasta 36 órdenes.
2. **Evidencia:** escanea el hueco del log, la impresora sin confirmaciones y el switch. No se puede sumar dos veces la misma evidencia.
3. **Contención:** desconecta el conector rojo. Se desbloquean los retos de defensa.
4. **MFA:** memoriza los módulos a medida que se iluminan. El orden se sortea al iniciar cada partida. Si fallas, los módulos vuelven a mostrar la secuencia.
5. **Horario:** detén el pulso dentro de la zona verde.
6. **SIEM:** conecta los nodos 1 → 2 → 3 para centralizar la auditoría.
7. **Contraataque:** intercepta cinco oleadas activando el módulo rojo. Los aciertos rápidos dan puntos extra y cada intercepción emite una onda.
8. **Resultado:** rango S/A/B, puntos, racha máxima y comparación 36 → 0. Una nueva misión reinicia el progreso.

Una frase útil al presentar: «No solo contamos el ataque: el jugador conserva evidencias, contiene el incidente y verifica cómo cambia el resultado al aplicar controles». Es una reconstrucción educativa; no se conecta a bancos ni ejecuta malware.

## Audio y gráficos

- Banda sonora original de sintetizadores, bajo, percusión y arpegios. Se genera localmente y baja su volumen durante la narración.
- Nueve frases breves con diálogo de héroe de anime y voz neuronal de Piper en español mexicano. [Ficha de la voz ALD](https://huggingface.co/rhasspy/piper-voices/blob/v1.0.0/es/es_MX/ald/medium/MODEL_CARD).
- El generador local de esta edición está en `SourceAssets/Generate-NightfallSurgeAudio.py`. Los diálogos se editan en la lista `Lines` del controlador de la misión.
- Efectos de selección, escaneo, confirmación, fallo, teletransporte e intrusión, con nuevos sonidos de intercepción y victoria.
- Paquetes de datos animados, núcleo holográfico, escudo tridimensional, motas ambientales, luces de alerta, pasillo con señalización y 108 LEDs.
- Materiales sencillos, geometría estática agrupable y límite de partículas para ajustar el coste en Android. El objetivo es 60 fps; debe medirse en el teléfono real.

## Fuentes y licencias

- Google Cardboard XR Plugin **1.36.0**, distribuido en `Packages/com.google.xr.cardboard`, bajo Apache 2.0. [Repositorio oficial](https://github.com/googlevr/cardboard-xr-plugin/releases/tag/v1.36.0), [configuración oficial](https://developers.google.com/cardboard/develop/unity/quickstart).
- Kenney **Sci-Fi Sounds**, CC0. Descarga desde [la publicación de Kenney en OpenGameArt](https://opengameart.org/content/sci-fi-sounds), licencia conservada en `DataCenterMobile/Kenney-License.txt`. Selección: computerNoise_000, computerNoise_002, forceField_000, impactMetal_000, doorOpen_000 y lowFrequency_explosion_000.
- Música y geometría adicional creadas para este proyecto. No requieren cuenta ni pago.

## Android y compilación

Configuración prevista: Android 8/API 26 mínimo, destino API 35, IL2CPP, ARM64, OpenGLES3, orientación Landscape Left y Activity. La configuración de Android usa Cardboard; la app inicia XR solo cuando se selecciona MODO VISOR. Un equipo sin giroscopio puede usar la experiencia en pantalla con arrastre, pero no puede ofrecer seguimiento de cabeza real en Cardboard.

Menús de Unity: **CyberShield > Nightfall Android > Crear experiencia móvil**, **Configurar Cardboard Android** y **Validar misión completa**. Para el APK se necesitan los módulos Android Build Support, Android SDK & NDK Tools y OpenJDK de Unity. La compilación incluye solo la escena Nightfall.

Para compilar desde un clon, instala las herramientas Android con Unity Hub y revisa sus rutas en **Preferences > External Tools**. En este equipo también se puede usar la instalación local de `SourceAssets/AndroidToolchain`, que el constructor detecta cuando está disponible. Cambia el destino a Android antes de usar **Generar APK**.

La validación automatizada genera `Logs/Nightfall-results.txt` y capturas en `DataCenterMobile/Preview`. Comprueba el recorrido completo, errores y reintentos, puntuación, recursos de audio, reinicio y componentes de la escena. La prueba automatizada de escritorio no sustituye la prueba de giroscopio, lentes, tacto, sonido y rendimiento en un teléfono físico.
