using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public sealed class GatilhoFortaleza : MonoBehaviour
{
    [SerializeField] private SegundaParada jornada;
    private bool solicitado;

    private void OnTriggerEnter2D(Collider2D outro) { Solicitar(outro); }
    private void OnTriggerStay2D(Collider2D outro) { Solicitar(outro); }

    private void Solicitar(Collider2D outro)
    {
        if (solicitado || jornada == null || outro.GetComponentInParent<PlayerMovement>() == null) return;
        solicitado = true;
        jornada.SolicitarEntradaFortaleza();
    }
}
