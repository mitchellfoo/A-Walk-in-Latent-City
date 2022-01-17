using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager S; // Singleton definition

    private AudioSource audio;

    [Header("Player Sounds")]
    public AudioSource ambientSound;

    private void Awake()
    {
        S = this; // singleton is assigned
    }

    // Start is called before the first frame update
    void Start()
    {
        // assign audio component
        audio = GetComponent<AudioSource>();
    }

    public void StopAllSounds()
    {
        // stop ambient noise
        ambientSound.Stop();

        // stop all child sounds
        foreach (Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }

    }
}