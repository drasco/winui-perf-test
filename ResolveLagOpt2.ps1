#No need to run as admin/elevated
$currentSession = (Get-Process -PID $pid).SessionID;
$ErrorActionPreference = 'SilentlyContinue';
Get-Process | ?{$_.SessionId -eq $currentSession}  |  %{$_.Name; $_.ProcessorAffinity = 1}
