using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarFadeIn : MonoBehaviour
{

    void Start()
    {
        // Play Fade in animation at the beginning of the scene
        GetComponent<Animator>().SetTrigger("FadeIn");
    }
}
