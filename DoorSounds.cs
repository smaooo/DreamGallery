using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSounds : MonoBehaviour
{
    public AudioClip doorOpen;
    public AudioClip doorClose;
    public AudioClip doorUnlock;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void RemoveClip()
    {
        audioSource.clip = null;
    }
    public void PlaySound(string track)
    {
        switch (track)
        {
            case "open":
                audioSource.clip = doorOpen;
                audioSource.Play();
                break;

            case "close":
                audioSource.clip = doorClose;
                audioSource.Play();
                break;

            case "unlock":
                audioSource.clip = doorUnlock;
                audioSource.Play();
                break;
        }
    }

   
}
