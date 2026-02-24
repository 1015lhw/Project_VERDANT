using UnityEngine;

public class Berries_Interaction : MonoBehaviour
{
    public GameObject pressEUI;
    public GameObject berryTaskUI;

    private bool playerInRange = false;
    private BerryTaskManager taskManager;

    void Start()
    {
        if (pressEUI != null) pressEUI.SetActive(false);
        if (berryTaskUI != null) berryTaskUI.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (berryTaskUI != null)
            taskManager = berryTaskUI.GetComponent<BerryTaskManager>();
    }

    void Update()
    {
        if (!playerInRange) return;
        
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

        if (taskManager != null) taskManager.PrepareTask();

        var slide = berryTaskUI.GetComponent<TaskWindowSlide>();
        if (slide != null) slide.PlaySlideIn();

        if (pressEUI != null) pressEUI.SetActive(false);

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