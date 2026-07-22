PS C:\WINDOWS\system32>   New-ItemProperty `
>>     -Path "HKLM:\SOFTWARE\OpenSSH" `
>>     -Name DefaultShell `
>>     -Value "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" `
>>     -PropertyType String `
>>     -Force
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
>>   Restart-Service sshd
>>
>>   Get-ItemProperty "HKLM:\SOFTWARE\OpenSSH" -Name DefaultShell
>>


DefaultShell : C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe
PSPath       : Microsoft.PowerShell.Core\Registry::HKEY_LOCAL_MACHINE\SOFTWARE\OpenSSH
PSParentPath : Microsoft.PowerShell.Core\Registry::HKEY_LOCAL_MACHINE\SOFTWARE
PSChildName  : OpenSSH
PSDrive      : HKLM
PSProvider   : Microsoft.PowerShell.Core\Registry

DefaultShell : C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe
PSPath       : Microsoft.PowerShell.Core\Registry::HKEY_LOCAL_MACHINE\SOFTWARE\OpenSSH
PSParentPath : Microsoft.PowerShell.Core\Registry::HKEY_LOCAL_MACHINE\SOFTWARE
PSChildName  : OpenSSH
PSDrive      : HKLM
PSProvider   : Microsoft.PowerShell.Core\Registry
