PS C:\WINDOWS\system32>   cmd.exe /c hostname
>>   cmd.exe /d /c hostname
>>
>>   Get-ItemProperty `
>>     "HKCU:\Software\Microsoft\Command Processor" `
>>     -Name AutoRun `
>>     -ErrorAction SilentlyContinue
>>
>>   Get-ItemProperty `
>>     "HKLM:\Software\Microsoft\Command Processor" `
>>     -Name AutoRun `
>>     -ErrorAction SilentlyContinue
>>
>>   Get-Content "C:\ProgramData\ssh\sshd_config" |
>>     Select-String "ForceCommand|Match|AuthorizedKeysFile"
>>
c04
c04

AuthorizedKeysFile      .ssh/authorized_keys
#Match User anoncvs
#       ForceCommand cvs server
Match Group administrators
       AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys
