using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSimple : MonoBehaviour
{
    public void LoadPlaytest()
    {
        SceneManager.LoadScene("Playtest Scene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}