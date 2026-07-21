  Write-Host "=== USER ==="
  whoami
  whoami /groups | Select-String "S-1-5-32-544"

  Write-Host "=== SSH CONFIG ==="
  Get-Content C:\ProgramData\ssh\sshd_config |
    Select-String "AuthorizedKeysFile|Match Group|PubkeyAuthentication"

  Write-Host "=== ADMIN KEY ==="
  Get-Content C:\ProgramData\ssh\administrators_authorized_keys
  icacls C:\ProgramData\ssh\administrators_authorized_keys
  ssh-keygen -lf C:\ProgramData\ssh\administrators_authorized_keys

  Write-Host "=== USER KEY ==="
  Get-Content "$env:USERPROFILE\.ssh\authorized_keys"
  icacls "$env:USERPROFILE\.ssh\authorized_keys"
  ssh-keygen -lf "$env:USERPROFILE\.ssh\authorized_keys"

  Write-Host "=== SSH LOG ==="
  Get-WinEvent -LogName OpenSSH/Operational -MaxEvents 10 |
    Format-List TimeCreated,Id,Message
