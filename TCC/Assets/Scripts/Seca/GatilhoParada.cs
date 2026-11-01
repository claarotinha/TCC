
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class GatilhoParada : MonoBehaviour
{
    [Header("Identificação")]
    [SerializeField, Range(1, 5)]
    private int numeroDaParada = 1;

    [Header("Teste")]
    [SerializeField] private bool concluirComTeclaT = false;

    private bool iniciada;

    private void Awake()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Confere o objeto com PlayerMovement.
        if (other.GetComponent<PlayerMovement>() == null)
            return;

        ControleJornada controle = ControleJornada.Instance;

        if (controle == null)
            return;

        if (controle.IniciarParada(numeroDaParada))
        {
            iniciada = true;
            Debug.Log(
                "Objetivo da parada " + numeroDaParada +
                " iniciado."
            );
        }
    }

    private void Update()
    {
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
