using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button exitButton;

    void Start()
    {
        // Setup button click sounds if they have ButtonHoverAnimation
        if (newGameButton != null)
        {
            var hoverAnim = newGameButton.GetComponent<ButtonHoverAnimation>();
            if (hoverAnim != null)
            {
                newGameButton.onClick.AddListener(hoverAnim.PlayClickSound);
            }
        }

        if (exitButton != null)
        {
            var hoverAnim = exitButton.GetComponent<ButtonHoverAnimation>();
            if (hoverAnim != null)
            {
                exitButton.onClick.AddListener(hoverAnim.PlayClickSound);
            }
        }
    }

    // Метод для кнопки "Новая игра"я
    public void StartNewGame()
    {
        Debug.Log("Starting new game...");
        SceneManager.LoadScene("SampleScene"); // укажи точное имя сцены
        // или SceneManager.LoadScene(1); если по индексу
    }

    // Метод для кнопки "Выход"
    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
        // В редакторе Unity это не сработает, но в билде — да
    }
}
