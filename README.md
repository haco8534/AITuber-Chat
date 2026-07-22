  cmd.exe /c hostname
  cmd.exe /d /c hostname

  Get-ItemProperty `
    "HKCU:\Software\Microsoft\Command Processor" `
    -Name AutoRun `
    -ErrorAction SilentlyContinue

  Get-ItemProperty `
    "HKLM:\Software\Microsoft\Command Processor" `
    -Name AutoRun `
    -ErrorAction SilentlyContinue

  Get-Content "C:\ProgramData\ssh\sshd_config" |
    Select-String "ForceCommand|Match|AuthorizedKeysFile"
