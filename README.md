PS C:\WINDOWS\system32>  Write-Host "=== Normal CMD ==="
>>   cmd.exe /c hostname
>>
>>   Write-Host "=== CMD without AutoRun ==="
>>   cmd.exe /d /c hostname
>>
>>   Write-Host "=== User AutoRun ==="
>>   Get-ItemProperty `
>>     "HKCU:\Software\Microsoft\Command Processor" `
>>     -Name AutoRun `
>>     -ErrorAction SilentlyContinue
>>
>>   Write-Host "=== Machine AutoRun ==="
>>   Get-ItemProperty `
>>     "HKLM:\Software\Microsoft\Command Processor" `
>>     -Name AutoRun `
>>     -ErrorAction SilentlyContinue
>>
>>   Write-Host "=== ForceCommand ==="
>>   Get-Content "C:\ProgramData\ssh\sshd_config" |
>>     Select-String "ForceCommand|Match|AuthorizedKeysFile"
>>
=== Normal CMD ===
c04
=== CMD without AutoRun ===
c04
=== User AutoRun ===
=== Machine AutoRun ===
=== ForceCommand ===

AuthorizedKeysFile      .ssh/authorized_keys
#Match User anoncvs
#       ForceCommand cvs server
Match Group administrators
       AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys
