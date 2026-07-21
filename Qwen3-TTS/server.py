import io
import os

import soundfile as sf
import torch
from fastapi import FastAPI
from fastapi.responses import Response
from pydantic import BaseModel
from qwen_tts import Qwen3TTSModel

MODEL_NAME = os.environ.get("QWEN_TTS_MODEL", "Qwen/Qwen3-TTS-12Hz-1.7B-CustomVoice")
DEFAULT_SPEAKER = os.environ.get("QWEN_TTS_SPEAKER", "Ono_Anna")
DEFAULT_LANGUAGE = os.environ.get("QWEN_TTS_LANGUAGE", "Japanese")

app = FastAPI()
model = Qwen3TTSModel.from_pretrained(
    MODEL_NAME,
    device_map="cuda:0",
    dtype=torch.bfloat16,
)


class SynthesizeRequest(BaseModel):
    text: str
    speaker: str = DEFAULT_SPEAKER
    language: str = DEFAULT_LANGUAGE
    instruct: str | None = None


@app.get("/health")
def health():
    return {"status": "ok", "model": MODEL_NAME, "speaker": DEFAULT_SPEAKER}


@app.get("/speakers")
def speakers():
    return {"speakers": model.get_supported_speakers(), "languages": model.get_supported_languages()}


@app.post("/synthesize")
def synthesize(req: SynthesizeRequest):
    wavs, sr = model.generate_custom_voice(
        text=req.text,
        language=req.language,
        speaker=req.speaker,
        instruct=req.instruct,
    )
    buf = io.BytesIO()
    sf.write(buf, wavs[0], sr, format="WAV")
    return Response(content=buf.getvalue(), media_type="audio/wav")


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8002)
