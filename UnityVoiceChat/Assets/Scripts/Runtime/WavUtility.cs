using System.IO;
using UnityEngine;

public static class WavUtility
{
    // AudioClip(float PCM) を 16bit PCM WAV バイト列にエンコードする(マイク録音のアップロード用)
    public static byte[] FromAudioClip(AudioClip clip)
    {
        var samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        int dataSize = samples.Length * 2;

        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new[] { 'W', 'A', 'V', 'E' });
            writer.Write(new[] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((ushort)1); // PCM
            writer.Write((ushort)clip.channels);
            writer.Write(clip.frequency);
            writer.Write(clip.frequency * clip.channels * 2);
            writer.Write((ushort)(clip.channels * 2));
            writer.Write((ushort)16);
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);

            for (int i = 0; i < samples.Length; i++)
            {
                short v = (short)Mathf.Clamp(samples[i] * short.MaxValue, short.MinValue, short.MaxValue);
                writer.Write(v);
            }

            return stream.ToArray();
        }
    }
}
