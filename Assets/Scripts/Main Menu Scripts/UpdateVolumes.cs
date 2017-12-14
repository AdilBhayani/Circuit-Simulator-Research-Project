using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateVolumes : MonoBehaviour {
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider efxSlider;
    private string masterVolumeTitle = "MasterVolume";
    private string musicVolumeTitle = "MusicVolume";
    private string efxVolumeTitle = "EfxVolume";

    // Use this for initialization
    void Start () {
        masterSlider.value = PlayerPrefs.GetFloat(masterVolumeTitle, 1.0f);
        musicSlider.value = PlayerPrefs.GetFloat(musicVolumeTitle, 1.0f);
        efxSlider.value = PlayerPrefs.GetFloat(efxVolumeTitle, 1.0f);
    }
	
	// Update is called once per frame
	void Update () {
        PlayerPrefs.SetFloat(masterVolumeTitle, masterSlider.value);
        PlayerPrefs.SetFloat(musicVolumeTitle, musicSlider.value);
        PlayerPrefs.SetFloat(efxVolumeTitle, efxSlider.value);
    }
}
