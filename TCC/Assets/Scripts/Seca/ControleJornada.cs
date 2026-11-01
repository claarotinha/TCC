
using UnityEngine;

public class ControleJornada : MonoBehaviour
{
    public static ControleJornada Instance { get; private set; }

    [Header("Progresso")]
    [SerializeField, Range(0, 5)]
    private int paradasConcluidas = 0;

    [Header("Referências")]
    [SerializeField] private Transform mari;
    [SerializeField] private Transform[] limitesDasParadas = new Transform[5];

    private bool paradaAtiva;
    private int paradaAtual = -1;

    public int ParadasConcluidas => paradasConcluidas;
    public bool ParadaAtiva => paradaAtiva;
    public int ParadaAtual => paradaAtual;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "Já existe outro ControleJornada na cena.",
                this
            );
            enabled = false;
            return;
        }

        Instance = this;
    }

    private void LateUpdate()
    {
        if (mari == null || paradasConcluidas >= 5)
            return;

        Transform limite = limitesDasParadas[paradasConcluidas];

        if (limite == null)
            return;

        // Bloqueia somente o avanço para a direita.
        if (mari.position.x > limite.position.x)
        {
            Vector3 posicao = mari.position;
            posicao.x = limite.position.x;
            mari.position = posicao;

            Rigidbody2D corpo = mari.GetComponent<Rigidbody2D>();

            if (corpo != null && corpo.linearVelocity.x > 0f)
            {
                corpo.linearVelocity = new Vector2(
                    0f,
                    corpo.linearVelocity.y
                );
            }
        }
    }

    public bool IniciarParada(int numero)
    {
        if (paradaAtiva || numero != paradasConcluidas + 1)
            return false;

        paradaAtiva = true;
        paradaAtual = numero;

        Debug.Log("Parada iniciada: " + numero);
        return true;
    }

    public void ConcluirParada(int numero)
    {
        if (!paradaAtiva || paradaAtual != numero)
        {
            Debug.LogWarning(
                "Não é possível concluir a parada " + numero
            );
            return;
        }

        paradasConcluidas = numero;
        paradaAtiva = false;
        paradaAtual = -1;

        Debug.Log("Parada concluída: " + numero);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
