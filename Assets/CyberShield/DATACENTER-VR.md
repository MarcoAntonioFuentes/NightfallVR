# DataCenter VR · La noche del ataque

Esta guía describe la primera escena. La versión nueva para celular, con Cardboard, música y retos del jugador, está en **DataCenter_Nightfall_Android.unity**. Consulta **NIGHTFALL-ANDROID.md** para esa experiencia.

Abre `Assets/CyberShield/Scenes/DataCenter_VR.unity` y pulsa **Play**.
Si Unity todavía está importando los archivos, espera a que termine. También puedes crear la escena desde **CyberShield > Centro de datos > Crear escena DataCenter VR**.

## Recorrido para presentar en clase

1. En el portal, selecciona **ATAQUE 2016** e **INICIAR SESIÓN**.
2. Pulsa **2** para acercarte a la terminal y preserva el log con registros borrados.
3. Pulsa **4** e inspecciona la impresora apagada y el switch. Mira hacia cada lado para encontrarlos.
4. Observa el reloj acelerado: de 20:36 a 03:59 en 105 segundos. Las 36 órdenes se emiten durante los primeros 90 minutos simulados. **VER DESENLACE** permite saltar al final.
5. Pulsa **1**, elige **MODO DEFENSA**, luego **3** y activa MFA, bloqueo fuera de horario y SIEM.
6. Inicia la sesión desde el botón central o desde el portal. La alarma se dispara y la sesión queda bloqueada. Finaliza para comparar **36 frente a 0 órdenes**.

Cambiar de modo reinicia la ronda y las evidencias. Haber completado ambos modos se conserva durante la sesión de Play. SIEM por sí solo preserva la auditoría; no bloquea el acceso. MFA o la política de horario bloquean la sesión antes de emitir órdenes.

## Controles

- Ratón: apunta y haz clic sobre los botones físicos.
- Botón derecho mantenido + ratón: mirar alrededor.
- E: activar el botón al que apuntas.
- 1 / 2 / 3 / 4: teletransporte al portal, terminal, defensa o evidencias.
- R: reiniciar el modo actual. M: silenciar/restaurar audio.
- VR: apuntar con el mando derecho y pulsar gatillo. Sin mando: mantener la mirada 1,8 segundos sobre un botón y apartarla antes de repetir.
- Los cuatro destinos del suelo permiten teletransporte desde VR. No hay desplazamiento continuo.

## Asistente y ambiente

Nueve narraciones originales en español con archivos WAV locales, subtítulos, tono grave, ambiente de ventiladores y alarma sintetizada. La voz procede de Microsoft Helena y tiene el tono reducido en Unity; está inspirada en un asistente futurista tipo JARVIS, no es la voz del actor de las películas. No necesita internet, claves ni servicios de pago. Los WAV pueden sustituirse por una grabación propia conservando sus nombres.

El escenario incluye cuatro racks con 108 LEDs animados, terminal SWIFT, switch, impresora apagada, controles de defensa, reloj, informe comparativo y núcleo holográfico. Todo es una simulación local: no accede a sistemas bancarios ni ejecuta malware.

## Plataforma y límites de validación

La escena usa la configuración XR ya existente del proyecto y tiene control de escritorio propio. En esta escena se desactiva el simulador automático de XR del editor para evitar que compita con el ratón.

Las referencias proponen Android 8/API 26, IL2CPP ARM64, OpenGLES3 y Cardboard. Esa configuración y el inicio manual del visor están implementados en la versión nueva **Nightfall Android**. Para celular usa esa escena y su APK. La escena descrita aquí conserva el primer recorrido de escritorio; el proyecto mantiene el proveedor de escritorio. La prueba en un visor físico sigue pendiente.

## Verificación

**CyberShield > Centro de datos > Validar simulación** comprueba la escena en Play y genera `Logs/DataCenter-results.txt` y una captura en `Assets/CyberShield/DataCenter/Preview/DataCenter.png`. Guarda primero otras escenas abiertas con cambios pendientes. La prueba confirma las reglas de ataque/defensa y los recursos; la comodidad, los mandos y el rendimiento en un visor requieren una prueba física.

Los constructores no sobrescriben una escena DataCenter existente. Puedes personalizar materiales, geometría y texto desde el editor.
