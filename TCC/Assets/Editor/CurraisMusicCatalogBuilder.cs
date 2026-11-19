#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

/// <summary>Associa os WAV ja existentes ao catalogo, sem mover os arquivos ou editar cenas.</summary>
[InitializeOnLoad]
public static class CurraisMusicCatalogBuilder
{
    private const string AssetPath = "Assets/Resources/CurraisMusicCatalog.asset";
    private const string MusicPath = "Assets/Audio/Trilha sonora/";
    private static bool building;

    static CurraisMusicCatalogBuilder() => EditorApplication.delayCall += Build;

    [MenuItem("CURRAIS/Audio/Atualizar musicas")]
    public static void Build()
    {
        if (building || EditorApplication.isPlayingOrWillChangePlaymode) return;
        building = true;
        try
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            var catalog = AssetDatabase.LoadAssetAtPath<CurraisMusicCatalog>(AssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<CurraisMusicCatalog>();
                AssetDatabase.CreateAsset(catalog, AssetPath);
            }
            bool changed = false;
            changed |= Assign(ref catalog.introducao, "introduçao.wav");
            changed |= Assign(ref catalog.menu, "menu.wav");
            changed |= Assign(ref catalog.antesA, "antes A.wav");
            changed |= Assign(ref catalog.antesB, "antes B.wav");
            changed |= Assign(ref catalog.seca, "secaa.wav");
            changed |= Assign(ref catalog.estrada, "caminhada.wav");
            changed |= Assign(ref catalog.currais, "currais.wav");
            changed |= Assign(ref catalog.fogueira, "musica fogueira loop.wav");
            changed |= Assign(ref catalog.captura, "vertical captura loop.wav");
            changed |= Assign(ref catalog.funeral, "funeral loop.wav");
            changed |= Assign(ref catalog.negacao, "vertical_mariquinha_loop.wav");
            changed |= Assign(ref catalog.finalCarta, "final carta.wav");
            changed |= Assign(ref catalog.finalRuim, "final ruim.wav");
            if (changed)
            {
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
            }
        }
        finally { building = false; }
    }

    private static bool Assign(ref AudioClip field, string filename)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicPath + filename);
        if (clip == null) Debug.LogWarning("CURRAIS: trilha nao encontrada: " + MusicPath + filename);
        if (field == clip) return false;
        field = clip;
        return true;
    }
}
#endif
