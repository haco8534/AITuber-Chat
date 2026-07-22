  docker cp "$env:USERPROFILE\qwen-tts\server.py" qwen3-tts:/app/server.py
  docker restart qwen3-tts
