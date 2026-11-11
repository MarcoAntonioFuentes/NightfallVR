# Nexora: empresa bajo ataque

Aplicado a `Assets/CyberShield/Scenes/CyberShieldVR_ShieldOps.unity`.

La misión comienza con sistemas en línea. Al iniciarla aparecen una alerta corporativa, una conexión exterior con paquetes rojos y archivos de red bloqueados. Cuatro terminales de la oficina cambian a una pantalla de incidente. Un mural de 32 archivos representa el daño y otro muestra la respuesta y tres servicios corporativos.

La presión y los archivos afectados son indicadores visuales de la simulación; no añaden un fallo por tiempo ni cambian la puntuación. Las acciones se limitan a objetos, gráficos y audio de Unity. No se ejecutan conexiones, cifrado ni operaciones sobre archivos del equipo del jugador.

Al aislar PC-02 se corta la conexión exterior y se congela el daño. La restauración reduce progresivamente el número de archivos afectados, recupera los servicios y devuelve los terminales a EN LÍNEA. Un barrido de contención acompaña las transiciones. El reinicio borra la presión y los estados del ataque.

Sonidos originales adicionales: IntrusionImpact, DataTraffic y NetworkRecovery, guardados en `Assets/CyberShield/Audio/Operator`. Respetan el control de silencio existente. La iluminación de emergencia tiene más presencia y mantiene cambios suaves.

## Comprobación en el editor

- Compilación correcta y última sesión sin errores nuevos.
- Cuatro terminales afectados y archivos bloqueados durante la amenaza.
- Aislamiento: conexión desactivada, paquetes ocultos, daño congelado y terminales en contención.
- Audio del incidente y tráfico silenciados al usar el control existente.
- Restauración observada a velocidad reducida solo para la prueba: 16 archivos afectados pasaron a 6 con progreso aproximado de 57%; se observaron los ocho paquetes de restauración. Velocidad normal restablecida después.
- Secuencia completa mediante eventos de la misión: 100/100, cero archivos afectados, terminales EN LÍNEA y operación restablecida.
- Reinicio: presión cero, conexión desactivada, pantallas operativas y símbolo de estado correcto.

Las vistas laterales de inspección se tomaron con una cámara temporal eliminada después de revisar la escena. La captura Company-attack-final.png corresponde a la cámara del jugador. Pendientes: prueba manual integral y validación de comodidad, legibilidad, sonido y rendimiento en Quest 3.
