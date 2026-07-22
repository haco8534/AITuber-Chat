  New-ItemProperty `
    -Path "HKLM:\SOFTWARE\OpenSSH" `
    -Name DefaultShell `
    -Value "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" `
    -PropertyType String `
    -Force

  Remove-ItemProperty `
    -Path "HKLM:\SOFTWARE\OpenSSH" `
    -Name DefaultShellCommandOption `
    -ErrorAction SilentlyContinue

  Remove-ItemProperty `
    -Path "HKLM:\SOFTWARE\OpenSSH" `
    -Name DefaultShellEscapeArguments `
    -ErrorAction SilentlyContinue

  Restart-Service sshd

  Get-ItemProperty "HKLM:\SOFTWARE\OpenSSH" -Name DefaultShell
