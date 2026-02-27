using UnityEngine;

public class PressE_Interact : MonoBehaviour
{
    public GameObject pressEUI;
    public TextAsset inkJSON;
    public Sprite portrait;

    private bool playerInRange;

    void Start()
    {
        PressEPromptCoordinator.SetRequest(pressEUI, this, false);
    }

    void Update()
    {
        if (!playerInRange) return;
        if (!GameStateManager.IsNormal) return;

        // If the dialogue is opne, don't open it again
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            PressEPromptCoordinator.SetRequest(pressEUI, this, false);

            DialogueManager.Instance.StartDialogue(inkJSON, portrait);
        }
    }

    void LateUpdate()
    {
        bool shouldShow = playerInRange
            && GameStateManager.IsNormal
            && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);

        PressEPromptCoordinator.SetRequest(pressEUI, this, shouldShow);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // If the dialogue is closed and global state allows interaction, show Press E
        if (GameStateManager.IsNormal && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen))
            PressEPromptCoordinator.SetRequest(pressEUI, this, true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        PressEPromptCoordinator.SetRequest(pressEUI, this, false);
    }

    void OnDisable()
    {
        PressEPromptCoordinator.ClearRequester(this);
    }
}
