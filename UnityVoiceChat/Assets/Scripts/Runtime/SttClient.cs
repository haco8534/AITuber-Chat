using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class SttClient
{
    private readonly string baseUrl;

    public SttClient(string baseUrl)
    {
        this.baseUrl = baseUrl.TrimEnd('/');
    }

    public async Task<SttResponse> TranscribeAsync(byte[] wavBytes)
    {
        var form = new List<IMultipartFormSection>
        {
            new MultipartFormFileSection("file", wavBytes, "recording.wav", "audio/wav"),
        };

        using var req = UnityWebRequest.Post($"{baseUrl}/transcribe", form);
        await req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"STT呼び出し失敗: {req.error} {req.downloadHandler.text}");

        return JsonUtility.FromJson<SttResponse>(req.downloadHandler.text);
    }
}
