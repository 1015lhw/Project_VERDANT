using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class OpeningSequenceManager : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("切换到本场景后自动播放开场（适合主菜单 StartGame 直接切场景的流程）。")]
    public bool autoBeginOnSceneLoad = true;

    [Header("Slides")]
    public ComicSlide[] slides;
    [Min(0f)]
    [Tooltip("默认漫画淡入时长（秒）。当单幕未启用自定义淡入淡出时使用。")]
    public float defaultFadeInDuration = 0.35f;
    [Min(0f)]
    [Tooltip("默认漫画淡出时长（秒）。当单幕未启用自定义淡入淡出时使用。")]
    public float defaultFadeOutDuration = 0.35f;

    [Header("UI")]
    public Image comicImage;
    public TMP_Text subtitleText;
    public AudioSource audioSource;
    [Tooltip("建议指向包含大漫画与子漫画的 UI 根节点。用于统一淡入淡出。")]
    public CanvasGroup comicVisualGroup;
    [Tooltip("启用后使用预放的子漫画对象（推荐），不再运行时创建子漫画对象。")]
    public bool usePreplacedSubComics = true;
    [Tooltip("预放的子漫画槽位（例如 subComic 1 / subComic 2）。索引由 slide.subComics[*].targetSlotIndex 指定。")]
    public Image[] preplacedSubComicSlots;
    [Tooltip("子漫画挂载点。留空时自动使用大漫画所在 RectTransform。")]
    public RectTransform subComicContainer;
    [Tooltip("开场 UI 根节点。结束时会自动隐藏，避免卡在最后一页。")]
    public GameObject openingCanvasRoot;

    [Header("Scene Control")]
    [Tooltip("开场期间会隐藏这个物体（通常是游戏世界根节点）。如果留空则不会自动隐藏世界。")]
    public GameObject gameWorldRoot;

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
    private readonly List<Image> activeSubComicImages = new List<Image>();
    private readonly List<Image> runtimeCreatedSubComicImages = new List<Image>();

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

        if (comicVisualGroup == null)
        {
            comicVisualGroup = comicImage.GetComponent<CanvasGroup>();
            if (comicVisualGroup == null)
            {
                comicVisualGroup = comicImage.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (subComicContainer == null)
        {
            subComicContainer = comicImage.rectTransform;
        }

        if (usePreplacedSubComics && (preplacedSubComicSlots == null || preplacedSubComicSlots.Length == 0))
        {
            Debug.LogWarning("OpeningSequenceManager: usePreplacedSubComics 已启用，但 preplacedSubComicSlots 为空。将不会显示子漫画。\n如需运行时创建，请关闭 usePreplacedSubComics。\n");
        }

        ResetPreplacedSubComicSlots();

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

        ClearSubComics();
    }

    IEnumerator PlaySequence()
    {
        bool skipped = false;

        for (int i = 0; i < slides.Length; i++)
        {
            ComicSlide currentSlide = slides[i];
            if (currentSlide == null)
            {
                continue;
            }

            comicImage.sprite = currentSlide.image;
            subtitleText.text = currentSlide.subtitle;
            comicVisualGroup.alpha = 0f;
            ClearSubComics();

            float fadeInDuration = GetFadeInDuration(currentSlide);
            float fadeOutDuration = GetFadeOutDuration(currentSlide);

            if (fadeInDuration > 0f)
            {
                yield return FadeComicGroup(0f, 1f, fadeInDuration, () => skipped = true);
            }
            else
            {
                comicVisualGroup.alpha = 1f;
            }

            if (skipped)
            {
                break;
            }

            Coroutine subComicCoroutine = StartCoroutine(PlaySubComics(currentSlide));

            if (currentSlide.voice != null)
            {
                audioSource.loop = false;
                audioSource.clip = currentSlide.voice;
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
                float fallbackDuration = Mathf.Max(0.1f, currentSlide.durationIfNoVoice);
                Debug.LogWarning($"Opening slide {i} has no voice. Use fallback duration: {fallbackDuration:0.00}s");
                yield return WaitWithSkip(fallbackDuration, () => skipped = true);
            }

            if (!skipped && currentSlide.holdAfterComplete > 0f)
            {
                yield return WaitWithSkip(currentSlide.holdAfterComplete, () => skipped = true);
            }

            if (subComicCoroutine != null)
            {
                StopCoroutine(subComicCoroutine);
            }

            if (skipped)
            {
                break;
            }

            if (fadeOutDuration > 0f)
            {
                yield return FadeComicGroup(1f, 0f, fadeOutDuration, () => skipped = true);
            }
            else
            {
                comicVisualGroup.alpha = 0f;
            }

            ClearSubComics();

            if (skipped)
            {
                break;
            }
        }

        EndOpening();
    }

    IEnumerator WaitWithSkip(float duration, Action onSkip)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (HandleSkipInput())
            {
                onSkip?.Invoke();
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeComicGroup(float from, float to, float duration, Action onSkip)
    {
        float timer = 0f;
        comicVisualGroup.alpha = from;

        while (timer < duration)
        {
            if (HandleSkipInput())
            {
                comicVisualGroup.alpha = to;
                onSkip?.Invoke();
                yield break;
            }

            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            comicVisualGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        comicVisualGroup.alpha = to;
    }

    IEnumerator PlaySubComics(ComicSlide slide)
    {
        if (slide == null || slide.subComics == null || slide.subComics.Length == 0)
        {
            yield break;
        }

        float slideStartTime = Time.time;

        for (int i = 0; i < slide.subComics.Length; i++)
        {
            SubComicCue cue = slide.subComics[i];
            if (cue == null || cue.image == null)
            {
                continue;
            }

            float elapsedFromSlideStart = Time.time - slideStartTime;
            float waitTime = Mathf.Max(0f, cue.appearDelay - elapsedFromSlideStart);
            if (waitTime > 0f)
            {
                yield return new WaitForSeconds(waitTime);
            }

            Image subComicImage = AcquireSubComicImage(cue, i);
            if (subComicImage == null)
            {
                continue;
            }

            activeSubComicImages.Add(subComicImage);

            float fadeDuration = Mathf.Max(0f, cue.fadeInDuration);
            if (fadeDuration <= 0f)
            {
                SetImageAlpha(subComicImage, 1f);
                continue;
            }

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / fadeDuration);
                SetImageAlpha(subComicImage, t);
                yield return null;
            }

            SetImageAlpha(subComicImage, 1f);
        }
    }

    Image AcquireSubComicImage(SubComicCue cue, int cueIndex)
    {
        if (usePreplacedSubComics)
        {
            if (preplacedSubComicSlots == null || preplacedSubComicSlots.Length == 0)
            {
                return null;
            }

            int slotIndex = Mathf.Clamp(cue.targetSlotIndex, 0, preplacedSubComicSlots.Length - 1);
            Image slotImage = preplacedSubComicSlots[slotIndex];
            if (slotImage == null)
            {
                Debug.LogWarning($"OpeningSequenceManager: preplacedSubComicSlots[{slotIndex}] 为空，无法显示子漫画 cue {cueIndex}。\n");
                return null;
            }

            slotImage.gameObject.SetActive(true);
            slotImage.sprite = cue.image;
            slotImage.preserveAspect = true;
            SetImageAlpha(slotImage, 0f);
            return slotImage;
        }

        Image runtimeImage = CreateSubComicImage(cue, cueIndex);
        runtimeCreatedSubComicImages.Add(runtimeImage);
        return runtimeImage;
    }

    Image CreateSubComicImage(SubComicCue cue, int index)
    {
        GameObject subComicObject = new GameObject($"SubComic_{index}", typeof(RectTransform), typeof(Image));
        subComicObject.transform.SetParent(subComicContainer, false);

        RectTransform rectTransform = subComicObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = cue.anchorMin;
        rectTransform.anchorMax = cue.anchorMax;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = cue.anchoredPosition;
        rectTransform.sizeDelta = cue.sizeDelta;

        Image image = subComicObject.GetComponent<Image>();
        image.sprite = cue.image;
        image.preserveAspect = true;
        SetImageAlpha(image, 0f);
        return image;
    }

    void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    void ClearSubComics()
    {
        if (usePreplacedSubComics)
        {
            ResetPreplacedSubComicSlots();
        }

        for (int i = 0; i < runtimeCreatedSubComicImages.Count; i++)
        {
            if (runtimeCreatedSubComicImages[i] != null)
            {
                Destroy(runtimeCreatedSubComicImages[i].gameObject);
            }
        }

        runtimeCreatedSubComicImages.Clear();
        activeSubComicImages.Clear();
    }

    void ResetPreplacedSubComicSlots()
    {
        if (preplacedSubComicSlots == null)
        {
            return;
        }

        for (int i = 0; i < preplacedSubComicSlots.Length; i++)
        {
            Image slotImage = preplacedSubComicSlots[i];
            if (slotImage == null)
            {
                continue;
            }

            SetImageAlpha(slotImage, 0f);
            slotImage.sprite = null;
            slotImage.gameObject.SetActive(false);
        }
    }

    float GetFadeInDuration(ComicSlide slide)
    {
        if (slide != null && slide.useCustomFadeDuration)
        {
            return Mathf.Max(0f, slide.fadeInDuration);
        }

        return Mathf.Max(0f, defaultFadeInDuration);
    }

    float GetFadeOutDuration(ComicSlide slide)
    {
        if (slide != null && slide.useCustomFadeDuration)
        {
            return Mathf.Max(0f, slide.fadeOutDuration);
        }

        return Mathf.Max(0f, defaultFadeOutDuration);
    }

    void EndOpening()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        ClearSubComics();

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
