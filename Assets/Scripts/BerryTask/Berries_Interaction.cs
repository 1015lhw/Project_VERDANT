using UnityEngine;

public class Berries_Interaction : MonoBehaviour
{
    public GameObject pressEUI;
    public GameObject berryTaskUI;

    private bool playerInRange = false;
    private BerryTaskManager taskManager;
    private BerryShrubSwitcher shrubSwitcher;

    void Start()
    {
        if (pressEUI != null) pressEUI.SetActive(false);
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
    }

    void Update()
    {
        // 状态自愈：如果任务 UI 意外关闭，重置游戏状态
        if (GameStateManager.CurrentState == GameState.Task
            && (berryTaskUI == null || !berryTaskUI.activeInHierarchy))
        {
            GameStateManager.ResetToNormal();
        }

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

        if (pressEUI != null) pressEUI.SetActive(false);

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
    }

    private void RefreshPrompt()
    {
        if (pressEUI == null) return;

        // 核心修改：简化判断，确保 UI 能够正常激活
        // 只要玩家在范围内，且游戏处于正常状态，且任务未完成（如果没挂载任务脚本则默认未完成）
        bool isTaskDone = (taskManager != null && taskManager.taskCompleted);
        
        bool showPrompt = playerInRange 
            && GameStateManager.IsNormal 
            && !isTaskDone;

        if (pressEUI.activeSelf != showPrompt)
        {
            pressEUI.SetActive(showPrompt);
        }
    }
}