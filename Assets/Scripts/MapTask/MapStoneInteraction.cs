using UnityEngine;

public class MapStoneInteraction : TaskInteractionBase
{
    [Header("Task")]
    [SerializeField] private MapTaskManager mapTaskManager;

    protected override GameObject TaskUI => mapTaskManager != null ? mapTaskManager.gameObject : null;
    protected override bool IsTaskCompleted => mapTaskManager == null || mapTaskManager.IsCompleted;

    protected override void PrepareTask()
    {
        if (mapTaskManager == null)
        {
            return;
        }

        mapTaskManager.PrepareTask();
    }
}
