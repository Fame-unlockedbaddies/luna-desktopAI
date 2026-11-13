$ErrorActionPreference = 'SilentlyContinue'
$ProgressPreference = 'SilentlyContinue'
$payload = @{ title = ''; artist = ''; album = ''; status = 'Stopped'; source = '' }
try {
  Add-Type -AssemblyName System.Runtime.WindowsRuntime
  $t = [System.WindowsRuntimeSystemExtensions]
  $asTask = $t.GetMethods() | Where-Object {
    $_.Name -eq 'AsTask' -and $_.GetParameters().Count -eq 1 -and
    $_.GetParameters()[0].ParameterType.IsGenericType -and
    $_.GetParameters()[0].ParameterType.Name.StartsWith('IAsyncOperation')
  } | Select-Object -First 1
  function Await([object]$op, [type]$type) {
    $m = $asTask.MakeGenericMethod($type)
    $task = $m.Invoke($null, @($op))
    return $task.Result
  }
  $managerType = [Windows.Media.Control.GlobalSystemMediaTransportControlsSessionManager,Windows.Media.Control,ContentType=WindowsRuntime]
  $propsType = [Windows.Media.Control.GlobalSystemMediaTransportControlsSessionMediaProperties,Windows.Media.Control,ContentType=WindowsRuntime]
  $manager = Await ($managerType::RequestAsync()) $managerType

  $sessions = $manager.GetSessions()
  $spotify = $null
  foreach ($s in $sessions) { if ($s.SourceAppUserModelId -match 'spotify') { $spotify = $s; break } }
  $session = if ($spotify) { $spotify } else { $manager.GetCurrentSession() }

  if ($session) {
    $payload.source = $session.SourceAppUserModelId
    $props = Await ($session.TryGetMediaPropertiesAsync()) $propsType
    if ($props) {
      $payload.title = $props.Title
      $payload.artist = $props.Artist
      $payload.album = $props.AlbumTitle
    }
    $info = $session.GetPlaybackInfo()
    $payload.status = "$($info.PlaybackStatus)"
  }
} catch {
  $payload.error = $_.Exception.Message
}
[Console]::Out.WriteLine(($payload | ConvertTo-Json -Compress))
