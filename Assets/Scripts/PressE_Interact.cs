using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        string saveKey = GetEffectiveFirstTalkSaveKey();
        if (sessionResetKeys.Contains(saveKey)) return;

        PlayerPrefs.DeleteKey(saveKey);
        sessionResetKeys.Add(saveKey);
        PlayerPrefs.Save();
    }

    string GetEffectiveFirstTalkSaveKey()
    {
        if (!string.IsNullOrWhiteSpace(firstTalkSaveKey))
        {
            return firstTalkSaveKey.Trim();
        }

        string sceneName = SceneManager.GetActiveScene().name;
        string objectPath = gameObject.name;
        Transform current = transform.parent;
        while (current != null)
        {
            objectPath = current.name + "/" + objectPath;
            current = current.parent;
        }

        return $"FirstTalk::{sceneName}::{objectPath}";
    }
    TextAsset GetDialogueToPlay()
    {
        if (firstTimeInkJSON == null) return inkJSON;
        if (HasPlayedFirstTalk()) return inkJSON;
        return firstTimeInkJSON;
    }

    bool HasPlayedFirstTalk()
    {
        string saveKey = GetEffectiveFirstTalkSaveKey();
        return PlayerPrefs.GetInt(saveKey, 0) == 1;
    }

    void MarkFirstTalkAsPlayed()
    {
        if (firstTimeInkJSON == null) return;

        if (HasPlayedFirstTalk()) return;

        PlayerPrefs.SetInt(GetEffectiveFirstTalkSaveKey(), 1);
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
