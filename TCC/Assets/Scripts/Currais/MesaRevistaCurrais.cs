using System.Collections.Generic;
using UnityEngine;

// Lugares e pilhas já existem na Hierarchy; não instancia objetos durante a revista.
public sealed class MesaRevistaCurrais : MonoBehaviour
{
    [SerializeField] private InventorySaveCatalog catalogo;
    [SerializeField] private SpriteRenderer[] lugares;
    [SerializeField] private GameObject pilhaPertences, volumeAdicional;
    [SerializeField, Min(0)] private int lugaresNaMesa = 4;
    [SerializeField, Min(.05f)] private float tamanhoLocal = .48f;
    [SerializeField] private Sprite semIcone;
    private readonly List<string> idsExibidos = new List<string>();
    private readonly Dictionary<Sprite, Sprite> desenhos = new Dictionary<Sprite, Sprite>();
    private bool iniciou;

    public void Atualizar(IReadOnlyList<string> ids)
    {
        int quantidade = ids != null ? ids.Count : 0;
        bool mudou = !iniciou || quantidade != idsExibidos.Count;
        if (!mudou)
            for (int i = 0; i < quantidade; i++)
                if (ids[i] != idsExibidos[i]) { mudou = true; break; }
        if (!mudou) return;
        iniciou = true;
        idsExibidos.Clear();
        if (ids != null) for (int i = 0; i < ids.Count; i++) idsExibidos.Add(ids[i]);
        if (lugares == null) return;
        for (int i = 0; i < lugares.Length; i++)
        {
            SpriteRenderer lugar = lugares[i];
            if (lugar == null) continue;
            bool mostrar = i < quantidade;
            lugar.enabled = mostrar;
            if (!mostrar) continue;
            ItemData item = null;
            if (catalogo != null) catalogo.TryGetItem(ids[i], out item);
            lugar.sprite = Desenho(item);
            lugar.color = item != null && item.itemName == "Cabaça cheia"
                ? new Color(.65f, .85f, 1f, 1f) : Color.white;
        }
        if (pilhaPertences != null) pilhaPertences.SetActive(quantidade > lugaresNaMesa);
        // A representação adicional não impõe limite às entregas nem ao save.
        if (volumeAdicional != null) volumeAdicional.SetActive(quantidade > lugares.Length);
    }

    private Sprite Desenho(ItemData item)
    {
        Sprite fonte = item != null && item.icon != null ? item.icon : semIcone;
        if (fonte == null) return null;
        if (desenhos.TryGetValue(fonte, out Sprite pronto)) return pronto;
        float ppu = Mathf.Max(fonte.rect.width, fonte.rect.height) / Mathf.Max(.05f, tamanhoLocal);
        pronto = Sprite.Create(fonte.texture, fonte.rect, new Vector2(.5f, .5f), ppu, 0, SpriteMeshType.FullRect);
        pronto.name = fonte.name + " - mesa da revista";
        desenhos.Add(fonte, pronto);
        return pronto;
    }

    private void OnDestroy()
    {
        foreach (Sprite sprite in desenhos.Values) if (sprite != null) Destroy(sprite);
        desenhos.Clear();
    }
}
