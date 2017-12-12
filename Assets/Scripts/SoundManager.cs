using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public AudioSource efxSource;                   //Drag a reference to the audio source which will play the sound effects.
    public AudioSource musicSource;                 //Drag a reference to the audio source which will play the music.
    public static SoundManager instance = null;     //Allows other scripts to call functions from SoundManager.     
    public float lowPitchRange = .95f;              //The lowest a sound effect will be randomly pitched.
    public float highPitchRange = 1.05f;            //The highest a sound effect will be randomly pitched.
    private float masterVolume = 1.0f;              //The volume overall for both efx and music.
    private float musicVolume = 1.0f;               //The music volume.
    private float efxVolume = 1.0f;                 //The efx volume.

   
    void Awake()
    {
        //Check if there is already an instance of SoundManager
        if (instance == null)
            //if not, set it to this.
            instance = this;
        //If instance already exists:
        else if (instance != this)
            //Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
            Destroy(gameObject);

        //Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
        DontDestroyOnLoad(gameObject);
    }


    //Used to play single sound clips.
    public void PlaySingle(AudioClip clip)
    {
        //Set the clip of our efxSource audio source to the clip passed in as a parameter.
        efxSource.clip = clip;

        //Choose a random pitch to play back our clip at between our high and low pitch ranges.
        float randomPitch = Random.Range(lowPitchRange, highPitchRange);

        //Set the pitch of the audio source to the randomly chosen pitch.
        efxSource.pitch = randomPitch;

        //Play the clip.
        efxSource.Play();
    }

    //Method called by slider to control Overall Volume
    public void ChangeMasterVolume(float volume)
    {
        masterVolume = volume;
        UpdateVolumes();
    }

    //Method called by slider to control Music Volume
    public void ChangeMusicVolume(float volume)
    {
        musicVolume = volume;
        UpdateVolumes();
    }

    //Method called by slider to control effects volume
    public void ChangeEfxVolume(float volume)
    {
        efxVolume = volume;
        UpdateVolumes();
    }

    //Method internally called to update output volume
    private void UpdateVolumes()
    {
        musicSource.volume = masterVolume * musicVolume;
        efxSource.volume = masterVolume * efxVolume;
    }

}