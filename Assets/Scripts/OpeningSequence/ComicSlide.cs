using UnityEngine;

[System.Serializable]
public class SubComicCue
{
    public Sprite image;

    [Min(0)]
    [Tooltip("要使用的子漫画槽位索引（预放对象模式下生效）。")]
    public int targetSlotIndex = 0;

    [Min(0f)]
    [Tooltip("子漫画在当前幕开始后多久出现（秒）")]
    public float appearDelay = 0f;

    [Min(0f)]
    [Tooltip("子漫画淡入时长（秒）")]
    public float fadeInDuration = 0.3f;

    [Tooltip("锚点最小值（仅运行时创建子漫画模式生效）。")]
    public Vector2 anchorMin = new Vector2(0f, 1f);

    [Tooltip("锚点最大值（仅运行时创建子漫画模式生效）。")]
    public Vector2 anchorMax = new Vector2(0f, 1f);

    [Tooltip("相对锚点的偏移（仅运行时创建子漫画模式生效）。")]
    public Vector2 anchoredPosition = new Vector2(20f, -20f);

    [Tooltip("子漫画尺寸（像素，仅运行时创建子漫画模式生效）。")]
    public Vector2 sizeDelta = new Vector2(320f, 180f);
}

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

    [Min(0f)]
    [Tooltip("语音结束（或无语音计时结束）后，额外停留多久再切下一幕（秒）。")]
    public float holdAfterComplete = 0f;

    [Tooltip("启用后，使用本幕自定义淡入淡出时长。")]
    public bool useCustomFadeDuration = false;

    [Min(0f)]
    [Tooltip("本幕漫画淡入时长（秒）。")]
    public float fadeInDuration = 0.35f;

    [Min(0f)]
    [Tooltip("本幕漫画淡出时长（秒）。")]
    public float fadeOutDuration = 0.35f;

    [Tooltip("可选：用于第5幕等场景，在大漫画上叠加多个子漫画。")]
    public SubComicCue[] subComics;
}
