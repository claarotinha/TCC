using UnityEngine;

// Notas de edição presentes na Hierarchy. Não controla a história,
// não cria colliders e não altera posições, animações ou salvamentos.
[DisallowMultipleComponent]
[AddComponentMenu("Currais/Área do cenário")]
public sealed class AreaCurrais : MonoBehaviour
{
    [Header("Área e acontecimentos")]
    public string titulo;
    [TextArea(3, 8)] public string acontecimentos;

    [Header("Arte e produção")]
    [TextArea(3, 8)] public string artesExistentes;
    [TextArea(3, 10)] public string variantesPendentes;
    [TextArea(2, 6)] public string camadas;

    [Header("Referências editáveis da cena")]
    public Transform[] marcasRelacionadas;
    public GameObject[] estadosRelacionados;
}
