
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GatilhoParada : MonoBehaviour
{
    [Header("Identificação")]
    [SerializeField, Range(1, 5)]
    private int numeroDaParada = 1;

    [Header("Teste")]
    [SerializeField] private bool concluirComTeclaT = false;

    [SerializeField] private PlayerMovement personagem;
    private bool iniciada;

    private void Awake()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other) => TentarIniciar(other.GetComponent<PlayerMovement>());
    private void OnTriggerStay2D(Collider2D other) => TentarIniciar(other.GetComponent<PlayerMovement>());

    private void TentarIniciar(PlayerMovement jogador)
    {
        ControleJornada controle = ControleJornada.Instance;
        if (iniciada || jogador == null || controle == null || GameSaveSystem.IsRestoring ||
            controle.ParadaAtiva || controle.ParadasConcluidas + 1 != numeroDaParada) return;
        if (controle.IniciarParada(numeroDaParada)) iniciada = true;
    }

    private void Update()
    {
        // Also handles loading inside a trigger or concluding the previous stop after crossing it.
        BoxCollider2D area = GetComponent<BoxCollider2D>();
        if (!iniciada && personagem != null && area != null &&
            personagem.transform.position.x >= area.bounds.min.x)
            TentarIniciar(personagem);

        // Apenas para testar a progressão.
        // Será desativado quando os puzzles forem implementados.
        if (!concluirComTeclaT || !iniciada)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            ControleJornada controle = ControleJornada.Instance;

            if (controle != null)
            {
                controle.ConcluirParada(numeroDaParada);
                iniciada = false;
            }
        }
    }
}
