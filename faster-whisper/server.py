import os
import tempfile

# pip版cuBLAS/cuDNNのDLLをロードできるようにする(uvicorn起動前に必要)
_venv_site_packages = os.path.join(os.path.dirname(__file__), "venv", "Lib", "site-packages")
for _pkg in ("cublas", "cudnn"):
    _dll_dir = os.path.join(_venv_site_packages, "nvidia", _pkg, "bin")
    if os.path.isdir(_dll_dir):
        os.add_dll_directory(_dll_dir)
        os.environ["PATH"] = _dll_dir + os.pathsep + os.environ["PATH"]

from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
from faster_whisper import WhisperModel

MODEL_SIZE = os.environ.get("WHISPER_MODEL", "large-v3-turbo")
DEVICE = os.environ.get("WHISPER_DEVICE", "cuda")
COMPUTE_TYPE = os.environ.get("WHISPER_COMPUTE_TYPE", "float16")

app = FastAPI()
model = WhisperModel(MODEL_SIZE, device=DEVICE, compute_type=COMPUTE_TYPE)


@app.get("/health")
def health():
    return {"status": "ok", "model": MODEL_SIZE, "device": DEVICE}


@app.post("/transcribe")
async def transcribe(file: UploadFile = File(...)):
    data = await file.read()
    suffix = os.path.splitext(file.filename or "")[1] or ".wav"
    with tempfile.NamedTemporaryFile(suffix=suffix, delete=False) as tmp:
        tmp.write(data)
        tmp_path = tmp.name

    try:
        segments, info = model.transcribe(tmp_path, language="ja", vad_filter=True)
        text = "".join(seg.text for seg in segments)
        return JSONResponse({
            "text": text.strip(),
            "language": info.language,
            "duration": info.duration,
        })
    finally:
        os.remove(tmp_path)


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8001)
