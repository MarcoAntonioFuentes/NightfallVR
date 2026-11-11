$ErrorActionPreference='Stop'
$audioDir=Join-Path $PSScriptRoot '../Assets/CyberShield/DataCenterMobile/Audio'
New-Item -ItemType Directory -Force -Path $audioDir | Out-Null
$voice=New-Object -ComObject SAPI.SpVoice
$tokens=$voice.GetVoices()
for($i=0;$i -lt $tokens.Count;$i++){if($tokens.Item($i).GetDescription() -like '*Helena*'){$voice.Voice=$tokens.Item($i);break}}
$voice.Rate=1
$lines=@(
'Operador, el centro de datos está bajo tu control. Vamos a reconstruir el ataque y a detenerlo.',
'Intrusión detectada. Observa los paquetes rojos. Treinta y seis órdenes salen sin que nadie lo impida.',
'Tu turno. Escanea el registro, revisa la impresora y localiza el switch. Usa los destinos para acercarte.',
'Ya tienes las tres evidencias. Desconecta el enlace rojo del switch para contener el incidente.',
'Activemos la autenticación multifactor. Observa y recuerda: tres, uno, dos. Ahora repite la secuencia.',
'Configura el bloqueo fuera de horario. Activa el botón cuando el pulso esté dentro de la zona verde.',
'Falta conservar la auditoría. Conecta los nodos uno, dos y tres para enviar los registros al SIEM.',
'Nueva intrusión. Acceso bloqueado. El escudo mantiene las órdenes en cero y conserva la evidencia.',
'Misión completada. Has transformado un incidente sin control en una defensa verificada. Excelente trabajo, operador.'
)
for($i=0;$i -lt $lines.Count;$i++){
$stream=New-Object -ComObject SAPI.SpFileStream
$stream.Open((Join-Path $audioDir "Mission_$i.wav"),3,$false)
$voice.AudioOutputStream=$stream
$null=$voice.Speak($lines[$i])
$stream.Close()
}
Write-Output 'Narración breve de Nightfall generada.'
