using UnityEngine;
using TMPro;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private Story story;
    private bool isOpen;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isOpen) return;

       
        if (Input.GetKeyDown(KeyCode.Space)) // Press "Space" to continue
        {
            ContinueStory();
        }
    }

    public void StartDialogue(TextAsset inkJSON)
    {
        if (inkJSON == null)
        {
            Debug.LogError("Ink JSON is null!");
            return;
        }

        story = new Story(inkJSON.text);
        isOpen = true;

        dialoguePanel.SetActive(true);
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

        if (story.canContinue)
        {
            string line = story.Continue().Trim();
            dialogueText.text = line;
        }
        else
        {
            EndDialogue();
        }
    }

    public bool IsOpen => isOpen;
}
