using UnityEngine;
using UnityEngine.Serialization;

public abstract class TaskInteractionBase : MonoBehaviour
{
    [Header("Prompt")]
    [FormerlySerializedAs("pressEUI")]
    [FormerlySerializedAs("pressEPrompt")]
    [SerializeField] protected GameObject pressEPrompt;

    protected bool playerInRange;

    private bool wasTaskUIOpen;

    protected abstract GameObject TaskUI { get; }
    protected abstract bool IsTaskCompleted { get; }
    protected virtual bool CanInteractNow => true;

    protected virtual void Start()
    {
        PressEPromptCoordinator.SetRequest(pressEPrompt, this, false);

        if (TaskUI != null)
        {
            TaskUI.SetActive(false);
        }

        wasTaskUIOpen = TaskUI != null && TaskUI.activeInHierarchy;
    }

    protected virtual void Update()
    {
        bool isTaskUIOpen = TaskUI != null && TaskUI.activeInHierarchy;

        if (wasTaskUIOpen && !isTaskUIOpen && GameStateManager.CurrentState == GameState.Task)
        {
            GameStateManager.ResetToNormal();
        }

        wasTaskUIOpen = isTaskUIOpen;

        RefreshPrompt();

        if (!playerInRange || !GameStateManager.IsNormal || IsTaskCompleted || !CanInteractNow)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenTask();
        }
    }

    protected virtual void OpenTask()
    {
        if (TaskUI == null || !GameStateManager.IsNormal || IsTaskCompleted)
        {
            return;
        }

        if (TaskUI.activeInHierarchy)
        {
            return;
        }

        GameStateManager.SetState(GameState.Task);
        TaskUI.SetActive(true);

        PrepareTask();

        TaskWindowSlide slide = TaskUI.GetComponent<TaskWindowSlide>();
        if (slide != null)
        {
            slide.PlaySlideIn();
        }

        PressEPromptCoordinator.SetRequest(pressEPrompt, this, false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    protected abstract void PrepareTask();

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
        PressEPromptCoordinator.SetRequest(pressEPrompt, this, false);
    }

    private void OnDisable()
    {
        PressEPromptCoordinator.ClearRequester(this);
    }

    private void RefreshPrompt()
    {
        bool showPrompt = playerInRange
                          && GameStateManager.IsNormal
                          && !IsTaskCompleted
                          && CanInteractNow
                          && (TaskUI == null || !TaskUI.activeInHierarchy);

        PressEPromptCoordinator.SetRequest(pressEPrompt, this, showPrompt);
    }
}
