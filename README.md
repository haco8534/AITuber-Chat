  $key = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIMH5vcURnX5kK300U9zNlGCZsN7gWRqgUYqv/GVytM8i qwen-tts-deploy"

  # ユーザー用
  $userSsh = Join-Path $env:USERPROFILE ".ssh"
  $userAuth = Join-Path $userSsh "authorized_keys"

  New-Item -ItemType Directory -Force $userSsh | Out-Null
  Set-Content -Path $userAuth -Value $key -Encoding ascii

  icacls $userSsh /inheritance:r
  icacls $userSsh /grant:r "$($env:USERNAME):(F)" "SYSTEM:(F)"
  icacls $userAuth /inheritance:r
  icacls $userAuth /grant:r "$($env:USERNAME):(F)" "SYSTEM:(F)"

  # 管理者用
  $adminAuth = "C:\ProgramData\ssh\administrators_authorized_keys"
  Set-Content -Path $adminAuth -Value $key -Encoding ascii

  icacls $adminAuth /inheritance:r
  icacls $adminAuth /setowner "*S-1-5-32-544"
  icacls $adminAuth /grant:r "*S-1-5-32-544:(F)" "SYSTEM:(F)"

  Restart-Service sshd

  Get-Content $userAuth
  Get-Content $adminAuth
