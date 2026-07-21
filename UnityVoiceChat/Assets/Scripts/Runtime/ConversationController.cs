using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class ConversationController : MonoBehaviour
{
    [Header("Endpoints")]
    public string sttUrl = "http://localhost:8001";
    public string ollamaUrl = "http://localhost:11434";
    public string ttsUrl = "http://localhost:8002";

    [Header("UI - Status")]
    public Text statusText;
    public Text phaseText;

    [Header("UI - Params")]
    public Dropdown modelDropdown;
    public Dropdown speakerDropdown;
    public InputField languageInput;
    public InputField instructInput;
    public Slider temperatureSlider;
    public Text temperatureValueText;

    [Header("UI - Log")]
    public Text logText;
    public ScrollRect logScrollRect;

    [Header("UI - Input Row")]
    public InputField textInput;
    public Button sendButton;
    public Button micButton;
    public Button resetButton;
    public Text micButtonLabel;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("LLM Params")]
    public float topP = 0.9f;
    public int numPredict = 256;

    private OllamaClient ollamaClient;
    private SttClient sttClient;
    private TtsClient ttsClient;
    private MicRecorder micRecorder;

    private readonly List<OllamaMessage> messages = new List<OllamaMessage>();
    private bool busy;

    private void Awake()
    {
        ollamaClient = new OllamaClient(ollamaUrl);
        sttClient = new SttClient(sttUrl);
        ttsClient = new TtsClient(ttsUrl);
        micRecorder = new MicRecorder();
    }

    private async void Start()
    {
        sendButton.onClick.AddListener(OnSendClicked);
        micButton.onClick.AddListener(OnMicClicked);
        resetButton.onClick.AddListener(OnResetClicked);
        temperatureSlider.onValueChanged.AddListener(v => temperatureValueText.text = v.ToString("0.00"));
        temperatureValueText.text = temperatureSlider.value.ToString("0.00");

        SetPhase("IDLE");
        await InitAsync();
    }

    private async Task InitAsync()
    {
        try
        {
            var models = await ollamaClient.GetModelsAsync();
            modelDropdown.ClearOptions();
            modelDropdown.AddOptions(models);
            Log("info", $"LLMモデル一覧取得: {string.Join(", ", models)}");
            SetStatus(true, true, false);
        }
        catch (Exception e)
        {
            Log("error", $"Ollamaモデル一覧取得失敗: {e.Message}");
            SetStatus(false, false, false);
        }

        try
        {
            var spk = await ttsClient.GetSpeakersAsync();
            speakerDropdown.ClearOptions();
            speakerDropdown.AddOptions(spk.speakers);
            languageInput.text = "japanese";
            Log("info", $"TTS話者一覧取得: {string.Join(", ", spk.speakers)}");
            SetStatus(true, true, true);
        }
        catch (Exception e)
        {
            Log("error", $"TTS話者一覧取得失敗: {e.Message}");
        }
    }

    private void SetStatus(bool stt, bool llm, bool tts)
    {
        statusText.text = $"STT: {(stt ? "OK" : "-")}  LLM: {(llm ? "OK" : "-")}  TTS: {(tts ? "OK" : "-")}";
    }

    private void SetPhase(string phase)
    {
        phaseText.text = $"フェーズ: {phase}";
    }

    private void Log(string type, string msg)
    {
        string prefix = type switch
        {
            "start" => "▶",
            "ok" => "✓",
            "error" => "✗",
            _ => "・",
        };
        string line = $"[{DateTime.Now:HH:mm:ss.fff}] {prefix} {msg}";
        Debug.Log(line);
        logText.text += line + "\n";
        Canvas.ForceUpdateCanvases();
        if (logScrollRect != null) logScrollRect.verticalNormalizedPosition = 0f;
    }

    private void OnResetClicked()
    {
        messages.Clear();
        logText.text = "";
        SetPhase("IDLE");
        Log("info", "会話履歴をリセットしました");
    }

    private void OnMicClicked()
    {
        if (micRecorder.IsRecording)
        {
            var clip = micRecorder.StopRecording();
            micButtonLabel.text = "録音開始";
            if (clip == null)
            {
                Log("error", "録音データが空でした");
                return;
            }
            _ = TranscribeAndFillAsync(clip);
        }
        else
        {
            try
            {
                micRecorder.StartRecording();
                micButtonLabel.text = "停止";
                Log("start", "マイク録音開始");
            }
            catch (Exception e)
            {
                Log("error", $"マイクアクセス失敗: {e.Message}");
            }
        }
    }

    private async Task TranscribeAndFillAsync(AudioClip clip)
    {
        SetPhase("STT");
        Log("start", "STT: 録音データ送信");
        var sw = Stopwatch.StartNew();
        try
        {
            var wavBytes = WavUtility.FromAudioClip(clip);
            var resp = await sttClient.TranscribeAsync(wavBytes);
            Log("ok", $"STT: 完了 ({sw.ElapsedMilliseconds}ms) → \"{resp.text}\" (language={resp.language}, duration={resp.duration}s)");
            textInput.text = resp.text;
            SetPhase("IDLE");
        }
        catch (Exception e)
        {
            Log("error", $"STTエラー: {e.Message}");
            SetPhase("エラー");
        }
    }

    private void OnSendClicked()
    {
        string text = textInput.text;
        if (string.IsNullOrWhiteSpace(text) || busy) return;
        textInput.text = "";
        _ = SendMessageFlowAsync(text);
    }

    private async Task SendMessageFlowAsync(string userText)
    {
        busy = true;
        sendButton.interactable = false;
        Log("info", $"[USER] {userText}");
        messages.Add(new OllamaMessage { role = "user", content = userText });

        try
        {
            SetPhase("LLM");
            string model = modelDropdown.options.Count > 0 ? modelDropdown.options[modelDropdown.value].text : "";
            Log("start", $"LLM: {model} に送信 (temperature={temperatureSlider.value:0.00}, top_p={topP})");
            var sw = Stopwatch.StartNew();
            string reply = await ollamaClient.ChatAsync(model, messages, temperatureSlider.value, topP, numPredict);
            Log("ok", $"LLM: 完了 ({sw.ElapsedMilliseconds}ms) → \"{Truncate(reply, 60)}\"");
            Log("info", $"[ASSISTANT] {reply}");
            messages.Add(new OllamaMessage { role = "assistant", content = reply });

            if (!string.IsNullOrWhiteSpace(reply))
            {
                await SynthesizeAndPlaySentencesAsync(reply);
            }

            SetPhase("IDLE");
        }
        catch (Exception e)
        {
            Log("error", $"エラー: {e.Message}");
            SetPhase("エラー");
        }
        finally
        {
            busy = false;
            sendButton.interactable = true;
        }
    }

    private async Task SynthesizeAndPlaySentencesAsync(string fullText)
    {
        var sentences = SentenceSplitter.Split(fullText);
        if (sentences.Count == 0) return;

        SetPhase("TTS");
        Log("info", $"TTS: {sentences.Count}文に分割してパイプライン合成・再生します");

        string speaker = speakerDropdown.options.Count > 0 ? speakerDropdown.options[speakerDropdown.value].text : "";
        string language = languageInput.text;
        string instruct = instructInput.text;

        var sw = Stopwatch.StartNew();
        bool firstLogged = false;

        Task<byte[]> nextTask = SynthesizeSentenceAsync(sentences[0], 1, sentences.Count, speaker, language, instruct);

        for (int i = 0; i < sentences.Count; i++)
        {
            byte[] wavBytes = await nextTask;

            if (!firstLogged)
            {
                Log("info", $"TTS: 最初の音声まで {sw.ElapsedMilliseconds}ms(体感レイテンシ)");
                SetPhase("再生");
                firstLogged = true;
            }

            if (i + 1 < sentences.Count)
            {
                nextTask = SynthesizeSentenceAsync(sentences[i + 1], i + 2, sentences.Count, speaker, language, instruct);
            }

            var clip = await TtsClient.BytesToAudioClipAsync(wavBytes);
            Log("start", $"再生: {i + 1}/{sentences.Count} 文目 開始");
            audioSource.clip = clip;
            audioSource.Play();
            await Task.Delay(TimeSpan.FromSeconds(Math.Max(0.05f, clip.length)));
            Log("ok", $"再生: {i + 1}/{sentences.Count} 文目 終了");
        }

        SetPhase("完了");
    }

    private async Task<byte[]> SynthesizeSentenceAsync(string text, int index, int total, string speaker, string language, string instruct)
    {
        Log("start", $"TTS: [{index}/{total}] 音声合成開始 \"{Truncate(text, 30)}\"");
        var sw = Stopwatch.StartNew();
        var bytes = await ttsClient.SynthesizeAsync(text, speaker, language, instruct);
        Log("ok", $"TTS: [{index}/{total}] 完了 ({sw.ElapsedMilliseconds}ms, {bytes.Length}bytes)");
        return bytes;
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
        return s.Substring(0, max) + "...";
    }
}
