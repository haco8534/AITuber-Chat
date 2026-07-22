  Remove-ItemProperty `
    -Path "HKLM:\SOFTWARE\OpenSSH" `
    -Name DefaultShell `
    -ErrorAction SilentlyContinue

  Remove-ItemProperty `
    -Path "HKLM:\SOFTWARE\OpenSSH" `
    -Name DefaultShellCommandOption `
    -ErrorAction SilentlyContinue

  Remove-ItemProperty `
    -Path "HKLM:\SOFTWARE\OpenSSH" `
    -Name DefaultShellEscapeArguments `
    -ErrorAction SilentlyContinue

  if (Test-Path "$env:USERPROFILE\.ssh\rc") {
      Move-Item `
        "$env:USERPROFILE\.ssh\rc" `
        "$env:USERPROFILE\.ssh\rc.disabled" `
        -Force
  }

  Restart-Service sshd




  ^^^^^^^^^^^^^^^^^

    Get-ItemProperty "HKLM:\SOFTWARE\OpenSSH" | Format-List DefaultShell*
  Get-ChildItem "$env:USERPROFILE\.ssh" -Force
