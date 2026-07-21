import asyncio
import os

import httpx
from fastapi import FastAPI, File, Request, UploadFile
from fastapi.responses import JSONResponse, Response, StreamingResponse
from fastapi.staticfiles import StaticFiles

STT_URL = os.environ.get("STT_URL", "http://localhost:8001")
LLM_URL = os.environ.get("LLM_URL", "http://192.168.11.66:11434")
TTS_URL = os.environ.get("TTS_URL", "http://localhost:8002")

app = FastAPI()
client = httpx.AsyncClient(timeout=120.0)


def error_response(e: Exception, status_code: int = 502):
    return JSONResponse({"error": str(e)}, status_code=status_code)


@app.get("/api/health")
async def health():
    async def check(url, path):
        try:
            r = await client.get(url + path, timeout=3.0)
            return r.status_code == 200
        except Exception:
            return False

    stt_ok, llm_ok, tts_ok = await asyncio.gather(
        check(STT_URL, "/health"),
        check(LLM_URL, "/api/tags"),
        check(TTS_URL, "/health"),
    )
    return {"stt": stt_ok, "llm": llm_ok, "tts": tts_ok}


@app.get("/api/models")
async def models():
    try:
        r = await client.get(f"{LLM_URL}/api/tags")
        r.raise_for_status()
        data = r.json()
        return {"models": [m["name"] for m in data.get("models", [])]}
    except Exception as e:
        return error_response(e)


@app.get("/api/speakers")
async def speakers():
    try:
        r = await client.get(f"{TTS_URL}/speakers")
        r.raise_for_status()
        return r.json()
    except Exception as e:
        return error_response(e)


@app.post("/api/stt")
async def stt(file: UploadFile = File(...)):
    try:
        data = await file.read()
        files = {"file": (file.filename or "audio.wav", data, file.content_type or "audio/wav")}
        r = await client.post(f"{STT_URL}/transcribe", files=files, timeout=120.0)
        r.raise_for_status()
        return r.json()
    except Exception as e:
        return error_response(e)


@app.post("/api/llm")
async def llm(req: Request):
    body = await req.json()
    stream = bool(body.pop("stream", False))

    if not stream:
        try:
            r = await client.post(f"{LLM_URL}/api/chat", json={**body, "stream": False}, timeout=120.0)
            r.raise_for_status()
            return r.json()
        except Exception as e:
            return error_response(e)

    async def event_stream():
        try:
            async with client.stream("POST", f"{LLM_URL}/api/chat", json={**body, "stream": True}, timeout=120.0) as resp:
                async for line in resp.aiter_lines():
                    if line:
                        yield line + "\n"
        except Exception as e:
            yield '{"error": "%s"}\n' % str(e).replace('"', "'")

    return StreamingResponse(event_stream(), media_type="application/x-ndjson")


@app.post("/api/tts")
async def tts(req: Request):
    try:
        body = await req.json()
        r = await client.post(f"{TTS_URL}/synthesize", json=body, timeout=120.0)
        r.raise_for_status()
        return Response(content=r.content, media_type="audio/wav")
    except Exception as e:
        return error_response(e)


app.mount("/", StaticFiles(directory=os.path.join(os.path.dirname(__file__), "static"), html=True), name="static")


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
