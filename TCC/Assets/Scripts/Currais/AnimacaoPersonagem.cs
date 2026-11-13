using UnityEngine;

// Acompanha também movimentos de cutscene, quando o seguidor está desativado.
[DefaultExecutionOrder(1000)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class AnimacaoPersonagem : MonoBehaviour
{
    [SerializeField] private Sprite[] repouso;
    [SerializeField] private Sprite[] corrida;
    [SerializeField, Min(0.02f)] private float intervaloRepouso = .25f;
    [SerializeField, Min(0.02f)] private float intervaloCorrida = .13f;
    private Sprite[] repousoPreparado, corridaPreparada;
    private float tempo;
    private int estadoAnterior = -1;
    private static readonly int Andando = Animator.StringToHash("IsWalking");
    private static readonly int Correndo = Animator.StringToHash("IsRunning");
    private SeguidorFamilia seguidor;
    private Animator animador;
    private SpriteRenderer desenho;
    private Vector3 posicaoAnterior;

    private void Awake()
    {
        seguidor = GetComponent<SeguidorFamilia>();
        animador = GetComponent<Animator>();
        desenho = GetComponent<SpriteRenderer>();
        if (desenho.sprite != null)
        {
            Sprite referencia = desenho.sprite;
            Vector2 pivo = new Vector2(referencia.pivot.x / referencia.rect.width, referencia.pivot.y / referencia.rect.height);
            repousoPreparado = PrepararQuadros(repouso, referencia.bounds.size.y, pivo);
            corridaPreparada = PrepararQuadros(corrida, referencia.bounds.size.y, pivo);
        }
    }

    private void OnEnable() => posicaoAnterior = transform.position;

    private void LateUpdate()
    {
        float deslocamento = transform.position.x - posicaoAnterior.x;
        posicaoAnterior = transform.position;
        // Ferimentos e sequências de memória desativam o Animator de propósito.
        if (animador == null || !animador.enabled) return;

        bool emFila = seguidor != null && seguidor.isActiveAndEnabled;
        bool andando = emFila ? seguidor.EstaAndando : Time.timeScale > 0f && Mathf.Abs(deslocamento) > 0.001f;
        bool correndo = emFila ? seguidor.EstaCorrendo : andando && Mathf.Abs(deslocamento) / Mathf.Max(Time.deltaTime, 0.0001f) > 6.5f;
        if (!emFila)
        {
            animador.SetBool(Andando, andando);
            animador.SetBool(Correndo, correndo);
            if (andando) desenho.flipX = deslocamento * transform.lossyScale.x < 0f;
        }
        int estado = !andando ? 0 : correndo ? 2 : 1;
        if (estado != estadoAnterior) tempo = 0f;
        else if (!PauseHelper.BlockInput()) tempo += Time.deltaTime;
        estadoAnterior = estado;
        Sprite[] quadros = estado == 0 ? repousoPreparado : estado == 2 ? corridaPreparada : null;
        if (quadros != null && quadros.Length > 0)
        {
            float intervalo = estado == 0 ? intervaloRepouso : intervaloCorrida;
            desenho.sprite = quadros[Mathf.FloorToInt(tempo / Mathf.Max(.02f, intervalo)) % quadros.Length];
        }
    }

    private static Sprite[] PrepararQuadros(Sprite[] fontes, float altura, Vector2 pivo)
    {
        if (fontes == null || fontes.Length == 0 || fontes[0] == null) return null;
        Sprite[] quadros = new Sprite[fontes.Length];
        float ppu = fontes[0].rect.height / Mathf.Max(.001f, altura);
        for (int i = 0; i < fontes.Length; i++)
        {
            Sprite fonte = fontes[i] != null ? fontes[i] : fontes[0];
            quadros[i] = Sprite.Create(fonte.texture, fonte.rect, pivo, ppu, 0, SpriteMeshType.FullRect);
        }
        return quadros;
    }

    private void OnDestroy()
    {
        if (repousoPreparado != null) foreach (Sprite quadro in repousoPreparado) if (quadro != null) Destroy(quadro);
        if (corridaPreparada != null) foreach (Sprite quadro in corridaPreparada) if (quadro != null) Destroy(quadro);
    }
}
