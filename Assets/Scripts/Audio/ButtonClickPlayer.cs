using UnityEngine;

public class ButtonClickPlayer : MonoBehaviour
{
    public AK.Wwise.Event clickEvent;
    public AK.Wwise.Bank sfxBank;

    void Start()
    {
        if (sfxBank != null)
        {
            sfxBank.Load();
            Debug.Log("SFX bank loaded");
        }
        else
        {
            Debug.LogError("sfxBank is null");
        }
    }

    public void PlayClick()
    {
        if (clickEvent != null)
        {
            clickEvent.Post(gameObject);
        }
        else
        {
            Debug.LogError("clickEvent is null");
        }
    }
}