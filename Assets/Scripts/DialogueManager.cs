using Ink.Parsed;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;
    public Image portraitImage;

    private Ink.Runtime.Story story;
    private bool isOpen;
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
        Instance = this;
    }

    void Update()
    {
        if (!isOpen) return;

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
        if (inkJSON == null)
        {
            Debug.LogError("Ink JSON is null!");
            return;
        }

        story = new Ink.Runtime.Story(inkJSON.text);
        isOpen = true;

        dialoguePanel.SetActive(true);
        portraitImage.sprite = portraitSprite;//set portrait
        ContinueStory();
    }

    public void EndDialogue()
    {
        isOpen = false;
        story = null;
        dialoguePanel.SetActive(false);
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
            dialogueText.text = line;

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
        if (choicesContainer == null) return;
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);
    }

    void DisplayChoices()
    {
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

            var label = btnObj.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.gameObject.SetActive(true);
                label.enabled = true;
                label.text = choice.text.Trim();
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
