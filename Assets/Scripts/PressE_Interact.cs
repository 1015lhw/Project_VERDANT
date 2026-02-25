using UnityEngine;

public class PressE_Interact : MonoBehaviour
{
    public GameObject pressEUI;
    public TextAsset inkJSON;

    private bool playerInRange;

    void Start()
    {
        if (pressEUI != null) pressEUI.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;
        if (!GameStateManager.IsNormal) return;

        // If the dialogue is opne, don't open it again
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (pressEUI != null) pressEUI.SetActive(false);

            DialogueManager.Instance.StartDialogue(inkJSON);
        }
    }

    void LateUpdate()
    {
        if (pressEUI == null) return;

        bool shouldShow = playerInRange
            && GameStateManager.IsNormal
            && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);

        if (pressEUI.activeSelf != shouldShow)
            pressEUI.SetActive(shouldShow);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // If the dialogue is closed and global state allows interaction, show Press E
        if (GameStateManager.IsNormal && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen))
            if (pressEUI != null) pressEUI.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (pressEUI != null) pressEUI.SetActive(false);
    }
}
