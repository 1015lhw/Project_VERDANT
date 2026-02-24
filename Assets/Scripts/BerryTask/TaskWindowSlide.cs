using UnityEngine;
using System;
using System.Collections;

public class TaskWindowSlide : MonoBehaviour
{
    public RectTransform taskWindow;
    public float slideDuration = 0.3f;

    private Vector2 centerPos = Vector2.zero;
    private Vector2 hiddenPos;
    private Coroutine running;

    void Awake()
    {
        if (taskWindow == null)
            taskWindow = transform.Find("TaskWindow")?.GetComponent<RectTransform>();
        
        if (taskWindow != null)
            centerPos = taskWindow.anchoredPosition;

        hiddenPos = new Vector2(0, -1500f);
    }

    void OnEnable()
    {
        if (taskWindow == null) return;

        taskWindow.gameObject.SetActive(true);
        taskWindow.anchoredPosition = hiddenPos;
    }

    public void PlaySlideIn()
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Slide(hiddenPos, centerPos, null));
    }

    public void PlaySlideOut(Action onComplete)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Slide(taskWindow.anchoredPosition, hiddenPos, onComplete));
    }

    IEnumerator Slide(Vector2 from, Vector2 to, Action onComplete)
    {
        float time = 0f;

        while (time < slideDuration)
        {
            float t = time / slideDuration;
            t = 1 - Mathf.Pow(1 - t, 3);

            taskWindow.anchoredPosition = Vector2.Lerp(from, to, t);

            time += Time.unscaledDeltaTime;
            yield return null;
        }

        taskWindow.anchoredPosition = to;
        running = null;
        onComplete?.Invoke();
    }
}