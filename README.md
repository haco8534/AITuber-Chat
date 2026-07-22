New-ItemProperty `
    -Path "HKLM:\SOFTWARE\OpenSSH" `
    -Name DefaultShell `
    -Value "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" `
    -PropertyType String `
    -Force

  New-ItemProperty `
    -Path "HKLM:\SOFTWARE\OpenSSH" `
    -Name DefaultShellCommandOption `
    -Value "-Command" `
    -PropertyType String `
    -Force

  Restart-Service sshd

  Get-ItemProperty "HKLM:\SOFTWARE\OpenSSH" |
    Select-Object DefaultShell,DefaultShellCommandOption
