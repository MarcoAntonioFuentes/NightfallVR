# Pantallas y cartuchos de respaldo

La renovación está aplicada a `Assets/CyberShield/Scenes/CyberShieldVR_ShieldOps.unity`.

- Los tres respaldos tienen carcasa biselada, placa superior, contactos dorados, ventilación lateral, tornillos, identificador y etiqueta con iconos.
- Se mantienen los componentes originales de física e interacción. Las nuevas piezas decorativas no añaden colisionadores. Su geometría está combinada por material; se conserva por separado la luz de estado.
- Estado visual sin análisis: origen de la copia. Después del análisis: LIMPIO o NO SEGURO. Un contorno resalta el dispositivo al agarrarlo, apuntarlo o seleccionarlo en el panel de restauración.
- Los botones reciben marcos de control, resaltado al apuntar y una variación suave de escala al pulsar.
- Las pantallas principales incorporan esquinas y líneas de control. El monitor central muestra un radar durante la amenaza y una confirmación circular de recuperación después de contenerla.
- Escáner con guías ámbar, lector con guías cian y superficies oscuras. El barrido de análisis y los paquetes de transferencia se activan durante sus fases correspondientes.
- Se conservan el audio, los brazos, las puntuaciones y los tres retos existentes.

## Validación en Unity

Compilación correcta. Secuencia mediante eventos completada con 100/100. Copia sin analizar bloqueada; copia B analizada marcada NO SEGURO y rechazada con penalización; copia A marcada LIMPIO y aceptada. El reinicio borra el análisis previo.

Selección manual del sistema XR comprobada con el nuevo cartucho: mantiene `isSelected`, activa el contorno y se suelta correctamente. El evento de apuntado cambia la escala del botón a aproximadamente 1,025. Estas comprobaciones usan APIs y eventos de Unity, no constituyen una nueva prueba manual completa con ratón o visor.

Se corrigió una referencia de instalación a los controladores de pantallas, que pertenecen a objetos independientes del controlador de misión. La instalación se realiza fuera de Play y mantiene inactivo el nuevo controlador hasta completar sus referencias.

Las capturas `Deluxe-respaldos-final.png` muestran una vista de inspección temporal dentro de Unity. Esa cámara se eliminó después de revisar el acabado; la cámara del jugador se conserva. La escena está guardada fuera de Play.

Pendiente: prueba manual integral, comodidad y legibilidad en Quest 3, rendimiento de la geometría/UI y ajuste visual dentro del visor.
