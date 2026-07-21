import wave
import struct
import os
import sys

# pip版cuBLAS/cuDNNのDLLをロードできるようにする
_venv_site_packages = os.path.join(os.path.dirname(__file__), "venv", "Lib", "site-packages")
for _pkg in ("cublas", "cudnn"):
    _dll_dir = os.path.join(_venv_site_packages, "nvidia", _pkg, "bin")
    if os.path.isdir(_dll_dir):
        os.add_dll_directory(_dll_dir)
        os.environ["PATH"] = _dll_dir + os.pathsep + os.environ["PATH"]

from faster_whisper import WhisperModel

# 1秒間の無音WAVを生成して動作確認用に使う
test_wav = os.path.join(os.path.dirname(__file__), "silence_test.wav")
if not os.path.exists(test_wav):
    with wave.open(test_wav, "w") as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(16000)
        f.writeframes(struct.pack("<" + "h" * 16000, *([0] * 16000)))

print("=== GPU(CUDA) で試行 ===")
try:
    model = WhisperModel("small", device="cuda", compute_type="float16")
    segments, info = model.transcribe(test_wav, language="ja")
    segments = list(segments)
    print("CUDA OK: language=%s, duration=%.2f, segments=%d" % (info.language, info.duration, len(segments)))
except Exception as e:
    print("CUDA NG:", repr(e))

    print("=== CPU にフォールバックして試行 ===")
    model = WhisperModel("small", device="cpu", compute_type="int8")
    segments, info = model.transcribe(test_wav, language="ja")
    segments = list(segments)
    print("CPU OK: language=%s, duration=%.2f, segments=%d" % (info.language, info.duration, len(segments)))
