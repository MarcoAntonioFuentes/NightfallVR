# Probar CyberShield VR en la computadora

Abre Assets/CyberShield/Scenes/CyberShieldVR_Nexora.unity y pulsa Play. Haz clic dentro de la pestaña Game para que reciba teclado y ratón.

Se importó y activó XR Interaction Simulator 3.5.1, solo para el editor. Simula el visor y los mandos del rig VR; no se activa automáticamente en la aplicación de Quest.

- WASD: desplazamiento. Si solo se mueve un mando, pulsa Tab para volver al modo FPS.
- Mantén el botón derecho y mueve el ratón: mirar alrededor.
- Apunta con el ratón a un botón, espera a que el rayo lo alcance y mantén T brevemente, luego suelta: gatillo/seleccionar. También admite clic izquierdo.
- G: agarre del mando seleccionado. Mantén para tomar una tarjeta y suelta para dejarla.
- Tab: alternar recorrido FPS / manipulación de mandos.
- Corchete derecho o izquierdo: seleccionar mando derecho o izquierdo.
- En modo de mando, WASD mueve ese mando; Q/E baja/sube. Esto permite colocar tarjetas en las bandejas.
- Y: mostrar/ocultar la ayuda de selección de entradas del simulador.

Recorrido: Iniciar operación > revisar dos evidencias > reportar correo > desconectar Ethernet y Wi-Fi > soltar tarjeta en bandeja ámbar de análisis > llevar copia limpia al lector cian y soltar.

Diagnóstico: antes solo había Keyboard y Mouse, sin visor activo ni simulador. La escena avanzaba pero faltaban entradas VR. Después se verificaron XRSimulatedHMD y dos XRSimulatedController, desplazamiento por W y activación del inicio mediante gatillo T, sin invocar directamente el botón de inicio.

Para probar más adelante con Quest Link en el editor, desactiva Use XR Interaction Simulator in scenes en Edit > Project Settings > XR Plug-in Management > XR Interaction Toolkit para evitar mezclar entradas simuladas y reales. La prueba física del visor y del APK sigue pendiente.
