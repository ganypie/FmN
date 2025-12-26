using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public ScreenFader screenFader;

    public void StartNewGame()
    {
        screenFader.FadeToScene("SampleScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
