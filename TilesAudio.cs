using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesAudio : MonoBehaviour
{

    public AudioClip pickTrack;
    public AudioClip putTrack;
    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlaySound(string act)
    {
        switch (act)
        {
            case "pick":
                source.clip = pickTrack;
                break;

            case "put":
                source.clip = putTrack;
                break;

            default:
                break;
        }
        source.Play();
    }
}
