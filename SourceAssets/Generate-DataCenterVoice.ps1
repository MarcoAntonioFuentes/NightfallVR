$ErrorActionPreference = 'Stop'
$audioDir = Join-Path $PSScriptRoot '../Assets/CyberShield/DataCenter/Audio'
New-Item -ItemType Directory -Force -Path $audioDir | Out-Null
$voice = New-Object -ComObject SAPI.SpVoice
$tokens = $voice.GetVoices()
for ($i=0; $i -lt $tokens.Count; $i++) {
    if ($tokens.Item($i).GetDescription() -like '*Helena*') { $voice.Voice = $tokens.Item($i); break }
}
$voice.Rate = 0
$lines = @(
    'Bienvenido, operador. Soy tu asistente de seguridad. Estamos en el centro de datos. Revisa las evidencias antes de iniciar la sesión.',
    'Son las veinte treinta y seis. Una sesión con credenciales robadas ha accedido a SWIFT. Observa el reloj y busca los huecos del registro.',
    'Evidencia preservada. Faltan registros de auditoría. Un registro local puede ser manipulado. Centralizarlo permite conservar una copia independiente.',
    'Configuración actualizada. Prepara la autenticación multifactor, el bloqueo fuera de horario y el registro centralizado. Después inicia la sesión para comparar.',
    'Alerta. Acceso fuera de horario. Sesión bloqueada antes de emitir órdenes. Los sistemas de defensa responden correctamente.',
    'Simulación finalizada. Treinta y seis órdenes emitidas por novecientos cincuenta y un millones de dólares. Repite en modo defensa para impedir el acceso.',
    'Impresora apagada. No se imprimieron confirmaciones. La falta de papel no demuestra que no hubo actividad.',
    'Switch inspeccionado. La conectividad está activa. Conservar evidencia y bloquear accesos es más seguro que apagar equipos sin investigar.',
    'Simulación finalizada. Cero órdenes emitidas. Revisa si activaste también el registro centralizado para preservar la auditoría. Buen trabajo, operador.'
)
for ($i=0; $i -lt $lines.Count; $i++) {
    $stream = New-Object -ComObject SAPI.SpFileStream
    $stream.Open((Join-Path $audioDir "Asistente_$i.wav"), 3, $false)
    $voice.AudioOutputStream = $stream
    $null = $voice.Speak($lines[$i])
    $stream.Close()
}
Write-Output 'Nueve narraciones originales generadas con Microsoft Helena. La escena aplica tono grave y subtítulos.'
