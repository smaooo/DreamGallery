using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingSound : MonoBehaviour
{

    // Create an array of audio clips
    public AudioClip[] tracks;
    // Define audio source
    private AudioSource source;
    public bool sourceIsPlaying;

    void Start()
    {
        sourceIsPlaying = false;
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {   
        if (source.isPlaying == false)
        {
            sourceIsPlaying = false;
        }
    }

    public void PlaySound()
    {
        source.clip = tracks[Random.Range(0, tracks.Length - 1)];
        source.Play();
        sourceIsPlaying = true;
        
    }
}
