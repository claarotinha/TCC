using UnityEngine;
using UnityEngine.UI;

public class ControleVolume : MonoBehaviour
{
    public AudioSource musicaMenu;
    public Slider sliderVolume;

    void Start()
    {
        sliderVolume.value = musicaMenu.volume;
    }

    public void AlterarVolume()
    {
        musicaMenu.volume = sliderVolume.value;
    }
}