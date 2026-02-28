using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class OpeningSequenceManager : MonoBehaviour
{
    public ComicSlide[] slides;

    public Image comicImage;
    public TMP_Text subtitleText;
    public AudioSource audioSource;
    public CanvasGroup canvasGroup;

    public float fadeDuration = 0.5f;

    void Start()
    {
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        yield return FadeIn();

        for (int i = 0; i < slides.Length; i++)
        {
            comicImage.sprite = slides[i].image;
            subtitleText.text = slides[i].subtitle;

            if (slides[i].voice != null)
            {
                audioSource.clip = slides[i].voice;
                audioSource.Play();
                yield return new WaitForSeconds(slides[i].voice.length);
            }
            else
            {
                yield return new WaitForSeconds(slides[i].durationIfNoVoice);
            }
        }

        yield return FadeOut();
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t / fadeDuration;
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1 - (t / fadeDuration);
            yield return null;
        }
    }
}