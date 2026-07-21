PS C:\WINDOWS\system32>   Write-Host "=== USER ==="
>>   whoami
>>   whoami /groups | Select-String "S-1-5-32-544"
>>
>>   Write-Host "=== SSH CONFIG ==="
>>   Get-Content C:\ProgramData\ssh\sshd_config |
>>     Select-String "AuthorizedKeysFile|Match Group|PubkeyAuthentication"
>>
>>   Write-Host "=== ADMIN KEY ==="
>>   Get-Content C:\ProgramData\ssh\administrators_authorized_keys
>>   icacls C:\ProgramData\ssh\administrators_authorized_keys
>>   ssh-keygen -lf C:\ProgramData\ssh\administrators_authorized_keys
>>
>>   Write-Host "=== USER KEY ==="
>>   Get-Content "$env:USERPROFILE\.ssh\authorized_keys"
>>   icacls "$env:USERPROFILE\.ssh\authorized_keys"
>>   ssh-keygen -lf "$env:USERPROFILE\.ssh\authorized_keys"
>>
>>   Write-Host "=== SSH LOG ==="
>>   Get-WinEvent -LogName OpenSSH/Operational -MaxEvents 10 |
>>     Format-List TimeCreated,Id,Message
>>
=== USER ===
c04\student

BUILTIN\Administrators                                               エイリアス           S-1-5-32-544                                  固定グループ, 既定で有効, 有効なグループ, グループ所有者
=== SSH CONFIG ===
#PubkeyAuthentication yes
AuthorizedKeysFile      .ssh/authorized_keys
Match Group administrators
       AuthorizedKeysFile __PROGRAMDATA__/ssh/administrators_authorized_keys
=== ADMIN KEY ===
ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIMH5vcURnX5kK300U9zNlGCZsN7gWRqgUYqv/GVytM8i qwen-tts-deploy
C:\ProgramData\ssh\administrators_authorized_keys NT AUTHORITY\SYSTEM:(F)
                                                  BUILTIN\Administrators:(F)

1 個のファイルが正常に処理されました。0 個のファイルを処理できませんでした
256 SHA256:OtkP5lTAZ62HRv+km9NrRrn4p+vl/irD+k3SatwTGcQ qwen-tts-deploy (ED25519)
=== USER KEY ===
ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIMH5vcURnX5kK300U9zNlGCZsN7gWRqgUYqv/GVytM8i qwen-tts-deploy
C:\Users\student\.ssh\authorized_keys NT AUTHORITY\SYSTEM:(F)
                                      c04\student:(F)

1 個のファイルが正常に処理されました。0 個のファイルを処理できませんでした
256 SHA256:OtkP5lTAZ62HRv+km9NrRrn4p+vl/irD+k3SatwTGcQ qwen-tts-deploy (ED25519)
=== SSH LOG ===




TimeCreated : 2026/07/21 17:36:43
Id          : 4
Message     : sshd: Connection reset by authenticating user ikebe 192.168.11.19 port 55492 [preauth]

TimeCreated : 2026/07/21 17:36:38
Id          : 4
Message     : sshd: Connection reset by authenticating user ikebe 192.168.11.19 port 55490 [preauth]

TimeCreated : 2026/07/21 17:36:16
Id          : 4
Message     : sshd: Server listening on 0.0.0.0 port 22.

TimeCreated : 2026/07/21 17:36:16
Id          : 4
Message     : sshd: Server listening on :: port 22.

TimeCreated : 2026/07/21 17:35:50
Id          : 4
Message     : sshd: Connection reset by authenticating user ikebe 192.168.11.19 port 60485 [preauth]

TimeCreated : 2026/07/21 17:34:01
Id          : 4
Message     : sshd: Connection reset by authenticating user c04\\\\ikebe 192.168.11.19 port 64666 [preauth]

TimeCreated : 2026/07/21 17:33:45
Id          : 4
Message     : sshd: Connection reset by authenticating user ikebe 192.168.11.19 port 64663 [preauth]

TimeCreated : 2026/07/21 17:32:17
Id          : 4
Message     : sshd: Server listening on 0.0.0.0 port 22.

TimeCreated : 2026/07/21 17:32:17
Id          : 4
Message     : sshd: Server listening on :: port 22.

TimeCreated : 2026/07/21 17:32:15
Id          : 4
Message     : sshd: Server listening on 0.0.0.0 port 22.
