# Analizador forense y comodidad

Aplicado a la escena Shield Ops.

- El panel de red incorpora ANALIZAR. Un barrido de 1,6 segundos revela proceso, conexión y archivos del equipo seleccionado. Antes del análisis, los indicadores aparecen como desconocidos y las pantallas de la maqueta permanecen azules.
- Cambiar de equipo o salir de la etapa cancela el barrido sin marcarlo como analizado.
- El botón de aislamiento del panel requiere analizar primero el equipo seleccionado. Los interruptores físicos conservan su ruta alternativa de juego existente.
- Los símbolos de la maqueta cambian al descubrir el estado del equipo. Se corrigió la alineación de sus ocho etiquetas con los modelos y se comprobó que se mantiene al reabrir la escena.
- Al finalizar aparece rango S con puntuación de 90 o más, A desde 70 y B por debajo, junto con el número de equipos examinados. La puntuación original de los tres retos se conserva.
- El botón SOLO GUANTES / VER BRAZOS permite ocultar las mallas de brazos. Los guantes, el seguimiento de mandos y la lectura de agarre siguen activos. La elección se mantiene durante esa sesión; no se escribe como preferencia global.
- El radar del monitor se oculta al mostrar la victoria para evitar texto y gráficos superpuestos.

## Validación mediante eventos de Unity

Compilación correcta. Se comprobaron bloqueo de aislamiento sin análisis, revelación de datos del equipo comprometido, diagnóstico de un equipo normal, cancelación al cambiar de objetivo y reinicio sin análisis previos. El selector de vista oculta ambos brazos y conserva ambos mandos activos; se comprobó su botón y el retorno a brazos completos.

La secuencia completa terminó con 100/100 y rango S. La última revisión no registró errores nuevos. Las comprobaciones se realizaron con eventos y APIs del editor; quedan pendientes la prueba manual integral, ergonomía y rendimiento en Quest 3.
