const chatLog = document.getElementById("chat-log");
const traceLog = document.getElementById("trace-log");
const phaseBadge = document.getElementById("phase-badge");
const textInput = document.getElementById("text-input");
const btnSend = document.getElementById("btn-send");
const btnReset = document.getElementById("btn-reset");
const btnUpload = document.getElementById("btn-upload");
const fileAudio = document.getElementById("file-audio");

let messages = []; // {role, content} 会話履歴(system除く)
let busy = false;

// ---------- ログ / フェーズ表示 ----------

function nowStr() {
  const d = new Date();
  return d.toTimeString().slice(0, 8) + "." + String(d.getMilliseconds()).padStart(3, "0");
}

function log(type, msg) {
  const line = document.createElement("div");
  line.className = "trace-line " + type;
  line.textContent = `[${nowStr()}] ${msg}`;
  traceLog.appendChild(line);
  traceLog.scrollTop = traceLog.scrollHeight;
}

function setPhase(phase, state, timeMs) {
  document.querySelectorAll(".phase-step").forEach((el) => {
    if (el.dataset.phase === phase) {
      el.classList.remove("active", "done", "error");
      if (state) el.classList.add(state);
      if (timeMs !== undefined) el.querySelector(".phase-time").textContent = `${timeMs}ms`;
    } else if (state === "active") {
      // 新しいフェーズが始まったら他はそのまま(done/errorは維持、activeは解除)
      if (el.classList.contains("active")) el.classList.remove("active");
    }
  });
}

function resetPhases() {
  document.querySelectorAll(".phase-step").forEach((el) => {
    el.classList.remove("active", "done", "error");
    el.querySelector(".phase-time").textContent = "";
  });
}

function setBadge(text, cls) {
  phaseBadge.textContent = text;
  phaseBadge.className = cls || "";
}

// ---------- チャットバブル ----------

function addBubble(role, text, { audioUrl, meta } = {}) {
  const b = document.createElement("div");
  b.className = "bubble " + role;
  const textNode = document.createElement("span");
  textNode.textContent = text;
  b.appendChild(textNode);
  if (meta) {
    const m = document.createElement("span");
    m.className = "meta";
    m.textContent = meta;
    b.appendChild(m);
  }
  if (audioUrl) {
    const audio = document.createElement("audio");
    audio.controls = true;
    audio.src = audioUrl;
    b.appendChild(audio);
  }
  chatLog.appendChild(b);
  chatLog.scrollTop = chatLog.scrollHeight;
  return { bubble: b, textNode };
}

// ---------- ステータス確認 ----------

async function checkHealth() {
  try {
    const r = await fetch("/api/health");
    const data = await r.json();
    setStatusPill("status-stt", "STT", data.stt);
    setStatusPill("status-llm", "LLM", data.llm);
    setStatusPill("status-tts", "TTS", data.tts);
    log("info", `health check: STT=${data.stt} LLM=${data.llm} TTS=${data.tts}`);
  } catch (e) {
    log("error", `health check失敗: ${e}`);
  }
}

function setStatusPill(id, label, ok) {
  const el = document.getElementById(id);
  el.textContent = `${label}: ${ok ? "接続OK" : "未接続"}`;
  el.className = "status-pill " + (ok ? "ok" : "ng");
}

async function loadModels() {
  try {
    const r = await fetch("/api/models");
    const data = await r.json();
    const sel = document.getElementById("llm-model");
    sel.innerHTML = "";
    (data.models || []).forEach((name) => {
      const opt = document.createElement("option");
      opt.value = name;
      opt.textContent = name;
      sel.appendChild(opt);
    });
    log("info", `LLMモデル一覧取得: ${(data.models || []).join(", ")}`);
  } catch (e) {
    log("error", `モデル一覧取得失敗: ${e}`);
  }
}

async function loadSpeakers() {
  try {
    const r = await fetch("/api/speakers");
    const data = await r.json();
    const spkSel = document.getElementById("tts-speaker");
    spkSel.innerHTML = "";
    (data.speakers || []).forEach((name) => {
      const opt = document.createElement("option");
      opt.value = name;
      opt.textContent = name;
      spkSel.appendChild(opt);
    });
    const langSel = document.getElementById("tts-language");
    langSel.innerHTML = "";
    (data.languages || []).forEach((name) => {
      const opt = document.createElement("option");
      opt.value = name;
      opt.textContent = name;
      langSel.appendChild(opt);
    });
    log("info", `TTS話者一覧取得: ${(data.speakers || []).join(", ")}`);
  } catch (e) {
    log("error", `話者一覧取得失敗: ${e}`);
  }
}

// ---------- LLM 呼び出し ----------

async function callLLM(userText) {
  const model = document.getElementById("llm-model").value;
  const system = document.getElementById("llm-system").value.trim();
  const temperature = parseFloat(document.getElementById("llm-temperature").value);
  const topP = parseFloat(document.getElementById("llm-top-p").value);
  const numPredict = parseInt(document.getElementById("llm-num-predict").value, 10);
  const think = document.getElementById("llm-think").checked;
  const stream = document.getElementById("llm-stream").checked;

  const msgs = [];
  if (system) msgs.push({ role: "system", content: system });
  msgs.push(...messages);

  const body = {
    model,
    messages: msgs,
    think,
    stream,
    options: { temperature, top_p: topP, num_predict: numPredict },
  };

  log("start", `LLM: ${model} に送信 (temperature=${temperature}, top_p=${topP}, think=${think}, stream=${stream})`);
  const t0 = performance.now();

  const { bubble, textNode } = addBubble("assistant", "");
  let fullText = "";
  let thinkingNode = null;

  const res = await fetch("/api/llm", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) });
  if (!res.ok) {
    const errBody = await res.json().catch(() => ({}));
    throw new Error(`LLM呼び出し失敗: ${res.status} ${errBody.error || ""}`);
  }

  if (!stream) {
    const data = await res.json();
    if (data.error) throw new Error(`LLM呼び出し失敗: ${data.error}`);
    fullText = data.message?.content || "";
    textNode.textContent = fullText;
    if (data.message?.thinking) {
      log("info", `LLM thinking: ${data.message.thinking}`);
    }
  } else {
    const reader = res.body.getReader();
    const decoder = new TextDecoder();
    let buf = "";
    while (true) {
      const { done, value } = await reader.read();
      if (done) break;
      buf += decoder.decode(value, { stream: true });
      const lines = buf.split("\n");
      buf = lines.pop();
      for (const line of lines) {
        if (!line.trim()) continue;
        let obj;
        try { obj = JSON.parse(line); } catch { continue; }
        if (obj.error) throw new Error(`LLM呼び出し失敗: ${obj.error}`);
        if (obj.message?.thinking) {
          log("info", `LLM thinking断片: ${obj.message.thinking}`);
        }
        if (obj.message?.content) {
          fullText += obj.message.content;
          textNode.textContent = fullText;
          chatLog.scrollTop = chatLog.scrollHeight;
        }
      }
    }
  }

  const dt = Math.round(performance.now() - t0);
  log("ok", `LLM: 完了 (${dt}ms) → "${fullText.slice(0, 60)}${fullText.length > 60 ? "..." : ""}"`);
  setPhase("llm", "done", dt);
  return { text: fullText, bubble };
}

// ---------- TTS 呼び出し ----------

function splitSentences(text) {
  const parts = text.match(/[^。！？\n]+[。！？]?/g) || [];
  return parts.map((s) => s.trim()).filter((s) => s.length > 0);
}

async function callTTS(text, label) {
  const speaker = document.getElementById("tts-speaker").value;
  const language = document.getElementById("tts-language").value;
  const instruct = document.getElementById("tts-instruct").value.trim();
  const tag = label ? `[${label}] ` : "";

  log("start", `TTS: ${tag}音声合成開始 "${text.slice(0, 30)}${text.length > 30 ? "..." : ""}" (speaker=${speaker}, language=${language}${instruct ? ", instruct=" + instruct : ""})`);
  const t0 = performance.now();

  const res = await fetch("/api/tts", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ text, speaker, language, instruct: instruct || null }),
  });
  if (!res.ok) {
    const errBody = await res.json().catch(() => ({}));
    throw new Error(`TTS呼び出し失敗: ${res.status} ${errBody.error || ""}`);
  }
  const blob = await res.blob();
  const dt = Math.round(performance.now() - t0);
  log("ok", `TTS: ${tag}完了 (${dt}ms, ${blob.size}bytes)`);
  return { url: URL.createObjectURL(blob), dt };
}

function playAudio(audioEl, url) {
  return new Promise((resolve, reject) => {
    audioEl.src = url;
    const onEnded = () => { cleanup(); resolve(); };
    const onError = () => { cleanup(); reject(new Error("音声再生エラー")); };
    function cleanup() {
      audioEl.removeEventListener("ended", onEnded);
      audioEl.removeEventListener("error", onError);
    }
    audioEl.addEventListener("ended", onEnded, { once: true });
    audioEl.addEventListener("error", onError, { once: true });
    audioEl.play().catch(onError);
  });
}

// 文単位でTTSを合成し、再生と並行して次の文を先読み合成する(体感レイテンシを削減)
async function synthesizeAndPlaySentences(fullText, bubble) {
  const sentences = splitSentences(fullText);
  if (sentences.length === 0) return;
  log("info", `TTS: ${sentences.length}文に分割してパイプライン合成・再生します`);

  const audioEl = document.createElement("audio");
  audioEl.controls = true;
  bubble.appendChild(audioEl);

  const ttsT0 = performance.now();
  let totalTtsMs = 0;
  let firstAudioAt = null;

  let nextChunk = callTTS(sentences[0], `1/${sentences.length}`);
  for (let i = 0; i < sentences.length; i++) {
    const { url, dt } = await nextChunk;
    totalTtsMs += dt;
    if (firstAudioAt === null) {
      firstAudioAt = Math.round(performance.now() - ttsT0);
      log("info", `TTS: 最初の音声まで ${firstAudioAt}ms(体感レイテンシ)`);
      setPhase("tts", "done", firstAudioAt);
      setPhase("play", "active");
    }
    if (i + 1 < sentences.length) {
      nextChunk = callTTS(sentences[i + 1], `${i + 2}/${sentences.length}`);
    }
    log("start", `再生: ${i + 1}/${sentences.length} 文目`);
    const playT0 = performance.now();
    await playAudio(audioEl, url);
    log("ok", `再生: ${i + 1}/${sentences.length} 文目 終了 (${Math.round(performance.now() - playT0)}ms)`);
  }
  setPhase("play", "done", Math.round(performance.now() - ttsT0) - firstAudioAt);
  log("ok", `TTS+再生: 全${sentences.length}文完了 (TTS合計${totalTtsMs}ms, 初音声まで${firstAudioAt}ms)`);
}

// ---------- メインパイプライン ----------

async function sendMessage(text) {
  if (busy || !text.trim()) return;
  busy = true;
  btnSend.disabled = true;
  resetPhases();
  setBadge("処理中...", "busy");

  addBubble("user", text);
  messages.push({ role: "user", content: text });

  try {
    setPhase("llm", "active");
    const { text: replyText, bubble } = await callLLM(text);
    messages.push({ role: "assistant", content: replyText });

    if (document.getElementById("tts-enabled").checked && replyText.trim()) {
      setPhase("tts", "active");
      await synthesizeAndPlaySentences(replyText, bubble);
    } else {
      log("info", "TTSはスキップされました(無効 or 空テキスト)");
    }

    setBadge("完了", "done");
  } catch (e) {
    log("error", `エラー: ${e.message || e}`);
    setBadge("エラー", "error");
    const activeStep = document.querySelector(".phase-step.active");
    if (activeStep) activeStep.classList.add("error");
  } finally {
    busy = false;
    btnSend.disabled = false;
  }
}

// ---------- STT (音声ファイルアップロード / マイク録音 共通) ----------

async function transcribeBlob(blob, filename) {
  resetPhases();
  setPhase("stt", "active");
  setBadge("STT実行中...", "busy");
  log("start", `STT: 音声送信 (${filename}, ${blob.size}bytes, type=${blob.type})`);
  const t0 = performance.now();
  try {
    const fd = new FormData();
    fd.append("file", blob, filename);
    const res = await fetch("/api/stt", { method: "POST", body: fd });
    const data = await res.json();
    if (!res.ok || data.error) throw new Error(data.error || `HTTP ${res.status}`);
    const dt = Math.round(performance.now() - t0);
    log("ok", `STT: 完了 (${dt}ms) → "${data.text}" (language=${data.language}, duration=${data.duration}s)`);
    setPhase("stt", "done", dt);
    setBadge("文字起こし完了", "done");
    textInput.value = data.text;
    textInput.focus();
  } catch (e) {
    log("error", `STTエラー: ${e.message || e}`);
    setPhase("stt", "error");
    setBadge("エラー", "error");
  }
}

// ---------- マイク録音 ----------

const btnMic = document.getElementById("btn-mic");
let mediaRecorder = null;
let recordedChunks = [];
let recordingStream = null;

async function startRecording() {
  try {
    recordingStream = await navigator.mediaDevices.getUserMedia({ audio: true });
  } catch (e) {
    log("error", `マイクアクセス失敗: ${e.message || e}`);
    return;
  }
  recordedChunks = [];
  mediaRecorder = new MediaRecorder(recordingStream);
  mediaRecorder.addEventListener("dataavailable", (e) => {
    if (e.data.size > 0) recordedChunks.push(e.data);
  });
  mediaRecorder.addEventListener("stop", () => {
    recordingStream.getTracks().forEach((t) => t.stop());
    btnMic.textContent = "🎤 録音開始";
    btnMic.classList.remove("recording");
    const mimeType = mediaRecorder.mimeType || "audio/webm";
    const blob = new Blob(recordedChunks, { type: mimeType });
    const ext = mimeType.includes("webm") ? "webm" : mimeType.includes("ogg") ? "ogg" : "wav";
    log("info", `録音終了 (${blob.size}bytes, ${mimeType})`);
    transcribeBlob(blob, `recording.${ext}`);
  });
  mediaRecorder.start();
  btnMic.textContent = "⏹ 録音停止";
  btnMic.classList.add("recording");
  log("start", "マイク録音開始");
}

function stopRecording() {
  if (mediaRecorder && mediaRecorder.state === "recording") {
    mediaRecorder.stop();
  }
}

btnMic.addEventListener("click", () => {
  if (mediaRecorder && mediaRecorder.state === "recording") {
    stopRecording();
  } else {
    startRecording();
  }
});

// ---------- イベント登録 ----------

document.getElementById("llm-temperature").addEventListener("input", (e) => {
  document.getElementById("out-temperature").textContent = e.target.value;
});
document.getElementById("llm-top-p").addEventListener("input", (e) => {
  document.getElementById("out-top-p").textContent = e.target.value;
});

btnSend.addEventListener("click", () => {
  const text = textInput.value;
  textInput.value = "";
  sendMessage(text);
});
textInput.addEventListener("keydown", (e) => {
  if (e.key === "Enter" && !e.isComposing) {
    const text = textInput.value;
    textInput.value = "";
    sendMessage(text);
  }
});

btnReset.addEventListener("click", () => {
  messages = [];
  chatLog.innerHTML = "";
  resetPhases();
  setBadge("IDLE", "");
  log("info", "会話履歴をリセットしました");
});

btnUpload.addEventListener("click", () => fileAudio.click());
fileAudio.addEventListener("change", () => {
  if (fileAudio.files.length > 0) {
    const f = fileAudio.files[0];
    transcribeBlob(f, f.name);
    fileAudio.value = "";
  }
});

document.getElementById("btn-refresh-status").addEventListener("click", checkHealth);
document.getElementById("btn-clear-log").addEventListener("click", () => { traceLog.innerHTML = ""; });

// ---------- 初期化 ----------

checkHealth();
loadModels();
loadSpeakers();
log("info", "デバッグコンソールを起動しました");
