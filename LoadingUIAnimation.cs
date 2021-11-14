using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUIAnimation : MonoBehaviour
{
    private Transform[] dots; // dots in front of the word loading
    private float timer; // timer for dots animation
    private float animTime = 1f; // limit of the timer
    private int arrIndex = -1; // index of the dots arrat
    public bool playing = true; // should the dots animation be palying
    
    void Start()
    {
        // Set timer to its limit
        timer = animTime;
        // assign dots to the array
        dots = new Transform[3] { transform.GetChild(1),
                                  transform.GetChild(2),
                                  transform.GetChild(3)};
    }

    void Update()
    {
        // inverse update the timer
        timer -= Time.deltaTime;
        // if the last dot has be shown
        if (arrIndex == dots.Length && timer < 0.1f && playing == true)
        {
            // hide all dots
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].gameObject.SetActive(false);
            }
            // reset dots array
            arrIndex = -1;
        }

        // if timer for one dot is finished
        if (timer < 0.1f && playing == true)
        {
            // if index is on the first dot
            if (arrIndex != -1)
            {
                // show that dot on screen
                dots[arrIndex].gameObject.SetActive(true);
            }
            // increment dots array index
            arrIndex++;
            // reset animation timer
            timer = animTime;
        }

        // if animation should stop
        if (playing == false)
        {
            // Hide all dots from screen
            for (int i = 0; i < dots.Length; i++)
            {
                dots[i].gameObject.SetActive(false);
            }
        }

    }
}
