# Night Breach — oficina bajo ataque

Escena: Assets/CyberShield/Scenes/CyberShieldVR_NightBreach.unity

Inspiración visual: las dos referencias aportadas por el usuario. Resultado estilizado de oficina nocturna; no es una reproducción fotorealista de esas imágenes y no incorpora personajes humanos.

Cambios: dos estaciones adicionales con modelos de escritorio, silla, monitor, teclado y ratón; dos racks con 18 servidores y 36 indicadores; cableado, teclas y marcos de monitor; ciudad nocturna procedural; materiales de oficina oscuros; tiras de alerta y dos luces sin sombras; señal sonora espacial de incidente. Las alertas rojas pasan gradualmente a cian al contener y a verde al completar. La alarma se silencia al aislar y respeta el control de audio.

Mobiliario: Furniture Kit de Kenney, CC0. Fuente: https://kenney.nl/assets/furniture-kit . Se importaron cinco FBX; licencia en Assets/CyberShield/ThirdParty/KenneyFurniture/License.txt. Archivo original y resto del paquete en SourceAssets/KenneyFurniture. No se importó la librería completa al juego. Materiales adaptados a URP.

Verificación: compilación completada sin errores; prueba de lógica en Play con eventos de pistas/correo, aislamiento y colocación de Rigidbody en análisis/lector: 100/100, misión terminada, alarma silenciada, pantallas de intrusión desactivadas y luces verdes. Consola sin errores nuevos desde cursor 34. La captura NightBreach-final.png usa una cámara temporal de revisión que desaparece al salir de Play.

Las versiones anteriores se conservan. Pendientes: prueba completa de interacción y audio percibido en Quest 3, medida de rendimiento y APK. Se reutiliza el simulador del editor; no hay verificación de hardware en esta entrega.
