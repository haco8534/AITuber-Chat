 Write-Host "=== OpenSSH Registry ==="
  Get-ItemProperty "HKLM:\SOFTWARE\OpenSSH" |
    Format-List DefaultShell*

  Write-Host "=== SSH user files ==="
  Get-ChildItem "$env:USERPROFILE\.ssh" -Force |
    Select-Object Name,Length

  if (Test-Path "$env:USERPROFILE\.ssh\rc") {
      Write-Host "=== SSH RC ==="
      Get-Content "$env:USERPROFILE\.ssh\rc" -Raw
  }

  Write-Host "=== SSHD CONFIG ==="
  Get-Content "C:\ProgramData\ssh\sshd_config" |
    Select-String "ForceCommand|Match|AuthorizedKeysFile|Subsystem"

  Write-Host "=== SSHD SERVICE ==="
  Get-CimInstance Win32_Service -Filter "Name='sshd'" |
    Select-Object Name,State,PathName

  Write-Host "=== PowerShell Profile ==="
  $PROFILE
  Test-Path $PROFILE
  if (Test-Path $PROFILE) {
      Get-Content $PROFILE -Raw
  }
