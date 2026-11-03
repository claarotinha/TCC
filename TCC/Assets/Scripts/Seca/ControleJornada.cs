
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

    private void LateUpdate()
    {
        if (mari == null)
            return;

        Transform limite = limitesDasParadas[Mathf.Min(paradasConcluidas, 4)];

        if (limite == null)
            return;

        float limiteX = limite.position.x;
        if (paradasConcluidas == 2)
            limiteX += aguaTerceiraLiberada && (encontroTerceiraLiberado || !SegundaParada.JanelaQuebrada)
                ? trechoAposAguaTerceira : trechoAguaTerceira;
        if (paradasConcluidas == 3) limiteX += trechoJuazeiro;
        if (paradasConcluidas == 4) limiteX += trechoAcampamento;
        if (paradasConcluidas >= 5) limiteX += trechoAposAcampamento;
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




