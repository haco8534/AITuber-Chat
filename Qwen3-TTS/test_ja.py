import torch
import soundfile as sf
from qwen_tts import Qwen3TTSModel

model = Qwen3TTSModel.from_pretrained(
    "Qwen/Qwen3-TTS-12Hz-1.7B-CustomVoice",
    device_map="cuda:0",
    dtype=torch.bfloat16,
)

wavs, sr = model.generate_custom_voice(
    text="こんにちは、これはQwen3-TTSのテストです。",
    language="Japanese",
    speaker="Ono_Anna",
)
sf.write("output_ja_test.wav", wavs[0], sr)
print("done", sr, len(wavs[0]))
