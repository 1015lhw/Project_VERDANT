using UnityEngine;

[RequireComponent(typeof(AkGameObj))]
public class AkMultiPositionFromChild : MonoBehaviour
{
    private void Awake()
    {
        Debug.LogError($"🍃 AWAKE: {name} is running!");
        SetPositions();
    }

    private void OnEnable()
    {
        Debug.LogError($"🍃 ON ENABLE: {name} is running!");
        SetPositions();
    }

    private void Start()
    {
        Debug.LogError($"🍃 START: {name} is running!");
        SetPositions();
    }

    private void SetPositions()
    {
        int count = transform.childCount;

        Debug.LogError($"🍃 {name}: child count = {count}");

        if (count == 0)
        {
            Debug.LogWarning($"{name}: No child positions for Wwise multi-position.");
            return;
        }

        AkPositionArray positions = new AkPositionArray((uint)count);

        foreach (Transform child in transform)
        {
            positions.Add(child.position, child.forward, child.up);
        }

        AkSoundEngine.SetMultiplePositions(
            gameObject,
            positions,
            (ushort)count,
            AkMultiPositionType.AkMultiPositionType_MultiSources
        );

        Debug.LogError($"🍃 {name}: Set {count} Wwise Multi-position Points.");
    }
}