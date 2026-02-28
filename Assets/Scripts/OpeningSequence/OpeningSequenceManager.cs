using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class OpeningSequenceManager : MonoBehaviour
{
    [Header("Slides")]
    public ComicSlide[] slides;

    [Header("UI")]
    public Image comicImage;
    public TMP_Text subtitleText;
    public AudioSource audioSource;

    [Header("Scene Control")]
    [Tooltip("开场期间会隐藏这个物体（通常是游戏世界根节点）。如果留空则不会自动隐藏世界。")]
    public GameObject gameWorldRoot;

    [Header("No Voice Fallback")]
    [Min(0.1f)]
    [Tooltip("当某页未配置 voice 且该页 durationIfNoVoice <= 0 时，使用这个默认时长（秒）。")]
    public float defaultNoVoiceDuration = 2f;

    private Coroutine sequenceCoroutine;

    public void BeginOpening()
    {
        if (sequenceCoroutine != null)
        {
            return;
        }

        if (slides == null || slides.Length == 0)
        {
            Debug.LogError("OpeningSequenceManager: slides 未配置，无法播放开场。");
            return;
        }

        if (comicImage == null || subtitleText == null)
        {
            Debug.LogError("OpeningSequenceManager: comicImage / subtitleText 有未配置项。");
            return;
        }

        if (HasAnyVoiceClip() && audioSource == null)
        {
            Debug.LogError("OpeningSequenceManager: slides 中含有 voice，但 audioSource 未配置。");
            return;
        }

        OpeningLock.IsLocked = true;

        if (gameWorldRoot != null)
        {
            gameWorldRoot.SetActive(false);
        }

        sequenceCoroutine = StartCoroutine(PlaySequence());
    }

    void OnDisable()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
            OpeningLock.IsLocked = false;
        }
    }

    IEnumerator PlaySequence()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            comicImage.sprite = slides[i].image;
            subtitleText.text = slides[i].subtitle;

            if (slides[i].voice != null)
            {
                audioSource.clip = slides[i].voice;
                audioSource.Play();

                yield return new WaitUntil(() => !audioSource.isPlaying);
                continue;
            }

            float fallbackDuration = slides[i].durationIfNoVoice > 0f
                ? slides[i].durationIfNoVoice
                : defaultNoVoiceDuration;

            Debug.LogWarning($"Opening slide {i} has no voice. Use fallback duration: {fallbackDuration:0.00}s");
            yield return new WaitForSeconds(fallbackDuration);
        }

        EndOpening();
    }

    void EndOpening()
    {
        if (gameWorldRoot != null)
        {
            gameWorldRoot.SetActive(true);
        }

        OpeningLock.IsLocked = false;
        sequenceCoroutine = null;

        // TODO: trigger forced Ink dialogue here.
    }

    bool HasAnyVoiceClip()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            if (slides[i] != null && slides[i].voice != null)
            {
                return true;
            }
        }

        return false;
    }
}
