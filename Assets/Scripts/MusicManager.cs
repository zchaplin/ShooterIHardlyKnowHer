using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager AudioManager;
    public AudioSource source;
    public AudioSource bgSource;

    public AudioClip backgroundClip;
    public AudioClip[] deathGoops;
     void Awake()
    {
        if (AudioManager == null)
            AudioManager = this;
        else
            Destroy(AudioManager);
    }
    // Start is called before the first frame update
    void Start()
    {
        startBackgroundMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startBackgroundMusic() {
        bgSource.clip = backgroundClip;
        bgSource.Play();
    }

    public void goopMusic() {
        if (deathGoops.Length > 0)
        {
            int randomIndex = Random.Range(0, deathGoops.Length);
            source.clip  = deathGoops[randomIndex];
            source.Play();
        }
    }
}
