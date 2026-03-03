using UnityEngine;
using UnityEngine.Serialization;

public class MapStoneInteraction : TaskInteractionBase
{
    [Header("Task")]
    [SerializeField] private GameObject mapTaskUI;

    // Legacy slot to keep old scene references valid when upgrading to unified task UI field.
    [FormerlySerializedAs("mapTaskManager")]
    [SerializeField, HideInInspector] private MapTaskManager legacyTaskManager;

    private MapTaskManager taskManager;

    protected override GameObject TaskUI => mapTaskUI;
    protected override bool IsTaskCompleted => taskManager == null || taskManager.IsCompleted;

    protected override void Start()
    {
        ResolveReferences();
        base.Start();
    }

    protected override void PrepareTask()
    {
        ResolveReferences();

        if (taskManager == null)
        {
            return;
        }

        taskManager.PrepareTask();
    }

    private void OnValidate()
    {
        ResolveReferences();
    }

    private void ResolveReferences()
    {
        if (mapTaskUI == null && legacyTaskManager != null)
        {
            mapTaskUI = legacyTaskManager.gameObject;
        }

        if (taskManager == null && mapTaskUI != null)
        {
            taskManager = mapTaskUI.GetComponent<MapTaskManager>();
        }
    }
}
