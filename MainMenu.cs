using UnityEngine;
public class MainMenu : MonoBehaviour
{

    public Loader loader;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    // Pressing Play button in main menu call this function
    public void StartGame()
    {
        // Call load next level method from loader
        loader.LoadNextLevel("Bar");
    }
}
