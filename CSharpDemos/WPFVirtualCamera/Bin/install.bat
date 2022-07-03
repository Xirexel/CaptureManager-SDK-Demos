@echo off
if "%1"=="runas" (
  cd "%~dp0"
  echo Hello from admin mode

  C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe   "%~dp0VirtualCameraDShowFilter.dll" /nologo /codebase /tlb: "%~dp0VirtualCameraDShowFilter.tlb"  
  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe "%~dp0VirtualCameraDShowFilter.dll" /nologo /codebase /tlb: "%~dp0VirtualCameraDShowFilter.tlb"
  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe "%~dp0WPFVirtualCameraServer.exe"   /nologo /codebase /tlb: "%~dp0WPFVirtualCameraServer.tlb"
    
  exit
) else (
  powershell Start -File "cmd '/K \"%~f0\" runas'" -Verb RunAs
)