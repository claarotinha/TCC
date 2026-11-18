using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class AudioVolumeChannel : MonoBehaviour
{
    public enum Channel { Music, Effects }
    [SerializeField] private Channel channel = Channel.Effects;
    private AudioSource source;
    private float authoredVolume;
    private static event Action SettingsChanged;
    private bool ready, dirty;

    public static float MusicVolume => Mathf.Clamp01(PlayerPrefs.GetFloat("MusicVolume", 1f));
    public static float EffectsVolume => Mathf.Clamp01(PlayerPrefs.GetFloat("EffectsVolume", 1f));

    public static void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", Mathf.Clamp01(value));
        SettingsChanged?.Invoke();
    }

    public static void SetEffectsVolume(float value)
    {
        PlayerPrefs.SetFloat("EffectsVolume", Mathf.Clamp01(value));
        SettingsChanged?.Invoke();
    }

    private void Start()
    {
        source = GetComponent<AudioSource>();
        authoredVolume = source.volume;
        ready = true;
        ApplyVolume();
    }

    private void OnEnable()
    {
        SettingsChanged += RequestVolumeUpdate;
        dirty = true;
    }

    private void OnDisable() => SettingsChanged -= RequestVolumeUpdate;

    private void RequestVolumeUpdate() => dirty = true;

    private void Update()
    {
        if (ready && dirty) ApplyVolume();
    }

    private void ApplyVolume()
    {
        dirty = false;
        if (source != null)
            source.volume = authoredVolume * (channel == Channel.Music ? MusicVolume : EffectsVolume);
    }
}
