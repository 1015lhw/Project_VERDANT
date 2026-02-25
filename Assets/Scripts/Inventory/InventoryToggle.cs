using UnityEngine;
using System.Collections;

public class InventoryToggle : MonoBehaviour
{
    public RectTransform panel;
    public CanvasGroup dimCanvasGroup;  // 用 CanvasGroup 控制透明度

    public float slideSpeed = 10f;
    public float dimFadeTime = 0.15f;   // ⭐ dim关闭时间

    private bool isOpen = false;
    private Vector2 shownPos;
    private Vector2 hiddenPos;

    void Start()
    {
        shownPos = panel.anchoredPosition;
        hiddenPos = shownPos + Vector2.down * 1200f;

        panel.anchoredPosition = hiddenPos;

        dimCanvasGroup.alpha = 0f;
        dimCanvasGroup.gameObject.SetActive(false);
    }

    void Update()
    {
        // Self-heal: if state says Inventory but panel is already closed,
        // unlock other interactions (Press E / tasks / dialogue).
        if (!isOpen && GameStateManager.CurrentState == GameState.Inventory)
        {
            GameStateManager.ResetToNormal();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            if (CanToggleInventory())
                Toggle();
        }

        AnimatePanel();
    }

    void Toggle()
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            GameStateManager.SetState(GameState.Inventory);
            dimCanvasGroup.gameObject.SetActive(true);
            StartCoroutine(FadeDim(1f));
        }
        else
        {
            GameStateManager.ResetToNormal();
            StartCoroutine(FadeDim(0f));
        }
    }

    bool CanToggleInventory()
    {
        return GameStateManager.CurrentState == GameState.Normal
            || (GameStateManager.CurrentState == GameState.Inventory && isOpen);
    }

    void AnimatePanel()
    {
        Vector2 target = isOpen ? shownPos : hiddenPos;

        panel.anchoredPosition =
            Vector2.Lerp(panel.anchoredPosition,
                target,
                Time.deltaTime * slideSpeed);
    }

    IEnumerator FadeDim(float targetAlpha)
    {
        float startAlpha = dimCanvasGroup.alpha;
        float time = 0f;

        while (time < dimFadeTime)
        {
            time += Time.deltaTime;
            dimCanvasGroup.alpha =
                Mathf.Lerp(startAlpha, targetAlpha, time / dimFadeTime);
            yield return null;
        }

        dimCanvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f)
            dimCanvasGroup.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (GameStateManager.CurrentState == GameState.Inventory)
            GameStateManager.ResetToNormal();
    }
}
