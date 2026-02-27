using UnityEngine;

public class MapStoneInteraction : MonoBehaviour
{
    [SerializeField] private MapTaskManager mapTaskManager;
    [SerializeField] private GameObject pressEPrompt;

    [SerializeField] private bool playerInRange;

    private GameObject mapTaskUI;
    private bool wasMapTaskUIOpen;

    private void Start()
    {
        if (mapTaskManager != null)
        {
            mapTaskUI = mapTaskManager.gameObject;
        }

        if (pressEPrompt != null)
        {
            PressEPromptCoordinator.SetRequest(pressEPrompt, this, false);
        }

        if (mapTaskUI != null)
        {
            mapTaskUI.SetActive(false);
        }

        wasMapTaskUIOpen = mapTaskUI != null && mapTaskUI.activeInHierarchy;
    }

    private void Update()
    {
        bool isMapTaskUIOpen = mapTaskUI != null && mapTaskUI.activeInHierarchy;

        // 状态自愈（限定到本任务）：
        // 只有“自己之前打开过且现在关闭”的时候，才重置 Task -> Normal。
        // 避免在其他任务 UI 打开时误把全局 Task 状态重置掉。
        if (wasMapTaskUIOpen
            && !isMapTaskUIOpen
            && GameStateManager.CurrentState == GameState.Task)
        {
            GameStateManager.ResetToNormal();
        }

        wasMapTaskUIOpen = isMapTaskUIOpen;

        RefreshPrompt();

        // Keep E interaction one-shot while task window is already open.
        if (isMapTaskUIOpen)
        {
            return;
        }

        if (!playerInRange || !GameStateManager.IsNormal)
        {
            return;
        }

        if (mapTaskManager == null || mapTaskManager.IsCompleted)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenTask();
        }
    }

    private void OpenTask()
    {
        if (mapTaskManager == null || mapTaskManager.IsCompleted)
        {
            return;
        }

        if (!GameStateManager.IsNormal)
        {
            return;
        }

        mapTaskUI = mapTaskManager.gameObject;
        if (mapTaskUI == null)
        {
            return;
        }

        if (mapTaskUI.activeInHierarchy)
        {
            return;
        }

        GameStateManager.SetState(GameState.Task);
        mapTaskUI.SetActive(true);

        mapTaskManager.PrepareTask();

        TaskWindowSlide slide = mapTaskUI.GetComponent<TaskWindowSlide>();
        if (slide != null)
        {
            slide.PlaySlideIn();
        }

        if (pressEPrompt != null)
        {
            PressEPromptCoordinator.SetRequest(pressEPrompt, this, false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;

        if (pressEPrompt != null)
        {
            PressEPromptCoordinator.SetRequest(pressEPrompt, this, false);
        }
    }

    private void OnDisable()
    {
        PressEPromptCoordinator.ClearRequester(this);
    }

    private void RefreshPrompt()
    {
        bool showPrompt = playerInRange
                          && GameStateManager.IsNormal
                          && mapTaskManager != null
                          && !mapTaskManager.IsCompleted
                          && (mapTaskUI == null || !mapTaskUI.activeInHierarchy);

        PressEPromptCoordinator.SetRequest(pressEPrompt, this, showPrompt);
    }
}
