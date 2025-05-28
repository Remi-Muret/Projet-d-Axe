using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";

    void Start()
    {
        float musicVolume = PlayerPrefs.HasKey(MusicVolumeKey) ? PlayerPrefs.GetFloat(MusicVolumeKey) : 0.5f;
        float sfxVolume = PlayerPrefs.HasKey(SFXVolumeKey) ? PlayerPrefs.GetFloat(SFXVolumeKey) : 0.5f;

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);

        if (musicSlider != null) musicSlider.value = musicVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;
    }

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(SFXVolumeKey, value);
        PlayerPrefs.Save();
    }

}
