using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class VolumeSettings : MonoBehaviour
{
    //to access audio mixer
    [SerializeField] private AudioMixer myMixer;
    //to access volume slider
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider SFXSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
        }
    }
    public void SetMusicVolume()
    {
        //get slider value of music playing
        float volume = musicSlider.value;
        //set volume to slider value
        myMixer.SetFloat("music", volume); //this is the exposed param set

        //to store volume at which player left it
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVolume()
    {
        //get slider value of music playing
        float volume = SFXSlider.value;
        //set volume to slider value
        myMixer.SetFloat("SFX", volume);

        //to store volume at which player left it
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        SFXSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        SetMusicVolume();
        SetSFXVolume();
    }
}
    