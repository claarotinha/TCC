
using UnityEngine;

public class ControleJornada : MonoBehaviour
{
    public static ControleJornada Instance { get; private set; }

    [Header("Progresso")]
    [SerializeField, Range(0, 5)]
    private int paradasConcluidas = 0;

    [Header("Referências")]
    [SerializeField] private Transform mari;
    [SerializeField] private SegundaParada caminhada;
    [SerializeField] private Transform[] limitesDasParadas = new Transform[5];

    [Header("Trecho da terceira parada")]
    [SerializeField, Min(0f)] private float trechoAguaTerceira = 5f;
    [SerializeField, Min(0f)] private float trechoAposAguaTerceira = 8f;
    private bool aguaTerceiraLiberada;
    private bool encontroTerceiraLiberado;

    [Header("Trecho do juazeiro")]
    [SerializeField, Min(0f)] private float trechoJuazeiro = 4f;

    [Header("Acampamento e ultima caminhada")]
    [SerializeField, Min(0f)] private float trechoAcampamento = 7.5f;
    [SerializeField, Min(0f)] private float trechoAposAcampamento = 22.5f;

    [SerializeField] private Transform limiteFortaleza;

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

    private void Start()
    {
        if (GameProgress.Instance == null) return;
        GameProgressData dados = GameProgress.Instance.Data;
        int salvo = Mathf.Clamp(dados.estradaParadasConcluidas, 0, 5);
        if (dados.estradaAbrigoConcluido) salvo = Mathf.Max(salvo, 2);
        if (dados.estradaJuazeiroConcluido) salvo = Mathf.Max(salvo, 4);
        if (dados.estradaAcampamentoConcluido) salvo = 5;
        paradasConcluidas = Mathf.Max(paradasConcluidas, salvo);
        aguaTerceiraLiberada = dados.estradaBarrigudaConcluida;
        encontroTerceiraLiberado = dados.estradaCurativoAplicado;
        if (paradasConcluidas == 2 && dados.estradaTerceiraAguaIniciada)
        {
            paradaAtiva = true;
            paradaAtual = 3;
        }
        else if (paradasConcluidas == 3 && dados.estradaJuazeiroIniciado)
        {
            paradaAtiva = true;
            paradaAtual = 4;
        }
        else if (paradasConcluidas == 4 && dados.estradaAcampamentoIniciado)
        {
            paradaAtiva = true;
            paradaAtual = 5;
        }
    }

    public float LimiteAvancoX
    {
        get
        {
            if (limitesDasParadas == null || limitesDasParadas.Length == 0) return float.PositiveInfinity;
            Transform limite = limitesDasParadas[Mathf.Min(paradasConcluidas, Mathf.Min(4, limitesDasParadas.Length - 1))];

            if (limite == null)
                return float.PositiveInfinity;

            float limiteX = limite.position.x;
            if (paradasConcluidas == 2)
                limiteX += (aguaTerceiraLiberada || encontroTerceiraLiberado)
                    ? trechoAposAguaTerceira : trechoAguaTerceira;
            if (paradasConcluidas == 3) limiteX += trechoJuazeiro;
            if (paradasConcluidas == 4) limiteX += trechoAcampamento;
            if (paradasConcluidas >= 5) limiteX = limiteFortaleza != null
                ? limiteFortaleza.position.x : limiteX + trechoAposAcampamento;
            if (paradasConcluidas == 1 && caminhada != null)
                limiteX = Mathf.Min(limiteX, caminhada.LimiteCaminhadaAntesDaBusca);
            return limiteX;
        }
    }

    public Vector3 MigrarPosicaoSalva(Vector3 posicao, GameProgressData dados)
    {
        // Os marcos e seus objetos foram transladados juntos, preservando a posição
        // relativa dentro de cada parada dos checkpoints anteriores à ampliação.
        if (dados.estradaAcampamentoConcluido || dados.estradaParadasConcluidas >= 5)
            posicao.x += posicao.x >= 162f ? 2120f : 1800f;
        else if (dados.estradaAcampamentoIniciado || dados.estradaParadasConcluidas >= 4)
            posicao.x += posicao.x >= 135f ? 1800f : 1500f;
        else if (dados.estradaJuazeiroIniciado || dados.estradaParadasConcluidas >= 3)
            posicao.x += posicao.x >= 105f ? 1500f : 1200f;
        else if (dados.estradaAbrigoConcluido || dados.estradaParadasConcluidas >= 2)
        {
            if (!dados.estradaTerceiraAguaIniciada && posicao.x < 79f && caminhada != null)
                posicao.x = caminhada.InicioCaminhadaDaManhaX;
            else posicao.x += 1200f;
        }
        else if (dados.estradaParadasConcluidas >= 1)
            // A busca ainda não possui checkpoint próprio: retoma a caminhada inicial.
            posicao.x = Mathf.Min(posicao.x + 260f, caminhada != null ? caminhada.transform.position.x : posicao.x + 260f);
        return posicao;
    }

    // Conta deslocamento real, sem contar pausa, inventário, restauração ou leitura.
    public System.Collections.IEnumerator EsperarCaminhada(float segundos)
    {
        float tempo = 0f;
        float anterior = mari != null ? mari.position.x : 0f;
        while (tempo < segundos)
        {
            yield return null;
            if (mari == null) continue;
            float atual = mari.position.x;
            float deslocamento = Mathf.Abs(atual - anterior);
            anterior = atual;
            PlayerMovement movimento = mari.GetComponent<PlayerMovement>();
            if (Time.timeScale <= 0f || PauseHelper.BlockInput() || GameSaveSystem.IsRestoring ||
                InvestigationGuard.Blocked || movimento == null || !movimento.enabled) continue;
            // Teletransportes/cutscenes não contam como caminhada.
            if (deslocamento > 0.001f && deslocamento <= 12f * Mathf.Max(Time.deltaTime, Time.fixedDeltaTime))
            {
                Rigidbody2D corpo = mari.GetComponent<Rigidbody2D>();
                float velocidade = corpo != null ? Mathf.Abs(corpo.linearVelocity.x) : 0f;
                if (velocidade > 0.01f)
                    tempo += Mathf.Min(deslocamento / velocidade, Mathf.Max(Time.deltaTime, Time.fixedDeltaTime) * 2f);
            }
        }
    }

    private void LateUpdate()
    {
        if (mari == null) return;
        float limiteX = LimiteAvancoX;
        // Bloqueia somente o avanço para a direita.
        if (mari.position.x > limiteX)
        {
            Vector3 posicao = mari.position;
            posicao.x = limiteX;
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

    public void LiberarTrechoTerceiraParada()
    {
        if (paradaAtiva && paradaAtual == 3) aguaTerceiraLiberada = true;
        // A terceira parada so termina depois do ferimento/encontro da etapa seguinte.
    }

    public void LiberarEncontroTerceiraParada()
    {
        encontroTerceiraLiberado = true;
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
        if (GameProgress.Instance != null)
            GameProgress.Instance.Data.estradaParadasConcluidas = numero;
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




