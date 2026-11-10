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

    public bool EstaAndando { get; private set; }
    public bool EstaCorrendo { get; private set; }
    public float Direcao { get; private set; } = 1f;

    private SpriteRenderer visualMari;
    private Rigidbody2D corpoMari;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (mari != null)
        {
            corpoMari = mari.GetComponent<Rigidbody2D>();
            visualMari = mari.GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        if (mari != null) return;
        Debug.LogError("SeguidorFamilia: atribua Mari no Inspector.", this);
        enabled = false;
    }

    private void LateUpdate()
    {
        if (mari == null) return;
        if (PauseHelper.BlockInput() || Time.timeScale == 0f)
        {
            EstaAndando = false;
            EstaCorrendo = false;
            AtualizarAnimacoes(false, false);
            return;
        }

        float velocidadeMari = corpoMari != null ? corpoMari.linearVelocity.x : 0f;
        bool mariMovendo = Mathf.Abs(velocidadeMari) > 0.05f;
        float direcao = visualMari != null
            ? (visualMari.flipX ? -1f : 1f) * Mathf.Sign(mari.lossyScale.x)
            : Direcao;
        if (visualMari == null && mariMovendo) direcao = Mathf.Sign(velocidadeMari);
        bool virou = direcao != Direcao;
        Direcao = direcao;
        if (mariMovendo || virou)
        {
            // Reorganiza a fila inteira no mesmo frame, sem atravessar os
            // outros familiares durante uma interpolação entre os dois lados.
            Vector3 posicao = transform.position;
            posicao.x = mari.position.x - Direcao * distanciaAtras;
            transform.position = posicao;
        }

        // A reposição ao virar não é corrida nem movimento na direção oposta.
        EstaAndando = mariMovendo;
        EstaCorrendo = mariMovendo && Mathf.Abs(velocidadeMari) > (velocidade + velocidadeCorrida) * 0.5f;
        if (spriteRenderer != null)
            spriteRenderer.flipX = Direcao * transform.lossyScale.x < 0f;
        AtualizarAnimacoes(EstaAndando, EstaCorrendo);
    }

    private void AtualizarAnimacoes(bool andando, bool correndo)
    {
        if (animator == null || !animator.enabled) return;
        if (!string.IsNullOrEmpty(parametroAndando)) animator.SetBool(parametroAndando, andando);
        if (!string.IsNullOrEmpty(parametroCorrendo)) animator.SetBool(parametroCorrendo, correndo);
    }
}
