using UnityEngine;

// Ciclos de sprites existentes, sem alterar Transform, Collider ou escala dos personagens.
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class QuadrosAmbiente : MonoBehaviour
{
    [SerializeField] private Sprite[] quadros;
    [SerializeField, Min(0.02f)] private float intervalo = 0.2f;
    [SerializeField] private float alturaReferencia = 1f;
    [SerializeField] private float larguraReferencia = 1f;
    [SerializeField] private Vector2 pivoReferencia = new Vector2(0.5f, 0f);
    [SerializeField] private bool preservarLargura;
    [SerializeField] private bool aposEntregaBola;
    [SerializeField] private bool pararQuandoBentoCair;

    private SpriteRenderer desenho;
    private Sprite[] preparados;
    private float tempo;
    private bool iniciou;

    private void Awake() => desenho = GetComponent<SpriteRenderer>();

    private bool Liberado => !aposEntregaBola ||
        (TutorialManager.Instance != null && TutorialManager.Instance.IsCompleted);

    private void LateUpdate()
    {
        if (!Liberado || quadros == null || quadros.Length == 0) return;
        if (!iniciou)
        {
            PrepararQuadros();
            if (preparados == null) return;
            Animator anterior = GetComponent<Animator>();
            if (anterior != null) anterior.enabled = false;
            iniciou = true;
        }
        bool caiu = pararQuandoBentoCair && GameProgress.Instance != null &&
            (GameProgress.Instance.Data.curraisEtapaBusca >= 5 || GameProgress.Instance.Data.curraisBentoMorto);
        if (!caiu && !PauseHelper.BlockInput() && Time.timeScale > 0f) tempo += Time.deltaTime;
        int indice = caiu ? 0 : Mathf.FloorToInt(tempo / Mathf.Max(0.02f, intervalo)) % preparados.Length;
        desenho.sprite = preparados[indice];
    }

    private void PrepararQuadros()
    {
        Sprite primeiro = quadros[0];
        if (primeiro == null) return;
        float ppu = preservarLargura
            ? primeiro.rect.width / Mathf.Max(0.001f, larguraReferencia)
            : primeiro.rect.height / Mathf.Max(0.001f, alturaReferencia);
        preparados = new Sprite[quadros.Length];
        for (int i = 0; i < quadros.Length; i++)
        {
            Sprite origem = quadros[i] != null ? quadros[i] : primeiro;
            preparados[i] = Sprite.Create(origem.texture, origem.rect, pivoReferencia, ppu,
                0, SpriteMeshType.FullRect);
            preparados[i].name = origem.name + " - ciclo";
        }
    }

    private void OnDestroy()
    {
        if (preparados == null) return;
        foreach (Sprite quadro in preparados) if (quadro != null) Destroy(quadro);
    }
}
