using UnityEngine;

[System.Serializable]
public class ComicSlide
{
    public Sprite image;

    [TextArea(3,5)]
    public string subtitle;

    public AudioClip voice;
}
