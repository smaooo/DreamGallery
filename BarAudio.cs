using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarAudio : MonoBehaviour
{
    // Create an array of audio clips
    public AudioClip[] tracks;
    // Define audio source
    private AudioSource source;
    // Index value for the clips arrat
    private int index = 0;
    // Current playing clip length
    private float currentClipLength;
    // Timer for keeping track of current playing clip
    private float timer;

    void Start()
    {
        source = GetComponent<AudioSource>();
        PlayNextClip();
        
    }

    // Update is called once per frame
    void Update()
    {
      
            // Update the timer
            timer += Time.deltaTime;

            // Check if the timer has passed the current clip duration
            if (timer > currentClipLength)
            {
                // Check if there's still tacks remaning
                if (index + 1 == tracks.Length)
                {
                    // Go to the first clip of the playlist
                    index = 0;
                }
                else
                {
                    index++;
                }
                PlayNextClip();
            }

        
    }
    private void PlayNextClip()
    {
        // Stop the active clip
        source.Stop();
        // Set the next clip as active clip
        source.clip = tracks[index];
        // Set the current clip length
        currentClipLength = tracks[index].length;
        // Reset the timer
        timer = 0f;
        // Play the current clip
        source.Play();

    } 
}
