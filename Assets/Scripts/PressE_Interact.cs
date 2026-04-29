using System.Collections.Generic;
using UnityEngine;

public class PressE_Interact : MonoBehaviour
{
    public GameObject pressEUI;
    public TextAsset inkJSON;
    [Header("Optional first-time dialogue")]
    public TextAsset firstTimeInkJSON;
    [Tooltip("Unique id used to remember first-time dialogue completion.")]
    public string firstTalkSaveKey;
    [Tooltip("If enabled, clear the first-talk flag once when the game process starts so first talk can be re-tested after restart.")]
    public bool resetFirstTalkOnSessionStart = true;
    public Sprite portrait;

    private bool playerInRange;
    private static readonly HashSet<string> sessionResetKeys = new HashSet<string>();

    void Start()
    {
        ResetFirstTalkIfNeeded();
        PressEPromptCoordinator.SetRequest(pressEUI, this, false);
    }

    void Update()
    {
        if (!playerInRange) return;
        if (OpeningLock.IsLocked) return;
        if (!GameStateManager.IsNormal) return;

        // If the dialogue is opne, don't open it again
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            PressEPromptCoordinator.SetRequest(pressEUI, this, false);
            DialogueManager.Instance.StartDialogue(GetDialogueToPlay(), portrait);
            MarkFirstTalkAsPlayed();
        }
    }


    void ResetFirstTalkIfNeeded()
    {
        if (!resetFirstTalkOnSessionStart) return;
        if (string.IsNullOrWhiteSpace(firstTalkSaveKey)) return;
        if (sessionResetKeys.Contains(firstTalkSaveKey)) return;

        PlayerPrefs.DeleteKey(firstTalkSaveKey);
        sessionResetKeys.Add(firstTalkSaveKey);
        PlayerPrefs.Save();
    }
    TextAsset GetDialogueToPlay()
    {
        if (firstTimeInkJSON == null) return inkJSON;
        if (HasPlayedFirstTalk()) return inkJSON;
        return firstTimeInkJSON;
    }

    bool HasPlayedFirstTalk()
    {
        if (string.IsNullOrWhiteSpace(firstTalkSaveKey)) return false;
        return PlayerPrefs.GetInt(firstTalkSaveKey, 0) == 1;
    }

    void MarkFirstTalkAsPlayed()
    {
        if (firstTimeInkJSON == null) return;
        if (string.IsNullOrWhiteSpace(firstTalkSaveKey)) return;
        if (HasPlayedFirstTalk()) return;

        PlayerPrefs.SetInt(firstTalkSaveKey, 1);
        PlayerPrefs.Save();
    }

    void LateUpdate()
    {
        bool shouldShow = playerInRange
            && !OpeningLock.IsLocked
            && GameStateManager.IsNormal
            && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);

        PressEPromptCoordinator.SetRequest(pressEUI, this, shouldShow);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // If the dialogue is closed and global state allows interaction, show Press E
        if (!OpeningLock.IsLocked && GameStateManager.IsNormal && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen))
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
