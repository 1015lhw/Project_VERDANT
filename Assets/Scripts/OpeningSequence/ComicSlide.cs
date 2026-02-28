using UnityEngine;

[System.Serializable]
public class ComicSlide
{
    public Sprite image;

    [TextArea(3,5)]
    public string subtitle;

    [Tooltip("可选：有语音就按语音长度切页")]
    public AudioClip voice;

    [Min(0.1f)]
    [Tooltip("无语音时使用的切页时间（秒）")]
    public float durationIfNoVoice = 2f;
}
