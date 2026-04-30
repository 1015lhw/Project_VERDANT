using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System;

public class DialogueManager : MonoBehaviour
{
    // NOTE: Keep helper method definitions single-source in this file to avoid merge duplicate compile errors (CS0111).
    public static DialogueManager Instance;

    [Serializable]
    public class SpeakerPortraitBinding
    {
        [Tooltip("说话者 ID（不带前缀），例如 sierra / marcus。会匹配 Ink tag: # speaker:sierra")]
        public string speakerId;
        public Sprite portrait;
    }

    [Serializable]
    public class TagPortraitBinding
    {
        [Tooltip("完整 Ink tag（不带#），例如 SierraMAD / MarcusQUIET / TylerTALKBLOOD")]
        public string tag;
        public Sprite portrait;
    }

    enum Speaker
    {
        Npc,
        Player,
        Narration
    }

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    [Header("Portrait")]
    [FormerlySerializedAs("portraitImage")]
    [Tooltip("左侧 NPC 立绘。老场景中的 portraitImage 引用会自动迁移到这里。")]
    public Image npcPortrait;
    [Tooltip("右侧主角立绘（固定）。")]
    public Image playerPortrait;
    [Tooltip("右侧主角固定立绘。")]
    public Sprite playerPortraitSprite;
    [Tooltip("可选：按 Ink speaker 标签切换 NPC 立绘。")]
    public List<SpeakerPortraitBinding> npcSpeakerPortraits = new List<SpeakerPortraitBinding>();
    [Tooltip("可选：按完整 Ink tag 切换立绘（优先级高于 speaker）。")]
    public List<TagPortraitBinding> tagPortraits = new List<TagPortraitBinding>();
    [Range(0f, 1f)]
    public float dimBrightness = 0.5f;

    private Story story;
    private bool isOpen;
    private bool isForcedDialogue;
    private bool allowEscSkipForcedDialogue;
    private Action onDialogueClosed;

    bool ClickedOnChoiceButton()
    {
        if (EventSystem.current == null) return false;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (var r in results)
        {
            if (choicesContainer != null && r.gameObject.transform.IsChildOf(choicesContainer))
                return true;
        }

        return false;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        if (!isOpen) return;

        if (isForcedDialogue && allowEscSkipForcedDialogue && Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
            return;
        }

        if (story != null && story.currentChoices != null && story.currentChoices.Count > 0)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (ClickedOnChoiceButton())
                return;

            ContinueStory();
        }
    }

    public void StartDialogue(TextAsset inkJSON, Sprite portraitSprite)
    {
        if (OpeningLock.IsLocked)
        {
            return;
        }

        StartDialogueInternal(inkJSON, portraitSprite, false, false, null);
    }

    public void StartForcedDialogue(TextAsset inkJSON, Sprite portraitSprite, bool allowEscSkip, Action onClosed = null)
    {
        StartDialogueInternal(inkJSON, portraitSprite, true, allowEscSkip, onClosed);
    }

    void StartDialogueInternal(TextAsset inkJSON, Sprite portraitSprite, bool forced, bool allowEscSkip, Action onClosed)
    {
        if (inkJSON == null)
        {
            Debug.LogError("Ink JSON is null!");
            return;
        }

        if (isOpen)
        {
            EndDialogue();
        }

        story = new Story(inkJSON.text);
        try
        {
            PushEngineStateIntoStory(story);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DialogueManager] PushEngineStateIntoStory failed: {e.Message}");
        }

        isOpen = true;
        isForcedDialogue = forced;
        allowEscSkipForcedDialogue = forced && allowEscSkip;
        onDialogueClosed = onClosed;

        dialoguePanel.SetActive(true);

        if (npcPortrait != null)
        {
            npcPortrait.sprite = portraitSprite;
            npcPortrait.gameObject.SetActive(npcPortrait.sprite != null);
        }

        if (playerPortrait != null)
        {
            if (playerPortraitSprite != null)
            {
                playerPortrait.sprite = playerPortraitSprite;
            }

            playerPortrait.gameObject.SetActive(playerPortrait.sprite != null);
        }

        ApplySpeakerState(Speaker.Npc);

        ClearChoices();
        dialogueText.text = "";

        ContinueStory();
    }

    public void EndDialogue()
    {
        Action closedCallback = onDialogueClosed;

        isOpen = false;
        isForcedDialogue = false;
        allowEscSkipForcedDialogue = false;
        onDialogueClosed = null;
        story = null;
        ClearChoices();
        dialoguePanel.SetActive(false);

        closedCallback?.Invoke();
    }

    void ContinueStory()
    {
        if (story == null) return;

        if (story.currentChoices != null && story.currentChoices.Count > 0)
        {
            DisplayChoices();
            return;
        }

        ClearChoices();

        if (!story.canContinue)
        {
            EndDialogue();
            return;
        }

        string line = story.Continue().Trim();
        dialogueText.text = line;

        try
        {
            ProcessActionTags(story.currentTags);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DialogueManager] ProcessActionTags failed: {e.Message}");
        }

        if (!string.IsNullOrEmpty(line))
        {
            Speaker speaker = ResolveSpeakerFromCurrentTags();
            TrySwapPortraitFromCurrentTags(speaker);
            ApplySpeakerState(speaker);
        }

        if (story.currentChoices != null && story.currentChoices.Count > 0)
        {
            DisplayChoices();
        }
    }

    string TryGetSpeakerIdFromCurrentTags()
    {
        if (story == null || story.currentTags == null)
        {
            return null;
        }

        for (int i = 0; i < story.currentTags.Count; i++)
        {
            string tag = story.currentTags[i];
            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            string normalizedTag = tag.Trim().ToLowerInvariant();
            if (normalizedTag.StartsWith("#"))
            {
                normalizedTag = normalizedTag.Substring(1).Trim();
            }

            string[] separators = new[] {":", "："};
            for (int s = 0; s < separators.Length; s++)
            {
                string sep = separators[s];
                string prefix = "speaker" + sep;
                if (!normalizedTag.StartsWith(prefix))
                {
                    continue;
                }

                string id = normalizedTag.Substring(prefix.Length).Trim();
                return string.IsNullOrEmpty(id) ? null : id;
            }
        }

        return null;
    }

    string TryGetRawTagFromCurrentTags()
    {
        if (story == null || story.currentTags == null)
        {
            return null;
        }

        for (int i = 0; i < story.currentTags.Count; i++)
        {
            string tag = story.currentTags[i];
            if (string.IsNullOrWhiteSpace(tag))
            {
                continue;
            }

            string normalized = tag.Trim();
            if (normalized.StartsWith("#"))
            {
                normalized = normalized.Substring(1).Trim();
            }

            if (!string.IsNullOrEmpty(normalized))
            {
                return normalized;
            }
        }

        return null;
    }

    Speaker ResolveSpeakerFromCurrentTags()
    {
        string id = TryGetSpeakerIdFromCurrentTags();
        if (string.IsNullOrEmpty(id))
        {
            string rawTag = TryGetRawTagFromCurrentTags();
            if (!string.IsNullOrEmpty(rawTag))
            {
                string lower = rawTag.ToLowerInvariant();
                if (lower.StartsWith("tyler") || lower.StartsWith("player") || lower.StartsWith("you"))
                {
                    return Speaker.Player;
                }
                if (lower.StartsWith("narration") || lower.StartsWith("narrator") || lower.StartsWith("旁白"))
                {
                    return Speaker.Narration;
                }
                return Speaker.Npc;
            }
        }

        if (string.IsNullOrEmpty(id))
        {
            return Speaker.Npc;
        }

        if (id == "player" || id == "you")
        {
            return Speaker.Player;
        }
        if (id == "narration" || id == "narrator" || id == "旁白")
        {
            return Speaker.Narration;
        }

        return Speaker.Npc;
    }

    void TrySwapPortraitFromCurrentTags(Speaker speaker)
    {
        if (npcSpeakerPortraits == null)
        {
            return;
        }

        string rawTag = TryGetRawTagFromCurrentTags();
        if (!string.IsNullOrEmpty(rawTag) && tagPortraits != null)
        {
            string lowerRaw = rawTag.ToLowerInvariant();
            for (int i = 0; i < tagPortraits.Count; i++)
            {
                TagPortraitBinding binding = tagPortraits[i];
                if (binding == null || binding.portrait == null || string.IsNullOrWhiteSpace(binding.tag))
                {
                    continue;
                }

                if (binding.tag.Trim().ToLowerInvariant() != lowerRaw)
                {
                    continue;
                }

                if (speaker == Speaker.Player && playerPortrait != null)
                {
                    playerPortrait.sprite = binding.portrait;
                    playerPortrait.gameObject.SetActive(true);
                }
                else if (speaker == Speaker.Npc && npcPortrait != null)
                {
                    npcPortrait.sprite = binding.portrait;
                    npcPortrait.gameObject.SetActive(true);
                }

                return;
            }
        }

        string id = TryGetSpeakerIdFromCurrentTags();
        if (string.IsNullOrEmpty(id) || id == "player" || id == "you" || id == "narration" || id == "narrator" || id == "旁白")
        {
            return;
        }

        if (npcPortrait == null)
        {
            return;
        }

        for (int i = 0; i < npcSpeakerPortraits.Count; i++)
        {
            SpeakerPortraitBinding binding = npcSpeakerPortraits[i];
            if (binding == null || binding.portrait == null || string.IsNullOrWhiteSpace(binding.speakerId))
            {
                continue;
            }

            if (binding.speakerId.Trim().ToLowerInvariant() == id)
            {
                npcPortrait.sprite = binding.portrait;
                npcPortrait.gameObject.SetActive(true);
                return;
            }
        }
    }

    void ApplySpeakerState(Speaker speaker)
    {
        float npcBrightness = dimBrightness;
        float playerBrightness = dimBrightness;

        switch (speaker)
        {
            case Speaker.Player:
                npcBrightness = 1f;
                break;
            case Speaker.Npc:
                playerBrightness = 1f;
                break;
            case Speaker.Narration:
                break;
        }

        SetPortraitBrightness(npcPortrait, npcBrightness);
        SetPortraitBrightness(playerPortrait, playerBrightness);
    }

    void SetPortraitBrightness(Image portrait, float brightness)
    {
        if (portrait == null)
        {
            return;
        }

        portrait.color = new Color(brightness, brightness, brightness, 1f);
    }

    void ClearChoices()
    {
        if (choicesContainer == null) return;
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);
    }

    void DisplayChoices()
    {
        ApplySpeakerState(Speaker.Player);

        ClearChoices();

        if (story == null || choicesContainer == null || choiceButtonPrefab == null) return;

        foreach (var choice in story.currentChoices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);

            if (!btnObj.activeSelf) btnObj.SetActive(true);

            var img = btnObj.GetComponent<Image>();
            if (img != null) img.enabled = true;

            var btn = btnObj.GetComponent<Button>();
            if (btn != null) btn.enabled = true;

            var layout = btnObj.GetComponent<LayoutElement>();
            if (layout != null) layout.enabled = true;

            var label = btnObj.transform.Find("Text (TMP)")?.GetComponent<TMP_Text>();
            if (label != null)
            {
                label.gameObject.SetActive(true);
                label.enabled = true;
                label.enableAutoSizing = false;
                label.fontSize = 32;
                label.text = choice.text.Trim();
                label.ForceMeshUpdate();
            }

            if (btn != null)
            {
                int idx = choice.index;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnChoiceSelected(idx));
            }
        }
    }

    void OnChoiceSelected(int idx)
    {
        ClearChoices();
        story.ChooseChoiceIndex(idx);
        ContinueStory();
    }

    public bool IsOpen => isOpen;

    // ===== Engine <-> Ink bridge =====
    // Push current engine state (reputation, inventory, story flags) into Ink variables
    // so that .ink dialogue files can branch on real game state instead of hard-coded defaults.
    void PushEngineStateIntoStory(Story s)
    {
        if (s == null) return;

        if (ReputationManager.Instance != null)
        {
            TrySetInkVar(s, "Srep", ReputationManager.Instance.GetAffection(NpcId.Sie));
            TrySetInkVar(s, "Mrep", ReputationManager.Instance.GetAffection(NpcId.Mar));
        }

        var inv = InventorySystem.Instance;
        TrySetInkVar(s, "map",      inv != null && inv.Has("Map")      ? 1 : 0);
        TrySetInkVar(s, "mushroom", inv != null && inv.Has("Mushroom") ? 1 : 0);
        TrySetInkVar(s, "compass",  inv != null && inv.Has("Compass")  ? 1 : 0);
        // blanket is a starter item — Ink defaults to 1, do not overwrite from inventory.
        // gaveBlanketToSierra / gaveBlanketToMarcus flags below handle the "given once" gate.

        TrySetInkVar(s, "gaveMapToSierra",     StoryFlags.Get("gaveMapToSierra")     ? 1 : 0);
        TrySetInkVar(s, "gaveBlanketToSierra", StoryFlags.Get("gaveBlanketToSierra") ? 1 : 0);
        TrySetInkVar(s, "gaveBlanketToMarcus", StoryFlags.Get("gaveBlanketToMarcus") ? 1 : 0);
        TrySetInkVar(s, "MarcHasMush",         StoryFlags.Get("MarcHasMush")         ? 1 : 0);
        TrySetInkVar(s, "gotWolfRepellent",    StoryFlags.Get("gotWolfRepellent")    ? 1 : 0);

        Debug.Log($"[DialogueBridge] Srep={(ReputationManager.Instance != null ? ReputationManager.Instance.GetAffection(NpcId.Sie) : 0)} " +
                  $"Mrep={(ReputationManager.Instance != null ? ReputationManager.Instance.GetAffection(NpcId.Mar) : 0)} " +
                  $"map={(inv != null && inv.Has("Map") ? 1 : 0)} " +
                  $"mushroom={(inv != null && inv.Has("Mushroom") ? 1 : 0)} " +
                  $"compass={(inv != null && inv.Has("Compass") ? 1 : 0)} " +
                  $"gaveMapToSierra={(StoryFlags.Get("gaveMapToSierra") ? 1 : 0)} " +
                  $"gaveBlanketToSierra={(StoryFlags.Get("gaveBlanketToSierra") ? 1 : 0)} " +
                  $"gaveBlanketToMarcus={(StoryFlags.Get("gaveBlanketToMarcus") ? 1 : 0)} " +
                  $"MarcHasMush={(StoryFlags.Get("MarcHasMush") ? 1 : 0)} " +
                  $"gotWolfRepellent={(StoryFlags.Get("gotWolfRepellent") ? 1 : 0)}");
    }

    static void TrySetInkVar(Story s, string name, int value)
    {
        try
        {
            if (s.variablesState.GlobalVariableExistsWithName(name))
            {
                s.variablesState[name] = value;
            }
        }
        catch
        {
            // Different .ink files declare different variable sets; silently ignore mismatches.
        }
    }

    void ProcessActionTags(IList<string> tags)
    {
        if (tags == null) return;

        for (int i = 0; i < tags.Count; i++)
        {
            string raw = tags[i];
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string t = raw.Trim();
            if (t.StartsWith("#")) t = t.Substring(1).Trim();

            string lower = t.ToLowerInvariant();

            if (lower.StartsWith("grant:"))
                HandleGrantTag(t.Substring("grant:".Length).Trim());
            else if (lower.StartsWith("consume:"))
                HandleConsumeTag(t.Substring("consume:".Length).Trim());
            else if (lower.StartsWith("rep:"))
                HandleRepTag(t.Substring("rep:".Length).Trim());
            else if (lower.StartsWith("flag:"))
                HandleFlagTag(t.Substring("flag:".Length).Trim());
            // Other tags (SierraHAPPY, MarcusQUIET, speaker:..., etc.) are handled by portrait/speaker logic.
        }
    }

    void HandleGrantTag(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return;
        if (InventorySystem.Instance == null)
        {
            Debug.LogWarning($"[DialogueManager] grant:{itemId} ignored — InventorySystem.Instance is null.");
            return;
        }
        if (InventorySystem.Instance.Has(itemId)) return;

        int configuredAmount = DynamicInventoryUI.GetConfiguredAmountById(itemId);
        if (configuredAmount > 0)
            InventorySystem.Instance.AddTaskReward(itemId, configuredAmount);
        else
            InventorySystem.Instance.AddTaskReward(itemId);
    }

    void HandleConsumeTag(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId)) return;
        if (InventorySystem.Instance == null) return;
        if (!InventorySystem.Instance.Has(itemId)) return;
        InventorySystem.Instance.Remove(itemId, 1);
    }

    // Tag form: "Sie:+15", "Mar:-3", "sie:5"
    void HandleRepTag(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return;
        if (ReputationManager.Instance == null)
        {
            Debug.LogWarning($"[DialogueManager] rep:{body} ignored — ReputationManager.Instance is null.");
            return;
        }

        int sep = body.IndexOf(':');
        if (sep <= 0 || sep >= body.Length - 1)
        {
            Debug.LogWarning($"[DialogueManager] Malformed rep tag: rep:{body}");
            return;
        }

        string npcKey = body.Substring(0, sep).Trim().ToLowerInvariant();
        string deltaStr = body.Substring(sep + 1).Trim();

        NpcId npc;
        if (npcKey == "sie" || npcKey == "sierra") npc = NpcId.Sie;
        else if (npcKey == "mar" || npcKey == "marcus") npc = NpcId.Mar;
        else
        {
            Debug.LogWarning($"[DialogueManager] Unknown NPC in rep tag: rep:{body}");
            return;
        }

        if (deltaStr.StartsWith("+")) deltaStr = deltaStr.Substring(1);

        if (!int.TryParse(deltaStr, out int delta))
        {
            Debug.LogWarning($"[DialogueManager] Non-integer delta in rep tag: rep:{body}");
            return;
        }

        ReputationManager.Instance.AddAffection(npc, delta, "Ink");
    }

    // Tag form: "gaveMapToSierra:set", "gaveMapToSierra:clear"
    void HandleFlagTag(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return;

        int sep = body.IndexOf(':');
        if (sep <= 0 || sep >= body.Length - 1)
        {
            Debug.LogWarning($"[DialogueManager] Malformed flag tag: flag:{body}");
            return;
        }

        string flagName = body.Substring(0, sep).Trim();
        string action = body.Substring(sep + 1).Trim().ToLowerInvariant();

        if (action == "set" || action == "true" || action == "1")
            StoryFlags.Set(flagName, true);
        else if (action == "clear" || action == "false" || action == "0" || action == "unset")
            StoryFlags.Set(flagName, false);
        else
            Debug.LogWarning($"[DialogueManager] Unknown flag action: flag:{body}");
    }
}
