using UnityEngine;
using Ink.Runtime;

public class InkConsoleTest : MonoBehaviour
{
    public TextAsset inkJSON;

    void Start()
    {
        Story story = new Story(inkJSON.text);
        Debug.Log(story.Continue());
    }
}

