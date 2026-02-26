using UnityEngine;

public class AffectionDebugListener : MonoBehaviour
{
    private void OnEnable()
    {
        if (ReputationManager.Instance != null)
            ReputationManager.Instance.OnAffectionChanged += Handle;
    }

    private void OnDisable()
    {
        if (ReputationManager.Instance != null)
            ReputationManager.Instance.OnAffectionChanged -= Handle;
    }

    private void Handle(NpcId npc, int oldV, int newV, int delta, string reason)
    {
        Debug.Log($"[Affection] {npc}: {oldV} -> {newV} (жд{delta}) reason={reason}");
    }
}