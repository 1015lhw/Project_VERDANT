using UnityEngine;

public class Berries_Interaction : TaskInteractionBase
{
    [Header("Task")]
    [SerializeField] private GameObject berryTaskUI;

    private BerryTaskManager taskManager;
    private BerryShrubSwitcher shrubSwitcher;

    // Backward-compatible accessor for debug/legacy scripts still reading the old field name.
    public GameObject pressEUI => pressEPrompt;

    protected override GameObject TaskUI => berryTaskUI;
    protected override bool IsTaskCompleted => taskManager != null && taskManager.taskCompleted;

    protected override void Start()
    {
        if (berryTaskUI != null)
        {
            taskManager = berryTaskUI.GetComponent<BerryTaskManager>();
        }

        shrubSwitcher = GetComponent<BerryShrubSwitcher>();
        if (shrubSwitcher == null)
        {
            shrubSwitcher = GetComponentInParent<BerryShrubSwitcher>();
        }

        if (taskManager != null && shrubSwitcher != null)
        {
            taskManager.SetShrubSwitcher(shrubSwitcher);
        }

        base.Start();
    }

    protected override void PrepareTask()
    {
        if (taskManager == null)
        {
            return;
        }

        if (shrubSwitcher != null)
        {
            taskManager.SetShrubSwitcher(shrubSwitcher);
        }

        taskManager.PrepareTask();
    }
}
