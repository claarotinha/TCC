
using System.Collections.Generic;
using UnityEngine;

public class SeguidorFamilia : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform mari;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Acompanhamento")]
    [SerializeField, Min(0.1f)] private float distanciaAtras = 1.5f;
    [SerializeField, Min(0.1f)] private float velocidade = 5f;
    [SerializeField, Min(0.1f)] private float velocidadeCorrida = 8f;
    [SerializeField, Min(0.01f)] private float distanciaParada = 0.08f;

    [Header("Animações")]
    [SerializeField] private string parametroAndando = "IsWalking";
    [SerializeField] private string parametroCorrendo = "IsRunning";

    public float DistanciaAtras
    {
        get => distanciaAtras;
        set => distanciaAtras = Mathf.Max(0.1f, value);
    }

    private static readonly List<Vector3> caminho = new();
    private static Transform liderAtual;
    private static Vector3 ultimaPosicao;
    private static float distanciaTotal;

    private readonly List<Vector3> pontos = new();
    private readonly List<float> distancias = new();

    private Vector3 posicaoAnterior;
    private float distanciaPercorrida;
    private bool inicializado;

    private void Start()
    {
        if (mari == null)
        {
            Debug.LogError(
                "SeguidorFamilia: atribua Mari no Inspector.",
                this
            );
            enabled = false;
            return;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        posicaoAnterior = transform.position;

        // Cada familiar mantém seu próprio histórico de posições.
        pontos.Add(transform.position);
        distancias.Add(0f);

        inicializado = true;
    }

    private void LateUpdate()
    {
        if (!inicializado || mari == null)
            return;

        if (PauseHelper.BlockInput())
        {
            AtualizarAnimacoes(false, false);
            return;
        }

        // O destino fica atrás de Mari na direção da caminhada.
        float direcao = mari.localScale.x >= 0f ? 1f : -1f;

        Rigidbody2D corpoMari = mari.GetComponent<Rigidbody2D>();

        if (corpoMari != null &&
            Mathf.Abs(corpoMari.linearVelocity.x) > 0.05f)
        {
            direcao = Mathf.Sign(corpoMari.linearVelocity.x);
        }

        Vector3 destino = mari.position -
            new Vector3(direcao * distanciaAtras, 0f, 0f);

        destino.y = transform.position.y;
        destino.z = transform.position.z;

        float distancia = Vector3.Distance(
            transform.position,
            destino
        );

        bool mariMovendo = corpoMari != null &&
            Mathf.Abs(corpoMari.linearVelocity.x) > 0.05f;

        bool mariCorrendo = corpoMari != null &&
            Mathf.Abs(corpoMari.linearVelocity.x) >
            (velocidade + velocidadeCorrida) * 0.5f;

        if (mariMovendo && distancia > distanciaParada)
        {
            float velocidadeAtual = mariCorrendo
                ? velocidadeCorrida
                : velocidade;

            transform.position = Vector3.MoveTowards(
                transform.position,
                destino,
                velocidadeAtual * Time.deltaTime
            );
        }

        float movimentoReal =
            transform.position.x - posicaoAnterior.x;

        bool andando = Mathf.Abs(movimentoReal) > 0.001f;

        if (andando && spriteRenderer != null)
            spriteRenderer.flipX = movimentoReal < 0f;

        AtualizarAnimacoes(
            andando,
            andando && mariCorrendo
        );

        posicaoAnterior = transform.position;
    }

    private void AtualizarAnimacoes(bool andando, bool correndo)
    {
        if (animator == null)
            return;

        if (!string.IsNullOrEmpty(parametroAndando))
            animator.SetBool(parametroAndando, andando);

        if (!string.IsNullOrEmpty(parametroCorrendo))
            animator.SetBool(parametroCorrendo, correndo);
    }
}
