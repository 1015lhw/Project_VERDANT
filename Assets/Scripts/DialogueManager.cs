using Ink.Parsed;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isOpen) return;

        // if there are choices, don't continue the dialogue by space
        if (story != null && story.currentChoices != null && story.currentChoices.Count > 0)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
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

        //Clear the old choices
        ClearChoices();

        if (story.canContinue)
        {
            string line = story.Continue().Trim();
            dialogueText.text = line;

            //Check if there are new choices
            DisplayChoices();
        }
        else
        {
            //if no follow up text, end the dialogue or wait for player choose sth. seriously I really HATE this...
            if (story.currentChoices != null && story.currentChoices.Count > 0)
            {
                DisplayChoices();
            }
            else
            {
                EndDialogue();
            }
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

            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null) tmp.text = choice.text;

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
        story.ChooseChoiceIndex(idx);
        ContinueStory();
    }
    public bool IsOpen => isOpen;
}
