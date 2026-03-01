using Ink.Parsed;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;
    public Image portraitImage;

    [Header("Portrait")]
    public Image npcPortrait;
    [Range(0f, 1f)]
    public float dimBrightness = 0.5f;
    private void SetPortraitBrightness(float brightness)
    {
        if (portraitImage == null)
        {
            Debug.Log("[Portrait] portraitImage NULL");
            return;
        }

        Debug.Log($"[Portrait] set brightness={brightness}", portraitImage);

        portraitImage.color = new Color(brightness, brightness, brightness, 1f);
    }

    private Ink.Runtime.Story story;
    private bool isOpen;
    private bool isForcedDialogue;
    private bool allowEscSkipForcedDialogue;
    private Action onDialogueClosed;
    private int storyId = 0;
    private int storyIdCounter = 0;
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
        Debug.Log($"[Awake] DialogueManager on {gameObject.name}");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Awake] Duplicate DialogueManager destroyed: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("DialogueManager Awake: " + gameObject.name);
    }

    void Update()
    {
        if (!isOpen) return;

        if (isForcedDialogue && allowEscSkipForcedDialogue && Input.GetKeyDown(KeyCode.Escape))
        {
            EndDialogue();
            return;
        }

        //if there are choices, dont continue
        if (story != null && story.currentChoices != null && story.currentChoices.Count > 0)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            //only when you click on choices button, it won't continue. Finally fixed I hate this stupid coding stuff
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

        Debug.Log("[Start] " + inkJSON.name);
        Debug.Log($"[StartDialogue] prefabNull={(choiceButtonPrefab == null)} prefab={(choiceButtonPrefab ? choiceButtonPrefab.name : "NULL")}  DM={gameObject.name}");
        if (isOpen)
        {
            EndDialogue();
        }

        if (inkJSON == null)
        {
            Debug.LogError("Ink JSON is null!");

            return;
        }

        story = new Ink.Runtime.Story(inkJSON.text);
        storyId = ++storyIdCounter;
        Debug.Log($"[StartDialogue] storyId={storyId} ink={inkJSON.name}");

        isOpen = true;
        isForcedDialogue = forced;
        allowEscSkipForcedDialogue = forced && allowEscSkip;
        onDialogueClosed = onClosed;

        dialoguePanel.SetActive(true);
        portraitImage.sprite = portraitSprite;

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

        Debug.Log("Choices count = " + story.currentChoices.Count);

        //if there are choices, display
        if (story.currentChoices != null && story.currentChoices.Count > 0)
        {
            DisplayChoices();
            return;
        }

        ClearChoices();

        if (story.canContinue)
        {
            string line = story.Continue().Trim();
            Debug.Log("[After Continue] choices=" + story.currentChoices.Count);
            dialogueText.text = line;
            if (!string.IsNullOrEmpty(line))
                SetPortraitBrightness(1f);

            //Choices check
            if (story.currentChoices != null && story.currentChoices.Count > 0)
                DisplayChoices();
        }
        else
        {
            EndDialogue();
        }
    }

    void ClearChoices()
    {
        Debug.Log($"[ClearChoices] container={choicesContainer?.name} childCount={choicesContainer?.childCount}");
        if (choicesContainer == null) return;
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);
    }

    void DisplayChoices()
    {
        SetPortraitBrightness(dimBrightness);

        Debug.Log($"[DisplayChoices] storyChoices={story.currentChoices.Count} container={choicesContainer?.name} childCountBefore={choicesContainer?.childCount}");
        Debug.Log($"[DisplayChoices] NULLCHECK storyNull={(story == null)} containerNull={(choicesContainer == null)} prefabNull={(choiceButtonPrefab == null)}");
        ClearChoices();
        Debug.Log("[DisplayChoices] choices=" + story.currentChoices.Count);

        if (story == null || choicesContainer == null || choiceButtonPrefab == null) return;

        foreach (var choice in story.currentChoices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
            Debug.Log("[DisplayChoices] SPAWNED: " + choice.text);
            Debug.Log($"[DisplayChoices] spawned button for: {choice.text}");

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
        Debug.Log($"[OnChoiceSelected] prefabNull={(choiceButtonPrefab == null)} prefab={(choiceButtonPrefab ? choiceButtonPrefab.name : "NULL")}  DM={gameObject.name}");
        ClearChoices();
        story.ChooseChoiceIndex(idx);
        ContinueStory();
    }
    public bool IsOpen => isOpen;
}
