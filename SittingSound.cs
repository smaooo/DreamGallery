using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SittingSound : MonoBehaviour
{
    public AudioClip sitTrack;
    public AudioClip standTrack;
    private AudioSource source;
    public bool soundIsPlaying;

    private void Awake()
    {
        soundIsPlaying = false;
        source = GetComponent<AudioSource>();
    }
    void Start()
    {
        
        
    }

    private void Update()
    {
        if (source.isPlaying == false)
        {
            soundIsPlaying = false;
        }
    }
    public void PlaySound(string act)
    {
        switch (act)
        {
            case "sit":
                source.clip = sitTrack;
                break;

            case "stand":
                source.clip = standTrack;
                break;

            default:
                break;
        }
        soundIsPlaying = true;
        source.Play();
        
    }
}
