using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // 必须引用这个才能使用协程

public class BerryTaskManager : MonoBehaviour
{
    [Header("UI Slots (手动拖拽)")]
    public TMP_Text counterText;     // 显示 0/5 的数字
    public TMP_Text instructionText; // 显示 "Tap on berries..." 的文字
    public Button closeButton;

    [Header("Settings")]
    public float closeDelay = 0.5f;  // 控制任务完成后延迟关闭的速度

    [Header("Task State")]
    public bool taskCompleted = false;

    private Button[] berries;
    private TaskWindowSlide slide;
    private int clickedCount = 0;
    private const int total = 5;

    void Awake()
    {
        slide = GetComponent<TaskWindowSlide>();
        InitBerries();

        if (counterText == null) counterText = transform.Find("TaskWindow/CounterText")?.GetComponent<TMP_Text>();
        if (instructionText == null) instructionText = transform.Find("TaskWindow/InstructionText")?.GetComponent<TMP_Text>();
        if (closeButton == null) closeButton = transform.Find("TaskWindow/CloseButton")?.GetComponent<Button>();
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTask);
        
        UpdateUI();
    }

    private void InitBerries()
    {
        berries = new Button[total];
        for (int i = 0; i < total; i++)
        {
            Transform t = transform.Find($"TaskWindow/Berry{i + 1}");
            if (t != null) 
            {
                berries[i] = t.GetComponent<Button>();
                int index = i; 
                berries[i].onClick.AddListener(() => OnBerryClicked(berries[index]));
            }
        }
    }

    public void PrepareTask()
    {
        if (taskCompleted) return;
        clickedCount = 0;
        foreach (var b in berries) if (b != null) b.gameObject.SetActive(true);
        UpdateUI();
    }

    void OnBerryClicked(Button berry)
    {
        if (taskCompleted) return;
        berry.gameObject.SetActive(false);
        clickedCount++;
        UpdateUI();

        if (clickedCount >= total)
        {
            taskCompleted = true;
            // 启动延迟关闭的协程
            StartCoroutine(DelayedClose());
        }
    }

    // 协程：等待指定时间后再执行滑出动画
    IEnumerator DelayedClose()
    {
        // 即使游戏暂停(Time.scale=0)，WaitForSecondsRealtime 依然有效
        yield return new WaitForSecondsRealtime(closeDelay);
        SlideAndClose();
    }

    public void CloseTask() => SlideAndClose();

    void SlideAndClose()
    {
        if (slide != null)
        {
            slide.PlaySlideOut(() =>
            {
                gameObject.SetActive(false);
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            });
        }
    }

    void UpdateUI()
    {
        if (instructionText != null) 
            instructionText.text = "Tap on berries to harvest";
        
        if (counterText != null) 
            counterText.text = $"{clickedCount}/{total}";
    }
}