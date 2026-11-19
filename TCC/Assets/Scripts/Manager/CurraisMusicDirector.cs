using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Trilhas persistentes e momentos narrativos, sem duplicar AudioSources entre cenas.</summary>
public sealed class CurraisMusicDirector : MonoBehaviour
{
    private static CurraisMusicDirector instance;
    private CurraisMusicCatalog catalog;
    private AudioSource background, moment;
    private Coroutine transition;
    private string activeScene;
    private string activeMoment;
    private float backgroundGain = 0.65f, momentGain = 0.8f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        var root = new GameObject("Musicas CURRAIS - persistente");
        DontDestroyOnLoad(root);
        instance = root.AddComponent<CurraisMusicDirector>();
    }

    private void Awake()
    {
        if (instance != this) { Destroy(gameObject); return; }
        catalog = Resources.Load<CurraisMusicCatalog>("CurraisMusicCatalog");
        background = gameObject.AddComponent<AudioSource>();
        moment = gameObject.AddComponent<AudioSource>();
        foreach (var source in new[] { background, moment })
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = false;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (catalog == null)
            Debug.LogWarning("CURRAIS: catalogo de musicas ausente. Abra o projeto no Editor para gerar Assets/Resources/CurraisMusicCatalog.asset.");
    }

    private void OnDestroy()
    {
        if (instance == this) { SceneManager.sceneLoaded -= OnSceneLoaded; instance = null; }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        activeScene = scene.name;
        activeMoment = null;
        moment.Stop();
        moment.clip = null;
        if (catalog == null) catalog = Resources.Load<CurraisMusicCatalog>("CurraisMusicCatalog");
        if (catalog == null) return;
        AudioClip desired = SceneTrack(scene.name);
        if (desired == background.clip && background.isPlaying) return; // ANTES B atravessa as duas casas sem reiniciar.
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(ChangeBackground(desired));
    }

    private AudioClip SceneTrack(string scene)
    {
        switch (scene)
        {
            case "Intro": case "Splash": case "AvisoInicial": case "Cutscene": return catalog.introducao;
            case "Escola": return null; // O sinal e as vozes da escola sao a paisagem sonora desta cena.
            case "MenuPrincipal": return catalog.menu;
            case "CaminhoParaCasa": case "Casa_Manha": return catalog.antesB;
            case "Quartinho": return catalog.antesA;
            case "Seca": return catalog.seca;
            case "Estrada": return catalog.estrada;
            case "Currais": return catalog.currais;
            case "epilogo": return null; // O final é decidido pelo EpilogoSceneController.
            default: return null;
        }
    }

    private IEnumerator ChangeBackground(AudioClip next)
    {
        float initial = background.volume;
        for (float t = 0; t < 0.5f; t += Time.unscaledDeltaTime)
        {
            background.volume = Mathf.Lerp(initial, 0f, t / 0.5f);
            yield return null;
        }
        background.Stop();
        background.clip = next;
        if (next != null) background.Play();
        for (float t = 0; t < 0.7f; t += Time.unscaledDeltaTime)
        {
            background.volume = Mathf.Lerp(0f, backgroundGain * AudioVolumeChannel.MusicVolume, t / 0.7f);
            yield return null;
        }
        background.volume = backgroundGain * AudioVolumeChannel.MusicVolume;
        transition = null;
    }

    private void Update()
    {
        if (background != null && transition == null)
            background.volume = backgroundGain * AudioVolumeChannel.MusicVolume * (moment.isPlaying ? 0.22f : 1f);
        if (moment != null && moment.isPlaying)
            moment.volume = momentGain * AudioVolumeChannel.MusicVolume;
    }

    public static void PlayMoment(string name)
    {
        if (instance == null || instance.catalog == null) return;
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
        if (instance == null || instance.catalog == null || instance.activeScene != "epilogo") return;
        instance.BeginMoment(carta ? "finalCarta" : "finalRuim");
    }

    private void BeginMoment(string name)
    {
        AudioClip clip = null;
        switch (name)
        {
            case "fogueira": clip = catalog.fogueira; break;
            case "captura": clip = catalog.captura; break;
            case "funeral": clip = catalog.funeral; break;
            case "negacao": clip = catalog.negacao; break;
            case "finalCarta": clip = catalog.finalCarta; break;
            case "finalRuim": clip = catalog.finalRuim; break;
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
