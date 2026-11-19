#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class CurraisEffectsCatalogBuilder
{
    private const string Path = "Assets/Resources/CurraisEffectsCatalog.asset";
    static CurraisEffectsCatalogBuilder() => EditorApplication.delayCall += Build;

    [MenuItem("CURRAIS/Audio/Atualizar efeitos")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        var catalog = AssetDatabase.LoadAssetAtPath<CurraisEffectsCatalog>(Path);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<CurraisEffectsCatalog>();
            AssetDatabase.CreateAsset(catalog, Path);
        }
        const string s = "Assets/Audio/efeito sonoro/";
        catalog.arvore = Get(s+"arvore_clique.wav");
        catalog.choroMenino = Get(s+"menino chorando.wav");
        catalog.bola = Get(s+"coleta bola.wav");
        catalog.fotografia = Get(s+"fotografia.wav");
        catalog.folhaVelha = Get(s+"folha_velha_passando.wav");
        catalog.caixa = Get(s+"examine caixa.wav");
        catalog.cliqueCadeado = Get(s+"clique_cadeado_currais.wav");
        catalog.erroSenha = Get(s+"erro senha.wav");
        catalog.bau = Get(s+"bau destrancando.wav");
        catalog.porta = Get(s+"casa manha - quartinho/porta abrindo.wav");
        catalog.galho = Get(s+"pega galho.wav");
        catalog.folhas = Get(s+"reclhe folhas e plantas.wav");
        catalog.passaro = Get(s+"passaro atingido.wav");
        catalog.estilingue = Get(s+"estrada/estilingue.wav");
        catalog.faca = Get(s+"estrada/faca.wav");
        catalog.comer = Get(s+"ruido comendo.wav");
        catalog.espelho = Get(s+"ruido espelho.wav");
        catalog.vento = Get(s+"vento.wav");
        catalog.agua = Get(s+"agua despejada.wav");
        catalog.passos = Get(s+"movimentaçao/walk.wav");
        catalog.corrida = Get(s+"movimentaçao/run.wav");
        catalog.passosGuardas = Get(s+"passos guardas.wav");
        catalog.pig = Get(s+"currais/pig.mp3");
        catalog.pig2 = Get(s+"currais/pig2.wav");
        catalog.lavagem = Get(s+"lavagem de roupa.wav");
        catalog.tosse = Get(s+"tosse retirantes.wav");
        catalog.murmuro = Get(s+"murmurio patio.wav");
        catalog.bento = Get(s+"bento caindo.mp3");
        catalog.escrita = Get(s+"raimundo escrevendo.mp3");
        catalog.clique = Get(s+"interface/click.wav");
        catalog.botao = Get(s+"interface/button.wav");
        catalog.inventarioErro = Get(s+"interface/inventory error.wav");
        catalog.inventarioCombinar = Get(s+"interface/inventory combine.wav");
        catalog.voz = Get(s+"voizinha.wav");
        catalog.escolaSino = Get(s+"sino.wav");
        catalog.escolaCriancas = Get(s+"criancas_escola_fade.wav");
        catalog.giz = Get("Assets/Audio/398491__anthousai__writing-chalk-center-01.wav");
        catalog.chave = Get("Assets/Audio/efeito sonoro/casa manha - quartinho/chave.wav");
        catalog.abrirBau = Get("Assets/Audio/efeito sonoro/casa manha - quartinho/abrir bau.wav");
        catalog.folhaNova = Get("Assets/Audio/efeito sonoro/folha passando.wav");
        catalog.tosseMulher = Get("Assets/Audio/efeito sonoro/tosse mulher retirante.wav");
        catalog.aveAmbiente = Get("Assets/Audio/efeito sonoro/estrada/bird.wav");
        catalog.criancaCidade = Get("Assets/Audio/efeito sonoro/criança cidade.wav");
        catalog.criancasBrincando = Get("Assets/Audio/efeito sonoro/crianças brincando.mp3");
        catalog.blop = Get("Assets/Audio/efeito sonoro/interface/blop.mp3");
        catalog.erroCombinacao = Get("Assets/Audio/efeito sonoro/interface/erro combinaçao.wav");
        catalog.aguaEstrada = Get("Assets/Audio/efeito sonoro/estrada/agua.aiff");
        catalog.arvoreEstrada = Get("Assets/Audio/efeito sonoro/arvore.mp3");
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
    }

    private static AudioClip Get(string path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) Debug.LogWarning("CURRAIS: efeito ausente: " + path);
        return clip;
    }
}
#endif
