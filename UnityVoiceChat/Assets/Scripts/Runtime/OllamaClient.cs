using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaClient
{
    private readonly string baseUrl;

    public OllamaClient(string baseUrl)
    {
        this.baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<List<string>> GetModelsAsync()
    {
        using var req = UnityWebRequest.Get($"{baseUrl}/api/tags");
        await req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"Ollamaモデル一覧取得失敗: {req.error}");

        var resp = JsonUtility.FromJson<OllamaTagsResponse>(req.downloadHandler.text);
        var names = new List<string>();
        if (resp?.models != null)
        {
            foreach (var m in resp.models) names.Add(m.name);
        }
        return names;
    }

    public async Task<string> ChatAsync(string model, List<OllamaMessage> messages, float temperature, float topP, int numPredict)
    {
        var body = new OllamaChatRequest
        {
            model = model,
            messages = messages,
            stream = false,
            options = new OllamaOptions { temperature = temperature, top_p = topP, num_predict = numPredict },
        };
        string json = JsonUtility.ToJson(body);
        var bytes = Encoding.UTF8.GetBytes(json);

        using var req = new UnityWebRequest($"{baseUrl}/api/chat", "POST");
        req.uploadHandler = new UploadHandlerRaw(bytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        await req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"LLM呼び出し失敗: {req.error} {req.downloadHandler.text}");

        var resp = JsonUtility.FromJson<OllamaChatResponse>(req.downloadHandler.text);
        return resp?.message?.content ?? "";
    }
}
