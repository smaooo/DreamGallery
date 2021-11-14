using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class ImageSequence : MonoBehaviour
{

    public Texture[] sequence; // Array for image sequence
    private RawImage image; // Raw image object in the scene
    public bool play = false; // to determine if image sequence should be playing or not
    void Start()
    {
        // assign raw image to its component
        image = GetComponent<RawImage>();
        // set the raw image to firts frame of image sequence
        image.texture = sequence[1];
    }

    // To call coroutine resoponsible for playing image sequence
    public void CallPlay()
    {
        // Play image sequence
        StartCoroutine(PlaySequence());
    }

    // Play image sequence animation
    IEnumerator PlaySequence()
    {
        yield return null;
        // Set timer for each frame
        float timer = 0;
        // Set current index of image sequence to 0
        int index = 0;
        
        // while timer hasn't reached 1/24 second (animation frame rate should be 24 fps)
        while (timer < 0.04f)
        {
            // Update the frame
            yield return new WaitForEndOfFrame();
            // Update the timer
            timer += Time.deltaTime;
            // if a frame for animation has passed
            if (timer >= 0.04f)
            {
                // update image sequence animation to next frame
                image.texture = sequence[index];
                // increment image sequence index
                index++;
                // set timer 0 for next frame
                timer = 0;
            }

            // if image sequence animation has reached its end 
            if (index == sequence.Length)
            {
                // Reset the animation and set the index to 0
                index = 0;
            }
        }
    }
}
