using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class OpeningSequenceManager : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("切换到本场景后自动播放开场（适合主菜单 StartGame 直接切场景的流程）。")]
    public bool autoBeginOnSceneLoad = true;

    [Header("Slides")]
    public ComicSlide[] slides;

    [Header("UI")]
    public Image comicImage;
    public TMP_Text subtitleText;
    public AudioSource audioSource;
    [Tooltip("开场 UI 根节点。结束时会自动隐藏，避免卡在最后一页。")]
    public GameObject openingCanvasRoot;

    [Header("Scene Control")]
    [Tooltip("开场期间会隐藏这个物体（通常是游戏世界根节点）。如果留空则不会自动隐藏世界。")]
    public GameObject gameWorldRoot;

    [Header("No Voice Fallback")]
    [Min(0.1f)]
    [Tooltip("当某页未配置 voice 且该页 durationIfNoVoice <= 0 时，使用这个默认时长（秒）。")]
    public float defaultNoVoiceDuration = 2f;

    [Header("Skip")]
    public bool allowHoldEscToSkip = true;
    [Min(0.5f)]
    public float skipHoldDuration = 3f;
    [Tooltip("可选：长按 ESC 提示的根节点，开场时自动显示，结束时隐藏。")]
    public GameObject skipHintRoot;
    [Tooltip("可选：显示长按进度文本，例如：'Hold ESC to skip (1.2/3.0)'")]
    public TMP_Text skipHintText;
    [Tooltip("可选：填充图片（Image Type = Filled），用于长按进度条。")]
    public Image skipProgressFill;

    private Coroutine sequenceCoroutine;
    private float skipHoldTimer;

    void Start()
    {
        if (autoBeginOnSceneLoad)
        {
            BeginOpening();
        }
    }

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

        skipHoldTimer = 0f;
        UpdateSkipUI(0f);

        if (openingCanvasRoot != null)
        {
            openingCanvasRoot.SetActive(true);
        }

        if (skipHintRoot != null)
        {
            skipHintRoot.SetActive(allowHoldEscToSkip);
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
        bool skipped = false;

        for (int i = 0; i < slides.Length; i++)
        {
            comicImage.sprite = slides[i].image;
            subtitleText.text = slides[i].subtitle;

            if (slides[i].voice != null)
            {
                audioSource.loop = false;
                audioSource.clip = slides[i].voice;
                audioSource.Play();

                while (audioSource.isPlaying)
                {
                    if (HandleSkipInput())
                    {
                        skipped = true;
                        break;
                    }

                    yield return null;
                }
            }
            else
            {
                float fallbackDuration = slides[i].durationIfNoVoice > 0f
                    ? slides[i].durationIfNoVoice
                    : defaultNoVoiceDuration;

                Debug.LogWarning($"Opening slide {i} has no voice. Use fallback duration: {fallbackDuration:0.00}s");

                float timer = 0f;
                while (timer < fallbackDuration)
                {
                    if (HandleSkipInput())
                    {
                        skipped = true;
                        break;
                    }

                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            if (skipped)
            {
                break;
            }
        }

        EndOpening();
    }

    void EndOpening()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (openingCanvasRoot != null)
        {
            openingCanvasRoot.SetActive(false);
        }

        if (skipHintRoot != null)
        {
            skipHintRoot.SetActive(false);
        }

        if (gameWorldRoot != null)
        {
            gameWorldRoot.SetActive(true);
        }

        UpdateSkipUI(0f);
        OpeningLock.IsLocked = false;
        sequenceCoroutine = null;

        // TODO: trigger forced Ink dialogue here.
    }

    bool HandleSkipInput()
    {
        if (!allowHoldEscToSkip)
        {
            return false;
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            skipHoldTimer += Time.deltaTime;
        }
        else
        {
            skipHoldTimer = 0f;
        }

        float progress = Mathf.Clamp01(skipHoldTimer / skipHoldDuration);
        UpdateSkipUI(progress);

        return skipHoldTimer >= skipHoldDuration;
    }

    void UpdateSkipUI(float progress)
    {
        if (skipProgressFill != null)
        {
            skipProgressFill.fillAmount = progress;
        }

        if (skipHintText != null)
        {
            float holdSeconds = progress * skipHoldDuration;
            skipHintText.text = "Hold ESC to skip";
        }
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
