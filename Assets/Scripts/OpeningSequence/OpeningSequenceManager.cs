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

        if (comicImage == null || subtitleText == null || audioSource == null)
        {
            Debug.LogError("OpeningSequenceManager: comicImage / subtitleText / audioSource 有未配置项。");
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

            if (slides[i].voice == null)
            {
                Debug.LogError($"Opening slide {i} is missing voice AudioClip.");
                continue;
            }

            audioSource.clip = slides[i].voice;
            audioSource.Play();

            yield return new WaitUntil(() => !audioSource.isPlaying);
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
}
