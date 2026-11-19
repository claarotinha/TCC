using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class FogueiraEstradaVisual : MonoBehaviour
{
    [SerializeField] private Sprite acesa, brasas;
    [SerializeField] private Sprite[] assando;
    [SerializeField, Min(.02f)] private float intervalo = .22f;
    private SpriteRenderer desenho;
    private Sprite[] fogoPreparado, comidaPreparada, fimPreparado, atuais;
    private float tempo;

    private void Awake()
    {
        desenho = GetComponent<SpriteRenderer>();
        Sprite referencia = desenho.sprite;
        fogoPreparado = Preparar(new[] { acesa }, referencia);
        comidaPreparada = Preparar(assando, referencia);
        fimPreparado = Preparar(new[] { brasas }, referencia);
    }

    public void Mostrar(bool montada, bool refeicao, bool terminou, bool emBrasas = false)
    {
        desenho.enabled = montada && !terminou;
        Sprite[] novo = terminou || emBrasas ? fimPreparado : refeicao ? comidaPreparada : fogoPreparado;
        if (novo != atuais) tempo = 0f;
        atuais = novo;
        Aplicar();
    }

    private void LateUpdate()
    {
        if (!desenho.enabled || PauseHelper.BlockInput()) return;
        tempo += Time.deltaTime;
        Aplicar();
    }

    private void Aplicar()
    {
        if (atuais == null || atuais.Length == 0) return;
        desenho.sprite = atuais[Mathf.FloorToInt(tempo / Mathf.Max(.02f, intervalo)) % atuais.Length];
    }

    private static Sprite[] Preparar(Sprite[] fontes, Sprite referencia)
    {
        if (fontes == null || fontes.Length == 0 || referencia == null) return null;
        Sprite[] resultado = new Sprite[fontes.Length];
        for (int i = 0; i < fontes.Length; i++)
        {
            Sprite fonte = fontes[i];
            if (fonte == null) continue;
            float ppu = fonte.rect.width / referencia.bounds.size.x;
            float altura = fonte.rect.height / ppu;
            Vector2 pivo = new Vector2(.5f, -referencia.bounds.min.y / altura);
            resultado[i] = Sprite.Create(fonte.texture, fonte.rect, pivo, ppu, 0, SpriteMeshType.FullRect);
        }
        return resultado;
    }

    private void OnDestroy()
    {
        foreach (Sprite[] conjunto in new[] { fogoPreparado, comidaPreparada, fimPreparado })
            if (conjunto != null) foreach (Sprite quadro in conjunto) if (quadro != null) Destroy(quadro);
    }
}
