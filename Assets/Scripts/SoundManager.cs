using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public AudioSource efxSource;                   //Drag a reference to the audio source which will play the sound effects.
    public AudioSource musicSource;                 //Drag a reference to the audio source which will play the music.
    public static SoundManager instance = null;     //Allows other scripts to call functions from SoundManager.     
    public float lowPitchRange = .95f;              //The lowest a sound effect will be randomly pitched.
    public float highPitchRange = 1.05f;            //The highest a sound effect will be randomly pitched.
    private float masterVolume;              //The volume overall for both efx and music.
    private float musicVolume;               //The music volume.
    private float efxVolume;                 //The efx volume.
    private string masterVolumeTitle = "MasterVolume";
    private string MusicVolumeTitle = "MusicVolume";
    private string EfxVolumeTitle = "EfxVolume";

    void Awake()
    {
        //Check if there is already an instance of SoundManager
        if (instance == null)
        {
            Debug.Log("Awake called on first load only");
            PlayerPrefs.SetFloat(masterVolumeTitle, 1.0f);
            PlayerPrefs.SetFloat(MusicVolumeTitle, 1.0f);
            PlayerPrefs.SetFloat(EfxVolumeTitle, 1.0f);
            masterVolume = PlayerPrefs.GetFloat(masterVolumeTitle);
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeTitle);
            efxVolume = PlayerPrefs.GetFloat(EfxVolumeTitle);
            //if not, set it to this.
            instance = this;
        }
        //If instance already exists:
        else if (instance != this) {
            Debug.Log("Destroying repeated instance");
            //Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
            Destroy(gameObject);
        }
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

    //Method updates volume every frame
    private void Update()
    {
        masterVolume = PlayerPrefs.GetFloat(masterVolumeTitle);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeTitle);
        efxVolume = PlayerPrefs.GetFloat(EfxVolumeTitle);
        musicSource.volume = masterVolume * musicVolume;
        efxSource.volume = masterVolume * efxVolume;
    }

}