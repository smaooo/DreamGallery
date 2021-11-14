using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
public class Loader : MonoBehaviour
{
    public GameObject loadingScreen; // loading screen canvas
    public GameObject loadingText; // loading text in the ui
    public LoadingUIAnimation uiAnimation; // loading text animation in loading screen
    public Animator crossFade; // cross fade animation
    private bool load = false; // to determine if load the next scene completely
    private bool showing = true; // to determine if main menu is showing and limit if statement to do once
    private bool fadeout = false; // to determine if crossfade animation should play
    public ImageSequence loadingSequence; // Image sequence animation playing in loading screen
    public AudioSource levelSource; // Main menu audio source
    private AsyncOperation asyncop; // async operation for loading the next scen
    private float timer = 0f; // timer for limiting image sequence animation

    private void Awake()
    {
        // Hide cursor
        Cursor.visible = false;
    }

    private void Start()
    {
        // Start loading the bar scene
        asyncop = SceneManager.LoadSceneAsync("Bar");
        // Don't allow the bar scene to get activated
        asyncop.allowSceneActivation = false;
        // Show the cursor in Main menu
        Cursor.visible = true;
    }

    private void Update()
    {
        // If play button is pressed
        if (load && showing)
        {
            // Don't show the main menu anympre
            showing = false;
            // Set loading screen to active
            loadingScreen.SetActive(true);
            // Hide cursor
            Cursor.visible = false;
            // Lock cursor to screen
            Cursor.lockState = CursorLockMode.Locked;
            // Call image sequence animation to play
            loadingSequence.CallPlay();
            
        }

        // if loading is in process
        if (load)
        {
            // Update timer
            timer += Time.deltaTime;
            // if timer has reached end
            if (timer >= 5f)
            {
                // Change loading text to "Press Space to Continue!!"
                loadingText.GetComponent<Text>().text = "Press Space to Continue!!";
                // Change loading text color
                loadingText.GetComponent<Text>().color = new Color(102f / 255f, 0, 0, 255f / 255f);
                // Stop previous loading text animation
                uiAnimation.playing = false;
                // Set loading to false and end loading process
                load = false;
                // Set timer to zero 
                timer = 0;
            }
        }

        // If Player has pressed space to load bar scene completely 
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // start crossfade animation
            fadeout = true;
        }

        // If crossfade animation is going to start
        if (fadeout)
        {
            // Start the cross fade animation in its animator
            crossFade.SetTrigger("start");
            // Call fadeout coroutine to fadeout main menu sound
            StartCoroutine(FadeOutAudio());
            // update timer
            timer += Time.deltaTime;
            // if timer has reached its end
            if (timer >= 2f)
            {
                // Call coroutine to set bar scene to active
                StartCoroutine(SetActive());
            }
        }
    }

    // The function that main menu class calls when player has pressed Play button
    public void LoadNextLevel(string scene)
    {
        // Set loading screen to true
        load = true;
    }
   
    // Set bar scene to active
    IEnumerator SetActive()
    {
        yield return null;
        // Allow bar scene to get activated
        asyncop.allowSceneActivation = true;
    }

    // Fade out main menu soud
    IEnumerator FadeOutAudio()
    {
        // Set timer for fading out
        float audiotimer = 0;

        // while timer has not finished 
        while (audiotimer < 2)
        {
            // Update frame
            yield return new WaitForEndOfFrame();
            // Update timer
            audiotimer += Time.deltaTime;
            // Lerp audio source volume to 0
            levelSource.volume = Mathf.Lerp(1, 0, Mathf.Clamp01(timer));
            // If timer has ended
            if (audiotimer >= 2)
            {
                break;
            }

        }
    }
}
