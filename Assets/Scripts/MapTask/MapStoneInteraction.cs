using UnityEngine;

public class MapStoneInteraction : MonoBehaviour
{
    [SerializeField] private MapTaskManager mapTaskManager;
    [SerializeField] private GameObject pressEPrompt;

    [SerializeField] private bool playerInRange;

    private GameObject mapTaskUI;

    private void Start()
    {
        if (mapTaskManager != null)
        {
            mapTaskUI = mapTaskManager.gameObject;
        }

        if (pressEPrompt != null)
        {
            pressEPrompt.SetActive(false);
        }

        if (mapTaskUI != null)
        {
            mapTaskUI.SetActive(false);
        }
    }

    private void Update()
    {
        bool isTaskUIOpen = mapTaskUI != null && mapTaskUI.activeInHierarchy;

        if (GameStateManager.CurrentState == GameState.Task && (mapTaskUI == null || !mapTaskUI.activeInHierarchy))
        {
            GameStateManager.ResetToNormal();
        }

        RefreshPrompt();

        // Keep E interaction one-shot while task window is already open.
        if (isTaskUIOpen)
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
            pressEPrompt.SetActive(false);
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
            pressEPrompt.SetActive(false);
        }
    }

    private void RefreshPrompt()
    {
        if (pressEPrompt == null)
        {
            return;
        }

        bool showPrompt = playerInRange
                          && GameStateManager.IsNormal
                          && mapTaskManager != null
                          && !mapTaskManager.IsCompleted
                          && (mapTaskUI == null || !mapTaskUI.activeInHierarchy);

        if (pressEPrompt.activeSelf != showPrompt)
        {
            pressEPrompt.SetActive(showPrompt);
        }
    }
}
