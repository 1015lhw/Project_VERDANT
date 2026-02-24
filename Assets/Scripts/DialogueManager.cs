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
    public RectTransform choicesContainer;
    public GameObject choiceButtonPrefab;

    [Header("Debug / UI Safety")]
    public float choiceButtonMinHeight = 60f; // botton height
    public bool forceShowPanelWhenDialogueStarts = true;

    private Story story;
    private bool isOpen;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Update()
    {
        if (!isOpen) return;

        //if there are choices, banned sapce wait for the player to click
        if (story != null && story.currentChoices != null && story.currentChoices.Count > 0)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
            ContinueStory();
    }

    public void StartDialogue(TextAsset inkJSON)
    {
        if (inkJSON == null)
        {
            Debug.LogError("Ink JSON is null!");
            return;
        }

        if (forceShowPanelWhenDialogueStarts && dialoguePanel != null)
            dialoguePanel.SetActive(true);

        story = new Story(inkJSON.text);
        isOpen = true;

        Debug.Log($"StartDialogue: panel activeInHierarchy = {dialoguePanel?.activeInHierarchy}");

        ContinueStory();
    }

    public void EndDialogue()
    {
        Debug.Log("EndDialogue called.");
        isOpen = false;
        story = null;

        ClearChoices();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void ContinueStory()
    {
        if (story == null) return;

        Debug.Log("Choices count (before Continue) = " + story.currentChoices.Count);

        ClearChoices();

        if (story.canContinue)
        {
            string line = story.Continue().Trim();
            if (dialogueText != null) dialogueText.text = line;

            DisplayChoices(); //Continue->Choices
        }
        else
        {
            if (story.currentChoices != null && story.currentChoices.Count > 0)
                DisplayChoices();
            else
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
        if (story == null)
        {
            Debug.LogError("DisplayChoices: story is null");
            return;
        }
        if (choicesContainer == null)
        {
            Debug.LogError("DisplayChoices: choicesContainer is null");
            return;
        }
        if (choiceButtonPrefab == null)
        {
            Debug.LogError("DisplayChoices: choiceButtonPrefab is null");
            return;
        }

        Debug.Log("CHOICES COUNT (DisplayChoices): " + story.currentChoices.Count);
        Debug.Log($"choicesContainer rect = {choicesContainer.rect}, childCount(before)={choicesContainer.childCount}");

        foreach (var choice in story.currentChoices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);

            // 1) ∏¸Œ»£∫Button/TMP ”√ InChildren
            var btn = btnObj.GetComponentInChildren<Button>(true);
            var tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);

            if (tmp != null) tmp.text = choice.text;
            else Debug.LogWarning("Choice prefab has no TextMeshProUGUI in children.");

            if (btn == null)
            {
                Debug.LogWarning("Choice prefab has no Button in children.");
                continue;
            }

            //avoid the layout stuff make it 0
            var rt = btn.GetComponent<RectTransform>();
            if (rt != null)
            {
                if (rt.sizeDelta.y < choiceButtonMinHeight)
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x, choiceButtonMinHeight);

                //protect the button from the scale stuff
                rt.localScale = Vector3.one;

                Debug.Log($"Spawned choice '{choice.text}' rt.anchoredPos={rt.anchoredPosition} sizeDelta={rt.sizeDelta}");
            }

            int idx = choice.index;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnChoiceSelected(idx));
        }

        Debug.Log($"childCount(after)={choicesContainer.childCount}");
    }

    void OnChoiceSelected(int idx)
    {
        if (story == null) return;
        story.ChooseChoiceIndex(idx);
        ContinueStory();
    }

    public bool IsOpen => isOpen;
}