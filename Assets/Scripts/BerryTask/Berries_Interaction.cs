using UnityEngine;

public class Berries_Interaction : MonoBehaviour
{
    public GameObject pressEUI;
    public GameObject berryTaskUI;

    private bool playerInRange = false;
    private BerryTaskManager taskManager;

    void Start()
    {
        // 初始隐藏 UI
        if (pressEUI != null) pressEUI.SetActive(false);
        if (berryTaskUI != null) berryTaskUI.SetActive(false);

        // 核心修复：确保游戏开始时鼠标是可见且不锁定的
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (berryTaskUI != null)
            taskManager = berryTaskUI.GetComponent<BerryTaskManager>();
    }

    void Update()
    {
        if (!playerInRange) return;
        
        // 如果任务已完成，隐藏提示并拦截交互
        if (taskManager != null && taskManager.taskCompleted)
        {
            if (pressEUI != null && pressEUI.activeSelf) pressEUI.SetActive(false);
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

        berryTaskUI.SetActive(true);

        // 每次打开时准备/重置数据
        if (taskManager != null) taskManager.PrepareTask();

        // 动画滑入
        var slide = berryTaskUI.GetComponent<TaskWindowSlide>();
        if (slide != null) slide.PlaySlideIn();

        if (pressEUI != null) pressEUI.SetActive(false);

        // 暂停游戏，确保鼠标状态正确
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;

        if (taskManager != null && !taskManager.taskCompleted)
        {
            if (pressEUI != null) pressEUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (pressEUI != null) pressEUI.SetActive(false);
    }
}