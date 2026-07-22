PS C:\WINDOWS\system32>   Remove-ItemProperty `
>>     -Path "HKLM:\SOFTWARE\OpenSSH" `
>>     -Name DefaultShell `
>>     -ErrorAction SilentlyContinue
>>
>>   Remove-ItemProperty `
>>     -Path "HKLM:\SOFTWARE\OpenSSH" `
>>     -Name DefaultShellCommandOption `
>>     -ErrorAction SilentlyContinue
>>
>>   Remove-ItemProperty `
>>     -Path "HKLM:\SOFTWARE\OpenSSH" `
>>     -Name DefaultShellEscapeArguments `
>>     -ErrorAction SilentlyContinue
>>
>>   if (Test-Path "$env:USERPROFILE\.ssh\rc") {
>>       Move-Item `
>>         "$env:USERPROFILE\.ssh\rc" `
>>         "$env:USERPROFILE\.ssh\rc.disabled" `
>>         -Force
>>   }
>>
>>   Restart-Service sshd
PS C:\WINDOWS\system32>   Get-ChildItem "$env:USERPROFILE\.ssh" -Force


    ディレクトリ: C:\Users\student\.ssh


Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a----        2026/07/21     17:36             98 authorized_keys
