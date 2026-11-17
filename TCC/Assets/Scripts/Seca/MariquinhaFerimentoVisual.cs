using UnityEngine;

// Usa os quadros da autora; não move nem redimensiona a personagem.
[DefaultExecutionOrder(1200)]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class MariquinhaFerimentoVisual : MonoBehaviour
{
    [SerializeField] private Sprite[] feridaIdle, feridaWalk, feridaRun;
    [SerializeField] private Sprite[] curativoIdle, curativoWalk;
    [SerializeField, Min(.02f)] private float intervaloIdle = .25f, intervaloWalk = .16f, intervaloRun = .1f;
    private SpriteRenderer desenho;
    private Animator animador;
    private SeguidorFamilia seguidor;
    private Sprite[][] preparados;
    private Vector3 anterior;
    private float tempo;
    private int estadoAnterior = -1;
    private bool escolhaLocal, feridaLocal, curativoLocal;

    private void Awake()
    {
        desenho = GetComponent<SpriteRenderer>(); animador = GetComponent<Animator>();
        seguidor = GetComponent<SeguidorFamilia>();
        Sprite referencia = desenho.sprite;
        if (referencia == null) return;
        Vector2 pivo = new Vector2(referencia.pivot.x / referencia.rect.width, referencia.pivot.y / referencia.rect.height);
        Sprite[][] fontes = { feridaIdle, feridaWalk, feridaRun, curativoIdle, curativoWalk };
        preparados = new Sprite[fontes.Length][];
        for (int a = 0; a < fontes.Length; a++)
        {
            Sprite[] fonte = fontes[a];
            if (fonte == null || fonte.Length == 0 || fonte[0] == null) continue;
            preparados[a] = new Sprite[fonte.Length];
            float ppu = fonte[0].rect.height / Mathf.Max(.001f, referencia.bounds.size.y);
            for (int i = 0; i < fonte.Length; i++)
            {
                Sprite quadro = fonte[i] != null ? fonte[i] : fonte[0];
                preparados[a][i] = Sprite.Create(quadro.texture, quadro.rect, pivo, ppu, 0, SpriteMeshType.FullRect);
            }
        }
    }
    private void OnEnable() { anterior = transform.position; tempo = 0f; estadoAnterior = -1; }
    public void AtualizarFerimento(bool ferida, bool curativo)
    { escolhaLocal = true; feridaLocal = ferida; curativoLocal = curativo; }
    private void LateUpdate()
    {
        float dx = transform.position.x - anterior.x; anterior = transform.position;
        if (preparados == null || (animador != null && !animador.enabled) || !desenho.enabled) return;
        bool ferida = escolhaLocal ? feridaLocal : GameProgress.Instance != null && GameProgress.Instance.Data.estradaMariquinhaFerida;
        bool curativo = escolhaLocal ? curativoLocal : GameProgress.Instance != null && GameProgress.Instance.Data.estradaCurativoAplicado;
        if (!ferida) return;
        bool emFila = seguidor != null && seguidor.isActiveAndEnabled;
        bool andando = emFila ? seguidor.EstaAndando : Mathf.Abs(dx) > .001f;
        bool correndo = emFila ? seguidor.EstaCorrendo : andando && Mathf.Abs(dx) / Mathf.Max(.0001f, Time.deltaTime) > 6.5f;
        int estado = curativo ? (andando ? 4 : 3) : !andando ? 0 : correndo ? 2 : 1;
        if (estado != estadoAnterior) tempo = 0f;
        else if (!PauseHelper.BlockInput() && Time.timeScale > 0f) tempo += Time.deltaTime;
        estadoAnterior = estado;
        Sprite[] quadros = preparados[estado];
        if (quadros == null || quadros.Length == 0) return;
        float intervalo = !andando ? intervaloIdle : correndo && !curativo ? intervaloRun : intervaloWalk;
        desenho.sprite = quadros[Mathf.FloorToInt(tempo / Mathf.Max(.02f, intervalo)) % quadros.Length];
    }
    private void OnDestroy()
    {
        if (preparados == null) return;
        foreach (Sprite[] grupo in preparados)
            if (grupo != null) foreach (Sprite quadro in grupo) if (quadro != null) Destroy(quadro);
    }
}
