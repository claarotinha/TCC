using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
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

    private static readonly Dictionary<Transform, Rastro> rastros = new Dictionary<Transform, Rastro>();
    private Rastro rastro;

    private struct Ponto
    {
        public float x, distancia;
        public Ponto(float x, float distancia) { this.x = x; this.distancia = distancia; }
    }

    private class Rastro
    {
        public readonly List<SeguidorFamilia> membros = new List<SeguidorFamilia>();
        private readonly List<Ponto> pontos = new List<Ponto>();
        private int quadro = -1;
        private float comprimento = 2f;
        public float velocidadeMari;

        public void Atualizar(Transform lider)
        {
            if (quadro == Time.frameCount) return;
            quadro = Time.frameCount;
            foreach (SeguidorFamilia membro in membros)
                if (membro != null) comprimento = Mathf.Max(comprimento, membro.distanciaAtras + 2f);
            float x = lider.position.x;
            if (pontos.Count == 0 || Mathf.Abs(x - pontos[pontos.Count - 1].x) > 3f)
            {
                // A semente só descreve a fila; nunca reposiciona os familiares.
                // Reinicia em trocas de cenário/cutscene, não nas viradas normais.
                SpriteRenderer visual = lider.GetComponent<SpriteRenderer>();
                float direcao = visual != null && visual.flipX ? -1f : 1f;
                pontos.Clear();
                pontos.Add(new Ponto(x - direcao * comprimento, 0f));
                pontos.Add(new Ponto(x, comprimento));
                velocidadeMari = 0f;
                return;
            }
            Ponto ultimo = pontos[pontos.Count - 1];
            float deslocamento = Mathf.Abs(x - ultimo.x);
            velocidadeMari = deslocamento / Mathf.Max(Time.deltaTime, 0.0001f);
            if (deslocamento > 0.00001f)
                pontos.Add(new Ponto(x, ultimo.distancia + deslocamento));
            float removerAntes = pontos[pontos.Count - 1].distancia - comprimento;
            while (pontos.Count > 2 && pontos[1].distancia < removerAntes) pontos.RemoveAt(0);
        }

        public float Destino(float distancia)
        {
            float alvo = pontos[pontos.Count - 1].distancia - distancia;
            for (int i = pontos.Count - 1; i > 0; i--)
            {
                Ponto a = pontos[i - 1], b = pontos[i];
                if (alvo >= a.distancia)
                    return Mathf.Lerp(a.x, b.x, Mathf.InverseLerp(a.distancia, b.distancia, alvo));
            }
            return pontos[0].x;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRastros() => rastros.Clear();

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (mari == null) return;
        if (!rastros.TryGetValue(mari, out rastro))
        {
            rastro = new Rastro();
            rastros.Add(mari, rastro);
        }
        rastro.membros.Add(this);
    }

    private void OnDisable()
    {
        if (rastro != null)
        {
            rastro.membros.Remove(this);
            if (rastro.membros.Count == 0 && !object.ReferenceEquals(mari, null)) rastros.Remove(mari);
        }
        rastro = null;
        EstaAndando = EstaCorrendo = false;
        AtualizarAnimacoes();
    }

    private void LateUpdate()
    {
        if (mari == null || rastro == null) return;
        if (PauseHelper.BlockInput() || Time.timeScale == 0f)
        {
            EstaAndando = EstaCorrendo = false;
            AtualizarAnimacoes();
            return;
        }
        rastro.Atualizar(mari);
        float origem = transform.position.x;
        float destino = rastro.Destino(distanciaAtras);
        bool correndo = rastro.velocidadeMari > (velocidade + velocidadeCorrida) * 0.5f;
        Vector3 posicao = transform.position;
        float tolerancia = rastro.velocidadeMari > 0.01f ? 0.00001f : distanciaParada;
        if (Mathf.Abs(destino - origem) > tolerancia)
            posicao.x = Mathf.MoveTowards(origem, destino, (correndo ? velocidadeCorrida : velocidade) * Time.deltaTime);
        transform.position = posicao;
        float movimento = posicao.x - origem;
        EstaAndando = Mathf.Abs(movimento) > 0.00001f;
        EstaCorrendo = EstaAndando && correndo;
        if (EstaAndando)
        {
            Direcao = Mathf.Sign(movimento);
            if (spriteRenderer != null) spriteRenderer.flipX = Direcao * transform.lossyScale.x < 0f;
        }
        AtualizarAnimacoes();
    }

    private void AtualizarAnimacoes()
    {
        if (animator == null || !animator.enabled) return;
        if (!string.IsNullOrEmpty(parametroAndando)) animator.SetBool(parametroAndando, EstaAndando);
        if (!string.IsNullOrEmpty(parametroCorrendo)) animator.SetBool(parametroCorrendo, EstaCorrendo);
    }
}
