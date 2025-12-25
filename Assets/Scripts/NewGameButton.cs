using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButton : MonoBehaviour
{
    public string firstGameScene;

    public void StartNewGame()
    {
        GameState.StartedNewGame = true;
        SceneManager.LoadScene(firstGameScene);
    }
}
