PS C:\WINDOWS\system32>   New-ItemProperty `
>>     -Path "HKLM:\SOFTWARE\OpenSSH" `
>>     -Name DefaultShell `
>>     -Value "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" `
>>     -PropertyType String `
>>     -Force
>>
>>   Restart-Service sshd
>>


DefaultShell : C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe
PSPath       : Microsoft.PowerShell.Core\Registry::HKEY_LOCAL_MACHINE\SOFTWARE\OpenSSH
PSParentPath : Microsoft.PowerShell.Core\Registry::HKEY_LOCAL_MACHINE\SOFTWARE
PSChildName  : OpenSSH
PSDrive      : HKLM
PSProvider   : Microsoft.PowerShell.Core\Registry
