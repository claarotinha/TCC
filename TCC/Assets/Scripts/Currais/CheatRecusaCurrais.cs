using UnityEngine;

// Remova ou desative o GameObject CHEAT da Hierarchy para voltar ao fluxo normal.
// O atalho só funciona no Editor e não grava sobre a partida real.
[DisallowMultipleComponent]
public sealed class CheatRecusaCurrais : MonoBehaviour
{
    public static bool Ativo { get; private set; }
    private void OnEnable() { if (Application.isEditor) Ativo = true; }
    private void OnDisable() { Ativo = false; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar() => Ativo = false;
}
