using UnityEngine;

// A arte fica em um filho editável; o corpo, pivô e collider do personagem não mudam.
[DefaultExecutionOrder(1500)]
[DisallowMultipleComponent]
public sealed class PoseEntradaCurrais : MonoBehaviour
{
    [SerializeField] private SpriteRenderer origem, visual;
    [SerializeField] private Sprite[] quadros;
    [SerializeField, Min(.04f)] private float intervalo = .18f;
    [SerializeField] private bool repetir;
    [SerializeField] private Transform olharPara;
    private Vector3 posicaoVisual;
    private float centroReferenciaX;
    private Sprite[] preparados;
    private bool mostrando;
    private float tempo;

    private void Awake()
    {
        if (visual != null) posicaoVisual = visual.transform.localPosition;
        // A Estrada pode trocar o sprite e ajustar sua escala durante Start.
        // A referência da pose só é capturada quando ela realmente aparece.
    }

    public void DefinirVisivel(bool visivel)
    {
        if (mostrando == visivel) return;
        mostrando = visivel;
        tempo = 0f;
        Aplicar();
    }

    private void Preparar()
    {
        if (preparados != null || origem == null || origem.sprite == null ||
            quadros == null || quadros.Length == 0 || quadros[0] == null) return;
        Sprite referencia = origem.sprite;
        centroReferenciaX = referencia.bounds.center.x;
        Texture2D textura = quadros[0].texture;
        int largura = Mathf.FloorToInt(quadros[0].rect.width);
        int altura = Mathf.FloorToInt(quadros[0].rect.height);
        int esquerda = largura, baixo = altura, direita = -1, cima = -1;
        // Uma caixa comum aos quatro quadros mantém pés e enquadramento estáveis.
        if (textura.isReadable)
        {
            Color32[] pixels = textura.GetPixels32();
            foreach (Sprite quadro in quadros)
            {
                if (quadro == null || quadro.texture != textura) continue;
                int x0 = Mathf.FloorToInt(quadro.rect.x), y0 = Mathf.FloorToInt(quadro.rect.y);
                int w = Mathf.FloorToInt(quadro.rect.width), h = Mathf.FloorToInt(quadro.rect.height);
                largura = Mathf.Min(largura, w); altura = Mathf.Min(altura, h);
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        if (pixels[(y0 + y) * textura.width + x0 + x].a < 128) continue;
                        esquerda = Mathf.Min(esquerda, x); direita = Mathf.Max(direita, x);
                        baixo = Mathf.Min(baixo, y); cima = Mathf.Max(cima, y);
                    }
            }
        }
        if (direita < esquerda || cima < baixo)
        { esquerda = baixo = 0; direita = largura - 1; cima = altura - 1; }
        direita = Mathf.Min(direita, largura - 1); cima = Mathf.Min(cima, altura - 1);
        float wFinal = direita - esquerda + 1, hFinal = cima - baixo + 1;
        float ppu = hFinal / Mathf.Max(.001f, referencia.bounds.size.y);
        Vector2 pivo = new Vector2(Mathf.Clamp01(.5f - referencia.bounds.center.x * ppu / wFinal),
            referencia.pivot.y / referencia.rect.height);
        preparados = new Sprite[quadros.Length];
        for (int i = 0; i < quadros.Length; i++)
        {
            Sprite quadro = quadros[i] != null ? quadros[i] : quadros[0];
            Rect recorte = new Rect(quadro.rect.x + esquerda, quadro.rect.y + baixo, wFinal, hFinal);
            preparados[i] = Sprite.Create(quadro.texture, recorte, pivo, ppu, 0, SpriteMeshType.FullRect);
            preparados[i].name = quadro.name + " - pose normalizada";
        }
    }

    private void LateUpdate()
    {
        if (mostrando && !PauseHelper.BlockInput() && Time.timeScale > 0f) tempo += Time.deltaTime;
        Aplicar();
    }

    private void Aplicar()
    {
        if (origem == null || visual == null) return;
        if (mostrando && preparados == null) Preparar();
        bool ativa = mostrando && origem.enabled && preparados != null && preparados.Length > 0;
        origem.forceRenderingOff = ativa;
        visual.enabled = ativa;
        if (!ativa) return;
        int quadro = Mathf.FloorToInt(tempo / Mathf.Max(.04f, intervalo));
        quadro = repetir ? quadro % preparados.Length : Mathf.Min(quadro, preparados.Length - 1);
        visual.sprite = preparados[quadro];
        visual.color = origem.color;
        visual.flipX = olharPara != null ? olharPara.position.x < transform.position.x : origem.flipX;
        // Mudar o olhar não desloca a personagem quando o sprite original usa pivô lateral.
        float origemX = origem.flipX ? -centroReferenciaX : centroReferenciaX;
        float visualX = visual.flipX ? -centroReferenciaX : centroReferenciaX;
        visual.transform.localPosition = posicaoVisual + Vector3.right * (origemX - visualX);
        visual.sortingLayerID = origem.sortingLayerID;
        visual.sortingOrder = origem.sortingOrder;
    }

    private void OnDisable()
    {
        mostrando = false;
        if (origem != null) origem.forceRenderingOff = false;
        if (visual != null)
        {
            visual.enabled = false;
            visual.transform.localPosition = posicaoVisual;
        }
    }

    private void OnDestroy()
    {
        if (preparados == null) return;
        foreach (Sprite quadro in preparados) if (quadro != null) Destroy(quadro);
    }
}
