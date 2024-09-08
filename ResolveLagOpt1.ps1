$currentSession = (Get-Process -PID $pid).SessionID;
('msedge','chrome','tv_x64','explorer','TeamViewer_desktop','taskmgr','TeamViewer','ctfmon') | %{ 
  Get-Process -Name $_ | ?{$_.SessionId -eq $currentSession} |  %{$_.ProcessorAffinity = 1}
}
