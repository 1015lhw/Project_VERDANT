using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BerryTaskManager : MonoBehaviour
{
    [Header("UI Slots")]
    public TMP_Text counterText;
    public TMP_Text instructionText;
    public Button closeButton;

    [Header("Settings")]
    public float closeDelay = 0.5f;

    [Header("Task State")]
    public bool taskCompleted = false;

    [Header("Scene Links")]
    public BerryShrubSwitcher shrubSwitcher;

    [Header("Reward")]
    public string rewardItemID = "Berry";

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

        ApplyShrubState();

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

    public void SetShrubSwitcher(BerryShrubSwitcher switcher)
    {
        if (switcher == null) return;

        shrubSwitcher = switcher;
        ApplyShrubState();
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

            ApplyShrubState();

            TaskUICommon.GrantReward(rewardItemID);
            StartCoroutine(DelayedClose());
        }
    }

    IEnumerator DelayedClose()
    {
        yield return new WaitForSecondsRealtime(closeDelay);
        SlideAndClose();
    }

    public void CloseTask() => SlideAndClose();

    void SlideAndClose()
    {
        TaskUICommon.CloseTaskWindow(gameObject, slide, () =>
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        });
    }

    void UpdateUI()
    {
        if (instructionText != null)
            instructionText.text = "Tap on berries to harvest";

        if (counterText != null)
            counterText.text = $"{clickedCount}/{total}";
    }

    private void ApplyShrubState()
    {
        if (shrubSwitcher == null) return;

        if (taskCompleted)
            shrubSwitcher.SwitchToEmpty();
        else
            shrubSwitcher.SwitchToWithFruit();
    }
}
