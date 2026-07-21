using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class TtsClient
{
    private readonly string baseUrl;

    public TtsClient(string baseUrl)
    {
        this.baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<TtsSpeakersResponse> GetSpeakersAsync()
    {
        using var req = UnityWebRequest.Get($"{baseUrl}/speakers");
        await req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"TTS話者一覧取得失敗: {req.error}");

        return JsonUtility.FromJson<TtsSpeakersResponse>(req.downloadHandler.text);
    }

    public async Task<byte[]> SynthesizeAsync(string text, string speaker, string language, string instruct)
    {
        var body = new TtsRequest { text = text, speaker = speaker, language = language, instruct = instruct ?? "" };
        string json = JsonUtility.ToJson(body);
        var bytes = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest($"{baseUrl}/synthesize", "POST");
        req.uploadHandler = new UploadHandlerRaw(bytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        await req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"TTS呼び出し失敗: {req.error}");

        return req.downloadHandler.data;
    }

    // WAVバイト列を一時ファイルに書き出してAudioClipとしてデコードする
    public static async Task<AudioClip> BytesToAudioClipAsync(byte[] wavBytes)
    {
        string path = Path.Combine(Application.temporaryCachePath, $"tts_{Guid.NewGuid():N}.wav");
        File.WriteAllBytes(path, wavBytes);

        using var req = UnityWebRequestMultimedia.GetAudioClip("file://" + path, AudioType.WAV);
        await req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"AudioClipデコード失敗: {req.error}");

        var clip = DownloadHandlerAudioClip.GetContent(req);

        try { File.Delete(path); } catch { /* ignore */ }

        return clip;
    }
}
