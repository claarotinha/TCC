using UnityEngine;

[CreateAssetMenu(menuName = "Currais/Musicas", fileName = "CurraisMusicCatalog")]
public sealed class CurraisMusicCatalog : ScriptableObject
{
    public AudioClip introducao, menu, antesA, antesB, seca, estrada, currais;
    public AudioClip fogueira, captura, funeral, negacao, finalCarta, finalRuim;
}

