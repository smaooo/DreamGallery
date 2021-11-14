using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SeaSound : MonoBehaviour
{
    // Audio clip to be played
    public AudioClip seaSound;
    // Audio source of the game object
    private AudioSource audioSource;
    
    void Start()
    {
        // Asign audio source
        audioSource = GetComponent<AudioSource>();
    }

    // Play audio clip when called
    public void PlaySound()
    {
        // Assign audio clip to the source
        audioSource.clip = seaSound;
        // Play Audio clip
        audioSource.Play();
    }

    // Load Audio clip data
    public void LoadAudio()
    {
        seaSound.LoadAudioData();
    }

    // call start coroutine when this function is called 
    public void InitiateFadeOut()
    {
        // Start Fading out coroutine
        StartCoroutine(FadeOut());
    }

    // Fade out the sound
    public IEnumerator FadeOut()
    {
        // Set timer to 0
        float timer = 0f;

        // While timer has not reached 2 seconds
        while (timer < 2f)
        {
            // Update frame
            yield return new WaitForEndOfFrame();
            // Update timer
            timer += Time.deltaTime;
            // Lerp audio source volume to 0
            audioSource.volume = Mathf.Lerp(1, 0, Mathf.Clamp01(timer));

            // if timer is done
            if (timer >= 2)
            {
                // Unload audio clip from memory
                audioSource.clip.UnloadAudioData();
                // Load Main Menu
                SceneManager.LoadScene("MainMenu");

                break;
            }
        }
    }
}
