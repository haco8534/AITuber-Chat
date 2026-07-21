using System;
using UnityEngine;

public class MicRecorder
{
    public const int SampleRate = 16000;
    public const int MaxSeconds = 30;

    private string device;
    private AudioClip clip;
    private bool recording;

    public bool IsRecording => recording;

    public void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            throw new Exception("マイクデバイスが見つかりません");
        }
        device = Microphone.devices[0];
        clip = Microphone.Start(device, false, MaxSeconds, SampleRate);
        recording = true;
    }

    // 録音を停止し、実際に録音された長さにトリムしたAudioClipを返す
    public AudioClip StopRecording()
    {
        if (!recording) return null;
        int position = Microphone.GetPosition(device);
        Microphone.End(device);
        recording = false;

        if (position <= 0 || clip == null) return null;

        var data = new float[position * clip.channels];
        clip.GetData(data, 0);

        var trimmed = AudioClip.Create("recorded", position, clip.channels, clip.frequency, false);
        trimmed.SetData(data, 0);
        return trimmed;
    }
}
