using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Collections;

public class MapTaskManager : MonoBehaviour
{
    [Header("UI Slots")]
    [FormerlySerializedAs("counterText")]
    public TMP_Text counterText;
    [FormerlySerializedAs("instructionText")]
    public TMP_Text instructionText;
    [FormerlySerializedAs("closeButton")]
    public Button closeButton;

    [Header("Settings")]
    [FormerlySerializedAs("totalRocks")]
    public int totalTargets = 5;
    public float completeSoundDelay = 0.3f;
    public float closeDelay = 1.2f;

    [Header("Task State")]
    [FormerlySerializedAs("isCompleted")]
    public bool taskCompleted;

    [Header("Reward")]
    public string rewardItemID = "Map";

    [Header("Wwise Events")]
    public AK.Wwise.Event rockDropEvent;
    public AK.Wwise.Event taskCompleteEvent;

    private TaskWindowSlide slide;
    private UIDragRock[] rocks;

    private int clearedCount;
    private bool isClosing;
    private bool hasWarnedMissingRocks;

    public bool IsCompleted => taskCompleted;

    private void Awake()
    {
        CacheReferences();
    }

    private void Start()
    {
        CacheReferences();

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseTask);
            closeButton.onClick.AddListener(CloseTask);
        }

        if (InventorySystem.Instance != null && InventorySystem.Instance.Has(rewardItemID))
        {
            taskCompleted = true;
            clearedCount = totalTargets;
        }

        RefreshUI();
    }

    private void OnEnable()
    {
        CacheReferences();
        isClosing = false;

        if (taskCompleted)
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
        if (taskCompleted)
        {
            RefreshUI();
            return;
        }

        ResetTaskVisuals();
        RefreshUI();
    }

    public void NotifyRockCleared()
    {
        if (taskCompleted)
        {
            return;
        }

        clearedCount = Mathf.Min(clearedCount + 1, totalTargets);

        if (rockDropEvent != null)
        {
            rockDropEvent.Post(gameObject);
        }

        if (clearedCount >= totalTargets)
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
            TaskUICommon.CloseTaskWindow(gameObject, slide, null);
            return;
        }

        FinalizeClose();
    }

    private void CompleteTask()
    {
        if (taskCompleted)
        {
            return;
        }

        taskCompleted = true;

        RefreshUI();

        TaskUICommon.GrantReward(rewardItemID);
        StartCoroutine(DelayedClose());
    }

    private IEnumerator DelayedClose()
    {
        if (completeSoundDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(completeSoundDelay);
        }

        if (taskCompleteEvent != null)
        {
            taskCompleteEvent.Post(gameObject);
        }

        if (closeDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(closeDelay);
        }

        CloseTask();
    }

    private void FinalizeClose()
    {
        TaskUICommon.CloseTaskWindow(gameObject, null, null);
    }

    private void ResetTaskVisuals()
    {
        clearedCount = 0;

        CacheReferences();

        if (rocks == null || rocks.Length == 0)
        {
            if (!hasWarnedMissingRocks)
            {
                hasWarnedMissingRocks = true;
                Debug.LogWarning($"[{nameof(MapTaskManager)}] No rocks found under {name}.", this);
            }
            return;
        }

        foreach (UIDragRock rock in rocks)
        {
            if (rock == null)
            {
                continue;
            }

            rock.ResetRock();
        }
    }

    private void HideAllRocks()
    {
        CacheReferences();

        if (rocks == null || rocks.Length == 0)
        {
            return;
        }

        foreach (UIDragRock rock in rocks)
        {
            if (rock == null)
            {
                continue;
            }

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
            counterText.text = $"{clearedCount}/{totalTargets}";
        }
    }

    private void CacheReferences()
    {
        if (slide == null)
        {
            slide = GetComponent<TaskWindowSlide>();
        }

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

        RebuildRockCache();
    }

    private void RebuildRockCache()
    {
        UIDragRock[] discoveredRocks = GetComponentsInChildren<UIDragRock>(true);
        if (discoveredRocks == null || discoveredRocks.Length == 0)
        {
            rocks = discoveredRocks;
            return;
        }

        List<UIDragRock> validRocks = new List<UIDragRock>(discoveredRocks.Length);
        foreach (UIDragRock rock in discoveredRocks)
        {
            if (rock == null)
            {
                continue;
            }

            rock.BindManager(this);
            validRocks.Add(rock);
        }

        rocks = validRocks.ToArray();
    }
}
