PS C:\WINDOWS\system32>   Write-Host "=== AutoRun values ==="
>>
>>   Get-ItemProperty `
>>     -Path "HKCU:\Software\Microsoft\Command Processor" `
>>     -Name AutoRun `
>>     -ErrorAction SilentlyContinue
>>
>>   Get-ItemProperty `
>>     -Path "HKLM:\Software\Microsoft\Command Processor" `
>>     -Name AutoRun `
>>     -ErrorAction SilentlyContinue
>>
>>   Write-Host "=== Remove broken AutoRun ==="
>>
>>   Remove-ItemProperty `
>>     -Path "HKCU:\Software\Microsoft\Command Processor" `
>>     -Name AutoRun `
>>     -ErrorAction SilentlyContinue
>>
>>   Remove-ItemProperty `
>>     -Path "HKLM:\Software\Microsoft\Command Processor" `
>>     -Name AutoRun `
>>     -ErrorAction SilentlyContinue
>>
>>   if (Test-Path "C:\ProgramData\ssh\sshrc") {
>>       Move-Item `
>>         "C:\ProgramData\ssh\sshrc" `
>>         "C:\ProgramData\ssh\sshrc.disabled" `
>>         -Force
>>   }
>>
>>   Restart-Service sshd
>>
>>   cmd.exe /c echo CMD_O
>>
=== AutoRun values ===
=== Remove broken AutoRun ===
CMD_O
