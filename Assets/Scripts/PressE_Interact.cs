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

        // If the dialogue is opne, don't open it again
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (pressEUI != null) pressEUI.SetActive(false);

            DialogueManager.Instance.StartDialogue(inkJSON);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // If the dialogue is closed,don't show Press E
        if (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen)
            if (pressEUI != null) pressEUI.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (pressEUI != null) pressEUI.SetActive(false);
    }
}
