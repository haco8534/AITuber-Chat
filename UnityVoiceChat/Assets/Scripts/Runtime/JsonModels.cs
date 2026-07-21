using System;
using System.Collections.Generic;

[Serializable]
public class OllamaMessage
{
    public string role;
    public string content;
}

[Serializable]
public class OllamaOptions
{
    public float temperature;
    public float top_p;
    public int num_predict;
}

[Serializable]
public class OllamaChatRequest
{
    public string model;
    public List<OllamaMessage> messages;
    public bool stream;
    public OllamaOptions options;
}

[Serializable]
public class OllamaChatResponseMessage
{
    public string role;
    public string content;
}

[Serializable]
public class OllamaChatResponse
{
    public string model;
    public OllamaChatResponseMessage message;
    public bool done;
}

[Serializable]
public class OllamaModelInfo
{
    public string name;
}

[Serializable]
public class OllamaTagsResponse
{
    public List<OllamaModelInfo> models;
}

[Serializable]
public class SttResponse
{
    public string text;
    public string language;
    public float duration;
}

[Serializable]
public class TtsRequest
{
    public string text;
    public string speaker;
    public string language;
    public string instruct;
}

[Serializable]
public class TtsSpeakersResponse
{
    public List<string> speakers;
    public List<string> languages;
}
