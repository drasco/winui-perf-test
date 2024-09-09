#Use a random core choice per session, for RDS systems with many users
#New procs may  need an affinity re-apllied, perhaps set a 30min re-run, but they may also inherit their affinity.
$threadCount = [int](Get-CimInstance -ClassName 'Win32_Processor' | Measure-Object -Sum -Property NumberOfLogicalProcessors).Sum
$coreChoice = (Get-Random -Minimum 0 -Maximum $threadCount);

$rndThread = [int]([math]::Pow(2, $coreChoice));//CPU0 is 0x1, CPU1 is 0x01 (2), CPU2 is 4 (0x001), CPU3 is 8 (0x0001)

$currentSession = (Get-Process -PID $pid).SessionID;
$ErrorActionPreference = 'SilentlyContinue';
Get-Process | ?{$_.SessionId -eq $currentSession}  |  %{$_.ProcessorAffinity = $rndThread}

#Code to give edge and chrome the above core choice, and also CPU0.
#Edge and chrome themselves run better with at least 2 cores. 
#But as soon as we do this the lag is re introduced.
#('msedge','chrome') | %{ 
#  Get-Process -Name $_ | ?{$_.SessionId -eq $currentSession} |  %{$_.ProcessorAffinity = ($rndThread+1)}
#}
