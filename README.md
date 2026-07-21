Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0

  Start-Service sshd
  Set-Service sshd -StartupType Automatic

  if (-not (Get-NetFirewallRule -Name sshd -ErrorAction SilentlyContinue)) {
      New-NetFirewallRule -Name sshd `
        -DisplayName "OpenSSH Server" `
        -Enabled True `
        -Direction Inbound `
        -Protocol TCP `
        -Action Allow `
        -LocalPort 22 `
        -RemoteAddress 192.168.11.0/24
  }

  $key = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIMH5vcURnX5kK300U9zNlGCZsN7gWRqgUYqv/GVytM8i qwen-tts-deploy"

  New-Item -ItemType Directory -Force C:\ProgramData\ssh | Out-Null
  Set-Content -Path C:\ProgramData\ssh\administrators_authorized_keys `
    -Value $key `
    -Encoding ascii

  icacls C:\ProgramData\ssh\administrators_authorized_keys /inheritance:r
  icacls C:\ProgramData\ssh\administrators_authorized_keys /grant "*S-1-5-32-544:F"
  icacls C:\ProgramData\ssh\administrators_authorized_keys /grant "SYSTEM:F"

  Restart-Service sshd
  Get-Service sshd
