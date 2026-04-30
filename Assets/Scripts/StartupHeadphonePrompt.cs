using UnityEngine;

public class StartupHeadphonePrompt : MonoBehaviour
{
    [Header("Prompt UI")]
    public GameObject promptPanel;

    [Header("Timing")]
    public float showTime = 3f;

    void Start()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
            Invoke(nameof(HidePrompt), showTime);
        }
    }

    void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
}