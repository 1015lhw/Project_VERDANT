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

        // 设置到屏幕下方足够远的地方
        hiddenPos = new Vector2(0, -Screen.height * 1.5f);
    }

    void OnEnable()
    {
        if (taskWindow == null) return;

        // 激活时先隐藏在屏幕外
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
            // 三次方易入易出
            t = 1 - Mathf.Pow(1 - t, 3);

            taskWindow.anchoredPosition = Vector2.Lerp(from, to, t);

            // Time.timeScale 为 0 时也能运动
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        taskWindow.anchoredPosition = to;
        running = null;
        onComplete?.Invoke();
    }
}