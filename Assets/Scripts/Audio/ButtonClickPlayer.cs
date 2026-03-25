using UnityEngine;

public class ButtonClickPlayer : MonoBehaviour
{
    string clickEventName = "Play_Click_Event";



    public void PlayClick()
    {
        if (clickEventName != null)
        {
            AkUnitySoundEngine.PostEvent(clickEventName, gameObject);
            Debug.Log("Click audio played");
        }
        else
        {
            Debug.LogError("clickEvent is null");
        }
    }
}