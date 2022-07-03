@echo off
if "%1"=="runas" (
  cd "%~dp0"
  echo Hello from admin mode

  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /unregister /nologo "%~dp0WPFVirtualCameraServer.exe"
  del "%~dp0WPFVirtualCameraServer.tlb"
  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /unregister /nologo "%~dp0VirtualCameraDShowFilter.dll"
  C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe /unregister /nologo "%~dp0VirtualCameraDShowFilter.dll"
  del "%~dp0VirtualCameraDShowFilter.tlb"
   
  exit
) else (
  powershell Start -File "cmd '/K \"%~f0\" runas'" -Verb RunAs
)