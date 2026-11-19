using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Currais/Efeitos", fileName = "CurraisEffectsCatalog")]
public sealed class CurraisEffectsCatalog : ScriptableObject
{
    public AudioClip arvore, choroMenino, bola, fotografia, folhaVelha, caixa, cliqueCadeado, erroSenha, bau;
    public AudioClip porta, galho, folhas, passaro, estilingue, faca, comer, espelho, vento, agua;
    public AudioClip passos, corrida, passosGuardas, pig, pig2, lavagem, tosse, murmuro, bento, escrita;
    public AudioClip clique, botao, inventarioErro, inventarioCombinar, voz;
    public AudioClip escolaSino, escolaCriancas;
}

public sealed class CurraisEffectsDirector : MonoBehaviour
{
    private static CurraisEffectsDirector instance;
    private CurraisEffectsCatalog catalog;
    private AudioSource oneShots, ambience;
    private string ambienceName;
    private float lastItemSoundTime = -1f;
    private string lastItemSound;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null) return;
        GameObject root = new GameObject("Efeitos CURRAIS - persistente");
        DontDestroyOnLoad(root);
        instance = root.AddComponent<CurraisEffectsDirector>();
    }

    private void Awake()
    {
        if (instance != this) { Destroy(gameObject); return; }
        catalog = Resources.Load<CurraisEffectsCatalog>("CurraisEffectsCatalog");
        oneShots = gameObject.AddComponent<AudioSource>();
        ambience = gameObject.AddComponent<AudioSource>();
        oneShots.playOnAwake = ambience.playOnAwake = false;
        oneShots.spatialBlend = ambience.spatialBlend = 0f;
        ambience.loop = true;
        SceneManager.sceneLoaded += SceneChanged;
    }

    private void OnDestroy()
    {
        if (instance == this) { SceneManager.sceneLoaded -= SceneChanged; instance = null; }
    }

    private void SceneChanged(Scene scene, LoadSceneMode mode)
    {
        StopAmbience();
        if (catalog == null) catalog = Resources.Load<CurraisEffectsCatalog>("CurraisEffectsCatalog");
        if (scene.name == "Currais") PlayAmbience("murmuro");
        else if (scene.name == "Seca" || scene.name == "Estrada") PlayAmbience("vento");
    }

    public static void Play(string name, float gain = 1f)
    {
        if (instance == null || instance.catalog == null) return;
        AudioClip clip = instance.Resolve(name);
        if (clip != null) instance.oneShots.PlayOneShot(clip, Mathf.Clamp01(gain) * AudioVolumeChannel.EffectsVolume);
    }

    // Evita o mesmo som duplicado quando dois sistemas notificam a mesma interacao no mesmo frame.
    public static void PlayItem(string name, float gain = 0.65f)
    {
        if (instance == null || instance.catalog == null) return;
        if (instance.lastItemSound == name && Time.unscaledTime - instance.lastItemSoundTime < 0.08f) return;
        instance.lastItemSound = name;
        instance.lastItemSoundTime = Time.unscaledTime;
        Play(name, gain);
    }

    public static void PlayAmbience(string name)
    {
        if (instance == null || instance.catalog == null) return;
        AudioClip clip = instance.Resolve(name);
        if (clip == null || (instance.ambienceName == name && instance.ambience.isPlaying)) return;
        instance.ambienceName = name;
        instance.ambience.Stop();
        instance.ambience.clip = clip;
        instance.ambience.volume = 0.24f * AudioVolumeChannel.EffectsVolume;
        instance.ambience.Play();
    }

    public static void StopAmbience()
    {
        if (instance == null) return;
        instance.ambienceName = null;
        instance.ambience.Stop();
    }

    private void Update()
    {
        if (ambience != null && ambience.isPlaying)
            ambience.volume = 0.24f * AudioVolumeChannel.EffectsVolume;
    }

    private AudioClip Resolve(string name)
    {
        switch (name)
        {
            case "arvore": return catalog.arvore;
            case "choro": return catalog.choroMenino;
            case "bola": return catalog.bola;
            case "fotografia": return catalog.fotografia;
            case "folha": return catalog.folhaVelha;
            case "caixa": return catalog.caixa;
            case "cadeado": return catalog.cliqueCadeado;
            case "senha": return catalog.erroSenha;
            case "bau": return catalog.bau;
            case "porta": return catalog.porta;
            case "galho": return catalog.galho;
            case "folhas": return catalog.folhas;
            case "passaro": return catalog.passaro;
            case "estilingue": return catalog.estilingue;
            case "faca": return catalog.faca;
            case "comer": return catalog.comer;
            case "espelho": return catalog.espelho;
            case "vento": return catalog.vento;
            case "agua": return catalog.agua;
            case "passos": return catalog.passos;
            case "corrida": return catalog.corrida;
            case "guardas": return catalog.passosGuardas;
            case "porco": return catalog.pig;
            case "porco2": return catalog.pig2;
            case "lavagem": return catalog.lavagem;
            case "tosse": return catalog.tosse;
            case "murmuro": return catalog.murmuro;
            case "bento": return catalog.bento;
            case "escrita": return catalog.escrita;
            case "clique": return catalog.clique;
            case "botao": return catalog.botao;
            case "inventarioErro": return catalog.inventarioErro;
            case "inventarioCombinar": return catalog.inventarioCombinar;
            case "voz": return catalog.voz;
            case "sino": return catalog.escolaSino;
            case "criancas": return catalog.escolaCriancas;
            default: return null;
        }
    }
}
