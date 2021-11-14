using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // To restart the level
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // To load the Main Menu
    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
