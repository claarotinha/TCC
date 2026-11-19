using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Trilhas persistentes e momentos narrativos, sem duplicar AudioSources entre cenas.</summary>
public sealed partial class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance == null)
            new GameObject(nameof(AudioManager)).AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("MasterVolume", 1f));
        InitializeMusic();
        InitializeEffects();
        radio = gameObject.AddComponent<AudioSource>();
        radio.playOnAwake = false;
        radio.loop = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        instance = null;
    }

    private void Update()
    {
        UpdateMusic();
        UpdateEffects();
    }

    private CurraisMusicCatalog musicCatalog;
    private AudioSource background, moment, radio;

    public static bool RadioPlaying => instance != null && instance.radio.isPlaying;

    public static void SetRadio(bool enabled, AudioClip clip = null)
    {
        if (instance == null) return;
        AudioSource source = instance.radio;
        if (!enabled)
        {
            source.Stop();
            source.clip = null;
        }
        else if (clip != null && (!source.isPlaying || source.clip != clip))
        {
            source.clip = clip;
            source.volume = 0.65f * AudioVolumeChannel.MusicVolume;
            source.Play();
        }
    }
    private Coroutine transition;
    private AudioClip nextTrack;
    private string activeScene;
    private string activeMoment;
    private float backgroundGain = 0.65f, momentGain = 0.8f;

    private void InitializeMusic()
    {
        musicCatalog = Resources.Load<CurraisMusicCatalog>("CurraisMusicCatalog");
        background = gameObject.AddComponent<AudioSource>();
        moment = gameObject.AddComponent<AudioSource>();
        foreach (var source in new[] { background, moment })
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = false;
        }
        if (musicCatalog == null)
            Debug.LogWarning("CURRAIS: catálogo de músicas ausente em Assets/Resources.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        ChangeAmbience(scene);
        radio.Stop();
        radio.clip = null;
        activeScene = scene.name;
        activeMoment = null;
        moment.Stop();
        moment.clip = null;
        if (musicCatalog == null) return;
        AudioClip desired = SceneTrack(scene.name);
        if (transition != null && desired == nextTrack) return;
        if (transition == null && desired == background.clip) return;
        if (transition != null) StopCoroutine(transition);
        nextTrack = desired;
        transition = StartCoroutine(ChangeBackground(desired));
    }

    private AudioClip SceneTrack(string scene)
    {
        switch (scene)
        {
            case "Intro": case "Splash": case "AvisoInicial": case "Cutscene": return musicCatalog.introducao;
            case "Escola": return null; // O sinal e as vozes da escola sao a paisagem sonora desta cena.
            case "MenuPrincipal": return musicCatalog.menu;
            case "CaminhoParaCasa": case "Casa_Manha": return musicCatalog.antesB;
            case "Quartinho": return musicCatalog.antesA;
            case "Seca": return musicCatalog.seca;
            case "Estrada": return musicCatalog.estrada;
            case "Currais": return musicCatalog.currais;
            case "epilogo": return null; // O final é decidido pelo EpilogoSceneController.
            default: return null;
        }
    }

    private IEnumerator ChangeBackground(AudioClip next)
    {
        float initial = background.volume;
        for (float t = 0; background.isPlaying && t < 0.5f; t += Time.unscaledDeltaTime)
        {
            background.volume = Mathf.Lerp(initial, 0f, t / 0.5f);
            yield return null;
        }
        background.Stop();
        background.clip = next;
        background.volume = 0f;
        if (next != null) background.Play();
        for (float t = 0; t < 0.7f; t += Time.unscaledDeltaTime)
        {
            background.volume = Mathf.Lerp(0f, backgroundGain * AudioVolumeChannel.MusicVolume, t / 0.7f);
            yield return null;
        }
        background.volume = backgroundGain * AudioVolumeChannel.MusicVolume;
        transition = null;
    }

    private void UpdateMusic()
    {
        radio.volume = 0.65f * AudioVolumeChannel.MusicVolume;
        if (background != null && transition == null)
            background.volume = backgroundGain * AudioVolumeChannel.MusicVolume * (moment.isPlaying || RadioPlaying ? 0.22f : 1f);
        if (moment != null && moment.isPlaying)
            moment.volume = momentGain * AudioVolumeChannel.MusicVolume;
    }

    public static void PlayMoment(string name)
    {
        if (instance == null || instance.musicCatalog == null) return;
        instance.BeginMoment(name);
    }

    public static void StopMoment(string name)
    {
        if (instance == null || instance.activeMoment != name) return;
        instance.activeMoment = null;
        instance.moment.Stop();
        instance.moment.clip = null;
    }

    public static void PlayEnding(bool carta)
    {
        if (instance == null || instance.musicCatalog == null || instance.activeScene != "epilogo") return;
        instance.BeginMoment(carta ? "finalCarta" : "finalRuim");
    }

    private void BeginMoment(string name)
    {
        AudioClip clip = null;
        switch (name)
        {
            case "fogueira": clip = musicCatalog.fogueira; break;
            case "captura": clip = musicCatalog.captura; break;
            case "funeral": clip = musicCatalog.funeral; break;
            case "negacao": clip = musicCatalog.negacao; break;
            case "finalCarta": clip = musicCatalog.finalCarta; break;
            case "finalRuim": clip = musicCatalog.finalRuim; break;
        }
        if (clip == null) { Debug.LogWarning("CURRAIS: musica nao configurada: " + name); return; }
        if (activeMoment == name && moment.isPlaying) return;
        activeMoment = name;
        moment.Stop();
        moment.clip = clip;
        moment.volume = momentGain * AudioVolumeChannel.MusicVolume;
        moment.Play();
    }
}
