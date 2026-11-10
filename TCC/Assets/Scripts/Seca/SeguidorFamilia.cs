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

    private Rigidbody2D corpoMari;
    private Vector3 posicaoAnterior;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (mari != null) corpoMari = mari.GetComponent<Rigidbody2D>();
    }

    private void OnEnable() => posicaoAnterior = transform.position;

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
            AtualizarAnimacoes(false, false);
            posicaoAnterior = transform.position;
            return;
        }

        float velocidadeMari = corpoMari != null ? corpoMari.linearVelocity.x : 0f;
        bool mariMovendo = Mathf.Abs(velocidadeMari) > 0.05f;
        bool correndo = Mathf.Abs(velocidadeMari) > (velocidade + velocidadeCorrida) * 0.5f;
        if (mariMovendo)
        {
            // A formação tem ordem fixa no mundo. Virar não espelha os destinos
            // nem faz os familiares cruzarem uns pelos outros para trocar de lado.
            Vector3 posicao = transform.position;
            posicao.x = mari.position.x - distanciaAtras;
            transform.position = posicao;
        }

        float movimento = transform.position.x - posicaoAnterior.x;
        bool andando = Mathf.Abs(movimento) > 0.001f;
        if (spriteRenderer != null && mariMovendo)
            spriteRenderer.flipX = velocidadeMari * transform.lossyScale.x < 0f;
        AtualizarAnimacoes(andando, andando && correndo);
        posicaoAnterior = transform.position;
    }

    private void AtualizarAnimacoes(bool andando, bool correndo)
    {
        if (animator == null || !animator.enabled) return;
        if (!string.IsNullOrEmpty(parametroAndando)) animator.SetBool(parametroAndando, andando);
        if (!string.IsNullOrEmpty(parametroCorrendo)) animator.SetBool(parametroCorrendo, correndo);
    }
}
