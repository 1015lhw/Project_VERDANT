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

    public GameObject gameWorldRoot;

    private Coroutine sequenceCoroutine;

    public void BeginOpening()
    {
        if (sequenceCoroutine != null)
        {
            return;
        }

        OpeningLock.IsLocked = true;

        if (gameWorldRoot != null)
        {
            gameWorldRoot.SetActive(false);
        }

        sequenceCoroutine = StartCoroutine(PlaySequence());
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
