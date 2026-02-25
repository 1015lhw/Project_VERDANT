using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject dim;
    public RectTransform panel;

    public float slideDistance = 1200f;   // 往下滑多远
    public float slideSpeed = 10f;

    private bool isOpen = false;
    private Vector2 shownPos;
    private Vector2 hiddenPos;

    void Start()
    {
        // 记录你在编辑器里摆好的位置
        shownPos = panel.anchoredPosition;

        // 自动计算隐藏位置
        hiddenPos = shownPos + Vector2.down * slideDistance;

        panel.anchoredPosition = hiddenPos;
        panel.gameObject.SetActive(false);
        dim.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                panel.gameObject.SetActive(true);
                dim.SetActive(true);
            }
        }

        Animate();

        if (!isOpen && Vector2.Distance(panel.anchoredPosition, hiddenPos) < 1f)
        {
            panel.gameObject.SetActive(false);
            dim.SetActive(false);
        }
    }

    void Animate()
    {
        Vector2 target = isOpen ? shownPos : hiddenPos;

        panel.anchoredPosition =
            Vector2.Lerp(panel.anchoredPosition,
                target,
                Time.deltaTime * slideSpeed);
    }
}