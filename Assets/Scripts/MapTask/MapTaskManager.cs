using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapTaskManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private Button closeButton;

    [Header("Settings")]
    [SerializeField] private int totalRocks = 5;
    [SerializeField] private string rewardItemID = "Map";

    private TaskWindowSlide slide;
    private UIDragRock[] rocks;

    private int clearedCount;
    private bool isCompleted;
    private bool isClosing;

    public bool IsCompleted => isCompleted;

    private void Awake()
    {
        slide = GetComponent<TaskWindowSlide>();

        if (counterText == null)
        {
            counterText = transform.Find("TaskWindow_Stone/CounterText_Stone")?.GetComponent<TMP_Text>();
        }

        if (instructionText == null)
        {
            instructionText = transform.Find("TaskWindow_Stone/InstructionText_Stone")?.GetComponent<TMP_Text>();
        }

        if (closeButton == null)
        {
            closeButton = transform.Find("TaskWindow_Stone/CloseButton_Stone")?.GetComponent<Button>();
        }

        rocks = GetComponentsInChildren<UIDragRock>(true);
        foreach (UIDragRock rock in rocks)
        {
            rock.BindManager(this);
        }
    }

    private void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseTask);
            closeButton.onClick.AddListener(CloseTask);
        }

        if (InventorySystem.Instance != null && InventorySystem.Instance.Has(rewardItemID))
        {
            isCompleted = true;
            clearedCount = totalRocks;
        }

        RefreshUI();
    }

    private void OnEnable()
    {
        isClosing = false;

        if (isCompleted)
        {
            HideAllRocks();
        }
        else
        {
            ResetTaskVisuals();
        }

        RefreshUI();
    }

    public void PrepareTask()
    {
        if (isCompleted)
        {
            RefreshUI();
            return;
        }

        ResetTaskVisuals();
        RefreshUI();
    }

    public void NotifyRockCleared()
    {
        if (isCompleted)
        {
            return;
        }

        clearedCount = Mathf.Min(clearedCount + 1, totalRocks);

        if (clearedCount >= totalRocks)
        {
            CompleteTask();
            return;
        }

        RefreshUI();
    }

    public void CloseTask()
    {
        if (isClosing)
        {
            return;
        }

        isClosing = true;

        if (slide != null)
        {
            slide.PlaySlideOut(FinalizeClose);
            return;
        }

        FinalizeClose();
    }

    private void CompleteTask()
    {
        if (isCompleted)
        {
            return;
        }

        isCompleted = true;
        RefreshUI();

        if (InventorySystem.Instance != null && !InventorySystem.Instance.Has(rewardItemID))
        {
            InventorySystem.Instance.AddItem(rewardItemID);
        }

        CloseTask();
    }

    private void FinalizeClose()
    {
        GameStateManager.ResetToNormal();
        gameObject.SetActive(false);
    }

    private void ResetTaskVisuals()
    {
        clearedCount = 0;

        if (rocks == null)
        {
            rocks = GetComponentsInChildren<UIDragRock>(true);
        }

        foreach (UIDragRock rock in rocks)
        {
            rock.ResetRock();
        }
    }

    private void HideAllRocks()
    {
        if (rocks == null)
        {
            rocks = GetComponentsInChildren<UIDragRock>(true);
        }

        foreach (UIDragRock rock in rocks)
        {
            rock.MarkRemoved();
        }
    }

    private void RefreshUI()
    {
        if (instructionText != null)
        {
            instructionText.text = "Drag rocks away to reveal the map";
        }

        if (counterText != null)
        {
            counterText.text = $"{clearedCount} / {totalRocks}";
        }
    }
}
