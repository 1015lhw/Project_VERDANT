using UnityEngine;

public class Berries_Interaction : MonoBehaviour
{
    public GameObject pressEUI;
    public GameObject berryTaskUI;

    private bool playerInRange = false;
    private bool wasBerryTaskUIOpen = false;
    private BerryTaskManager taskManager;
    private BerryShrubSwitcher shrubSwitcher;

    void Start()
    {
        PressEPromptCoordinator.SetRequest(pressEUI, this, false);
        if (berryTaskUI != null) berryTaskUI.SetActive(false);

        // 默认不锁定鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (berryTaskUI != null)
            taskManager = berryTaskUI.GetComponent<BerryTaskManager>();

        shrubSwitcher = GetComponent<BerryShrubSwitcher>();
        if (shrubSwitcher == null)
            shrubSwitcher = GetComponentInParent<BerryShrubSwitcher>();

        if (taskManager != null && shrubSwitcher != null)
            taskManager.SetShrubSwitcher(shrubSwitcher);

        wasBerryTaskUIOpen = berryTaskUI != null && berryTaskUI.activeInHierarchy;
    }

    void Update()
    {
        bool isBerryTaskUIOpen = berryTaskUI != null && berryTaskUI.activeInHierarchy;

        // 状态自愈（限定到本任务）：
        // 只有“自己之前是打开的”且“现在关闭了”时，才把 Task 重置回 Normal。
        // 避免误伤其他任务（例如 MapTask）导致 Task 状态被提前恢复。
        if (wasBerryTaskUIOpen
            && !isBerryTaskUIOpen
            && GameStateManager.CurrentState == GameState.Task)
        {
            GameStateManager.ResetToNormal();
        }

        wasBerryTaskUIOpen = isBerryTaskUIOpen;

        // 刷新 E 提示显示
        RefreshPrompt();

        if (!playerInRange) return;
        if (!GameStateManager.IsNormal) return;

        // 如果任务管理器显示任务已完成，则不执行交互逻辑
        if (taskManager != null && taskManager.taskCompleted)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenTask();
        }
    }

    void OpenTask()
    {
        if (berryTaskUI == null) return;
        if (!GameStateManager.IsNormal) return;

        GameStateManager.SetState(GameState.Task);
        berryTaskUI.SetActive(true);

        if (taskManager != null)
        {
            if (shrubSwitcher != null)
                taskManager.SetShrubSwitcher(shrubSwitcher);

            taskManager.PrepareTask();
        }

        var slide = berryTaskUI.GetComponent<TaskWindowSlide>();
        if (slide != null) slide.PlaySlideIn();

        PressEPromptCoordinator.SetRequest(pressEUI, this, false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        PressEPromptCoordinator.SetRequest(pressEUI, this, false);
    }

    private void RefreshPrompt()
    {
        // 核心修改：简化判断，确保 UI 能够正常激活
        // 只要玩家在范围内，且游戏处于正常状态，且任务未完成（如果没挂载任务脚本则默认未完成）
        bool isTaskDone = (taskManager != null && taskManager.taskCompleted);

        bool showPrompt = playerInRange
            && GameStateManager.IsNormal
            && !isTaskDone;

        PressEPromptCoordinator.SetRequest(pressEUI, this, showPrompt);
    }

    private void OnDisable()
    {
        PressEPromptCoordinator.ClearRequester(this);
    }
}
