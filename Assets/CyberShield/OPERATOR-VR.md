# Operador VR y sonido

Escena: `Assets/CyberShield/Scenes/CyberShieldVR_ShieldOps.unity`.

## Avatar

Dos manos articuladas reutilizan los modelos del paquete XR Hands ya importado. Los guantes oscuros incorporan protecciones y luces. Hay 24 articulaciones de dedos animadas por las entradas de agarre y activación de los mandos. Tres luces en el guante izquierdo muestran el progreso de los retos.

Los hombros se estiman a partir del visor y los codos se calculan con un modelo de brazo de dos segmentos. No es seguimiento corporal completo. No se modifican las poses rastreadas ni se añaden colisionadores a los brazos. Al desactivarse un mando, su brazo se oculta; la presentación de seguimiento nativo de manos existente conserva su propio funcionamiento.

La malla decorativa de los mandos se oculta en esta escena. Las entradas, los rayos y los interactuables originales siguen activos. Los materiales de los modelos originales no se modifican: se usan materiales propios en las instancias de la escena.

## Sonido

Ocho archivos WAV originales generados localmente en `Assets/CyberShield/Audio/Operator`:

- Click: pulsación de botones.
- Grab / Drop: agarre y soltado de tarjetas, con fuentes espaciales que las acompañan.
- Scan: análisis de una copia completado.
- Contain: contención conseguida.
- Victory: cierre de misión.
- ServerFan: ventilación espacial desde ambos racks.
- Tension: base musical suave durante el incidente, con entrada y salida gradual.

El control de silencio existente alcanza a todas las fuentes nuevas. Se solicita un impulso háptico breve al agarrar con un interactor XR; su resultado físico requiere un mando compatible.

## Comprobaciones

- Compilación correcta.
- Animación de agarre comprobada mediante el lector de entrada en modo manual: valor de agarre 1 y cambio de rotación de dedos.
- Agarre y soltado comprobados con `StartManualInteraction(IXRSelectInteractable)` y `EndManualInteraction`, activando sus sonidos. Es una selección controlada mediante API, no una nueva prueba manual completa del apuntado con ratón.
- Alineación del extremo de antebrazo con la muñeca: error inferior a 0,001 m en la pose comprobada.
- Ambiente y música en reproducción; silencio de interfaz, tarjetas, música y servidores comprobado.
- Secuencia mediante eventos: correo, aislamiento, análisis y restauración completada con 100/100; nueve disparos de efectos registrados durante esa comprobación.
- Reapertura de escena: referencias de audio, materiales de guantes y luces de progreso conservados.
- La importación de audio excedió el tiempo de espera de Pipeline; se verificó después que los ocho clips estaban importados. La última prueba no registró nuevos errores del juego.

Pendiente: ergonomía de manos y codos en Quest 3, vibración real, balance auditivo en el visor, prueba manual integral y medición de rendimiento. Los brazos son una representación estilizada controlada por mandos.
