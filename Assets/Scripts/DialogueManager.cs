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

        if (!string.IsNullOrEmpty(line))
        {
            Speaker speaker = ResolveSpeakerFromCurrentTags();
            TrySwapNpcPortraitFromCurrentTags();
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

    Speaker ResolveSpeakerFromCurrentTags()
    {
        string id = TryGetSpeakerIdFromCurrentTags();
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

    void TrySwapNpcPortraitFromCurrentTags()
    {
        if (npcPortrait == null || npcSpeakerPortraits == null)
        {
            return;
        }

        string id = TryGetSpeakerIdFromCurrentTags();
        if (string.IsNullOrEmpty(id) || id == "player" || id == "you" || id == "narration" || id == "narrator" || id == "旁白")
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
                playerBrightness = 1f;
                break;
            case Speaker.Npc:
                npcBrightness = 1f;
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
}
