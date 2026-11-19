using UnityEngine;
using UnityEngine.SceneManagement;

public sealed partial class AudioManager
{
    private CurraisEffectsCatalog effectsCatalog;
    private AudioSource oneShots, ambience, footsteps;
    private string ambienceName;
    private float lastItemSoundTime = -1f;
    private string lastItemSound;

    private void InitializeEffects()
    {
        effectsCatalog = Resources.Load<CurraisEffectsCatalog>("CurraisEffectsCatalog");
        oneShots = gameObject.AddComponent<AudioSource>();
        ambience = gameObject.AddComponent<AudioSource>();
        oneShots.playOnAwake = ambience.playOnAwake = false;
        oneShots.spatialBlend = ambience.spatialBlend = 0f;
        ambience.loop = true;
        footsteps = gameObject.AddComponent<AudioSource>();
        footsteps.playOnAwake = false;
        footsteps.loop = true;
        if (effectsCatalog == null)
            Debug.LogError("CURRAIS: catálogo de efeitos ausente em Assets/Resources.");
    }

    private void ChangeAmbience(Scene scene)
    {
        StopAmbience();
        SetFootsteps(false, false);
        if (scene.name == "Currais") PlayAmbience("murmuro");
        else if (scene.name == "Seca" || scene.name == "Estrada" || scene.name == "Cutscene")
            PlayAmbience("vento");
        else if (scene.name == "CaminhoParaCasa") PlayAmbience("criancaCidade");
    }

    public static void SetFootsteps(bool moving, bool running)
    {
        if (instance == null || instance.effectsCatalog == null) return;
        AudioSource source = instance.footsteps;
        if (!moving)
        {
            if (source.isPlaying) source.Stop();
            return;
        }

        AudioClip clip = running ? instance.effectsCatalog.corrida : instance.effectsCatalog.passos;
        if (clip == null || (source.isPlaying && source.clip == clip)) return;
        source.clip = clip;
        source.volume = 0.28f * AudioVolumeChannel.EffectsVolume;
        source.Play();
    }

    public static void Play(string name, float gain = 1f, AudioClip fallback = null)
    {
        if (instance == null) { Debug.LogWarning("CURRAIS AUDIO: controlador de efeitos nao inicializado: " + name); return; }
        if (instance.effectsCatalog == null) { Debug.LogWarning("CURRAIS AUDIO: catálogo ausente ao tocar: " + name); return; }
        AudioClip clip = instance.Resolve(name) ?? fallback;
        if (clip == null) { Debug.LogWarning("CURRAIS AUDIO: efeito sem AudioClip: " + name); return; }
        instance.oneShots.volume = AudioVolumeChannel.EffectsVolume;
        instance.oneShots.PlayOneShot(clip, Mathf.Clamp01(gain));
    }

    // Evita o mesmo som duplicado quando dois sistemas notificam a mesma interacao no mesmo frame.
    public static void PlayItem(string name, float gain = 0.65f)
    {
        if (instance == null || instance.effectsCatalog == null) return;
        if (instance.lastItemSound == name && Time.unscaledTime - instance.lastItemSoundTime < 0.08f) return;
        instance.lastItemSound = name;
        instance.lastItemSoundTime = Time.unscaledTime;
        Play(name, gain);
    }

    public static void PlayAmbience(string name, AudioClip fallback = null)
    {
        if (instance == null || instance.effectsCatalog == null) return;
        AudioClip clip = instance.Resolve(name) ?? fallback;
        if (clip == null || (instance.ambienceName == name && instance.ambience.isPlaying)) return;
        instance.ambienceName = name;
        instance.ambience.Stop();
        instance.ambience.clip = clip;
        instance.ambience.volume = 0.24f * AudioVolumeChannel.EffectsVolume;
        instance.ambience.Play();
    }

    public static void StopAmbience(string name = null)
    {
        if (instance == null || (name != null && instance.ambienceName != name)) return;
        instance.ambienceName = null;
        instance.ambience.Stop();
    }

    private void UpdateEffects()
    {
        oneShots.volume = AudioVolumeChannel.EffectsVolume;
        footsteps.volume = 0.28f * AudioVolumeChannel.EffectsVolume;
        if (ambience != null && ambience.isPlaying)
            ambience.volume = 0.24f * AudioVolumeChannel.EffectsVolume;
    }

    private AudioClip Resolve(string name)
    {
        switch (name)
        {
            case "arvore": return effectsCatalog.arvore;
            case "choro": return effectsCatalog.choroMenino;
            case "bola": return effectsCatalog.bola;
            case "fotografia": return effectsCatalog.fotografia;
            case "folhaVelha": return effectsCatalog.folhaVelha;
            case "caixa": return effectsCatalog.caixa;
            case "cadeado": return effectsCatalog.cliqueCadeado;
            case "senha": return effectsCatalog.erroSenha;
            case "bau": return effectsCatalog.bau;
            case "porta": return effectsCatalog.porta;
            case "galho": return effectsCatalog.galho;
            case "folhas": return effectsCatalog.folhas;
            case "passaro": return effectsCatalog.passaro;
            case "estilingue": return effectsCatalog.estilingue;
            case "faca": return effectsCatalog.faca;
            case "comer": return effectsCatalog.comer;
            case "espelho": return effectsCatalog.espelho;
            case "vento": return effectsCatalog.vento;
            case "agua": return effectsCatalog.agua;
            case "guardas": return effectsCatalog.passosGuardas;
            case "porco": return effectsCatalog.pig;
            case "porco2": return effectsCatalog.pig2;
            case "lavagem": return effectsCatalog.lavagem;
            case "tosse": return effectsCatalog.tosse;
            case "murmuro": return effectsCatalog.murmuro;
            case "bento": return effectsCatalog.bento;
            case "escrita": return effectsCatalog.escrita;
            case "clique": return effectsCatalog.clique;
            case "botao": return effectsCatalog.botao;
            case "inventarioErro": return effectsCatalog.inventarioErro;
            case "inventarioCombinar": return effectsCatalog.inventarioCombinar;
            case "voz": return effectsCatalog.voz;
            case "sino": return effectsCatalog.escolaSino;
            case "criancas": return effectsCatalog.escolaCriancas;
            case "giz": return effectsCatalog.giz;
            case "chave": return effectsCatalog.chave;
            case "abrirBau": return effectsCatalog.abrirBau;
            case "folhaNova": return effectsCatalog.folhaNova;
            case "tosseMulher": return effectsCatalog.tosseMulher;
            case "aveAmbiente": return effectsCatalog.aveAmbiente;
            case "criancaCidade": return effectsCatalog.criancaCidade;
            case "criancasBrincando": return effectsCatalog.criancasBrincando;
            case "blop": return effectsCatalog.blop;
            case "erroCombinacao": return effectsCatalog.erroCombinacao;
            case "aguaEstrada": return effectsCatalog.aguaEstrada;
            case "arvoreEstrada": return effectsCatalog.arvoreEstrada;
            default: return null;
        }
    }
}
