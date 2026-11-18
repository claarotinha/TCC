using UnityEngine;
using UnityEngine.UI;

public class ControleVolume : MonoBehaviour
{
    public AudioSource musicaMenu;
    public Slider sliderVolume;

    void Start()
    {
        sliderVolume.SetValueWithoutNotify(AudioVolumeChannel.MusicVolume);
    }

    public void AlterarVolume()
    {
        AudioVolumeChannel.SetMusicVolume(sliderVolume.value);
    }
}