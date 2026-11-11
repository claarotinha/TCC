using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Lavagem — separar pelo inventário")]
    [SerializeField] private Collider2D areaPanosUso, areaPanosAtendimento;
    [SerializeField] private ItemData panoUsoLavagem, panoAtendimentoLavagem, tiraPanoLavagem;
    [SerializeField] private GameObject panoUsoEntregue, panoAtendimentoEntregue;
    [SerializeField] private Fala[] instrucoesLavagem, tiraPanoAutorizada, guardaRepreensao, raimundoTrabalho;

    [Header("Passagem vigiada — marcas editáveis")]
    [SerializeField] private Transform guardaRonda, rondaInicio, rondaFim, retornoRonda, fimPassagem;
    [SerializeField] private Collider2D abrigoDosSacos;
    [SerializeField, Min(.1f)] private float velocidadeRonda = 2f;
    [SerializeField, Min(.1f)] private float alcanceVigia = 3.5f;
    private bool rondaVaiAoFim;
    private float pausaRonda;
    private bool rondaPreparada;

    private bool ReferenciasLavagemValidas()
    {
        UnityEngine.Object[] refs = { areaPanosUso, areaPanosAtendimento, panoUsoLavagem,
            panoAtendimentoLavagem, tiraPanoLavagem, panoUsoEntregue, panoAtendimentoEntregue,
            guardaRonda, rondaInicio, rondaFim, retornoRonda, fimPassagem, abrigoDosSacos };
        string[] nomes = { nameof(areaPanosUso), nameof(areaPanosAtendimento), nameof(panoUsoLavagem),
            nameof(panoAtendimentoLavagem), nameof(tiraPanoLavagem), nameof(panoUsoEntregue), nameof(panoAtendimentoEntregue),
            nameof(guardaRonda), nameof(rondaInicio), nameof(rondaFim), nameof(retornoRonda), nameof(fimPassagem), nameof(abrigoDosSacos) };
        bool ok = true;
        for (int i = 0; i < refs.Length; i++) ok &= ConferirReferencia(refs[i], nomes[i]);
        return ok && FalasValidas(instrucoesLavagem) && FalasValidas(tiraPanoAutorizada) && FalasValidas(guardaRepreensao) && FalasValidas(raimundoTrabalho);
    }

    private void PrepararEtapaLavagem()
    {
        if (Dados.curraisEtapaBusca >= 3)
        {
            Dados.curraisPanosSeparados = 3;
            if (!Dados.curraisTiraPanoRecolhida)
            {
                if (!TemItem(tiraPanoLavagem)) InventoryManager.Instance.AddItem(tiraPanoLavagem);
                Dados.curraisTiraPanoRecolhida = true;
            }
        }
        if (Dados.curraisEtapaBusca >= 4) Dados.curraisPassagemVigiadaConcluida = true;
        if (Dados.curraisEtapaBusca == 2) GarantirPanosLavagem();
        AtualizarVisuaisLavagem();
    }

    private void GarantirPanosLavagem()
    {
        if ((Dados.curraisPanosSeparados & 1) == 0 && !TemItem(panoUsoLavagem)) InventoryManager.Instance.AddItem(panoUsoLavagem);
        if ((Dados.curraisPanosSeparados & 2) == 0 && !TemItem(panoAtendimentoLavagem)) InventoryManager.Instance.AddItem(panoAtendimentoLavagem);
    }

    private void AtualizarVisuaisLavagem()
    {
        bool separando = Dados.curraisEtapaBusca == 2 && Dados.curraisPanelaConcluida;
        areaPanosUso.enabled = separando && (Dados.curraisPanosSeparados & 1) == 0;
        areaPanosAtendimento.enabled = separando && (Dados.curraisPanosSeparados & 2) == 0;
        panoUsoEntregue.SetActive((Dados.curraisPanosSeparados & 1) != 0);
        panoAtendimentoEntregue.SetActive((Dados.curraisPanosSeparados & 2) != 0);
        bool vigiaAtiva = Dados.curraisPanelaConcluida && Dados.curraisEtapaBusca >= 3 && Dados.curraisEtapaBusca <= 5;
        guardaRonda.gameObject.SetActive(vigiaAtiva);
        if (vigiaAtiva && !rondaPreparada)
        {
            guardaRonda.position = rondaFim.position;
            rondaVaiAoFim = false;
            rondaPreparada = true;
        }
    }

    private IEnumerator SepararPanoLavagem(bool atendimento)
    {
        ItemData esperado = atendimento ? panoAtendimentoLavagem : panoUsoLavagem;
        ItemData selecionado = InventoryManager.Instance.SelectedItem;
        if (selecionado != esperado || !TemItem(esperado))
        {
            yield return Dizer(instrucoesLavagem);
            yield break;
        }
        int bit = atendimento ? 2 : 1;
        if ((Dados.curraisPanosSeparados & bit) != 0) yield break;
        InventoryManager.Instance.RemoveItem(esperado);
        Dados.curraisPanosSeparados |= bit;
        AtualizarEstadoBusca(); Salvar();
        yield return Dizer(new[] { new Fala { personagem = "Lavadeira", texto = atendimento
            ? "Isso. As peças do atendimento ficam separadas, à direita. Lave as mãos depois de mexer nelas."
            : "Certo. Os panos de uso ficam do lado esquerdo." } });
        if (Dados.curraisPanosSeparados == 3) objetivoTexto.text = "Os lotes estão separados. Pegue a tira que a lavadeira deixou para você.";
    }

    private IEnumerator RecolherTiraLavagem()
    {
        if (Dados.curraisPanosSeparados != 3) yield break;
        if (!Dados.curraisTiraPanoRecolhida)
        {
            if (!TemItem(tiraPanoLavagem)) InventoryManager.Instance.AddItem(tiraPanoLavagem);
            Dados.curraisTiraPanoRecolhida = true;
            Salvar();
        }
        yield return Dizer(tiraPanoAutorizada);
        AvancarCampo(3);
    }

    private void AtualizarPatrulha()
    {
        if (!Dados.curraisPanelaConcluida || Dados.curraisEtapaBusca < 3 || Dados.curraisEtapaBusca > 5 ||
            !guardaRonda.gameObject.activeInHierarchy) return;
        if (ocupado || menuAberto || dialogando || PauseHelper.BlockInput() || Time.timeScale <= 0f ||
            (InventoryTabController.Instance != null && InventoryTabController.Instance.IsOpen))
        { AnimarEscolta(guardaRonda, false); return; }
        if (pausaRonda > 0f) { pausaRonda -= Time.deltaTime; AnimarEscolta(guardaRonda, false); }
        else
        {
            Vector3 destino = rondaVaiAoFim ? rondaFim.position : rondaInicio.position;
            guardaRonda.position = Vector3.MoveTowards(guardaRonda.position, destino, velocidadeRonda * Time.deltaTime);
            AnimarEscolta(guardaRonda, true);
            SpriteRenderer visual = guardaRonda.GetComponent<SpriteRenderer>();
            if (visual != null) visual.flipX = !rondaVaiAoFim;
            if (Vector3.Distance(guardaRonda.position, destino) < .01f)
            { rondaVaiAoFim = !rondaVaiAoFim; pausaRonda = 1.2f; }
        }
        if (Dados.curraisEtapaBusca != 3 || Dados.curraisPassagemVigiadaConcluida) return;
        if (mari.transform.position.x >= fimPassagem.position.x)
        {
            Dados.curraisPassagemVigiadaConcluida = true;
            Salvar(); Iniciar(LembrarLenda());
            return;
        }
        float delta = mari.transform.position.x - guardaRonda.position.x;
        SpriteRenderer olhar = guardaRonda.GetComponent<SpriteRenderer>();
        bool olhaDireita = olhar != null ? !olhar.flipX : rondaVaiAoFim;
        bool naFrente = olhaDireita ? delta > 0f : delta < 0f;
        bool escondida = abrigoDosSacos.bounds.Contains(new Vector3(mari.transform.position.x,
            abrigoDosSacos.bounds.center.y, abrigoDosSacos.bounds.center.z));
        if (naFrente && Mathf.Abs(delta) < alcanceVigia &&
            mari.transform.position.x > retornoRonda.position.x + .5f && !escondida)
            Iniciar(VoltarDaRonda());
    }

    private IEnumerator VoltarDaRonda()
    {
        yield return Dizer(guardaRepreensao);
        // Retorno curto, sem perda de itens e mantendo o nível dos pés da personagem.
        Vector3 destino = retornoRonda.position;
        destino.y = mari.transform.position.y;
        yield return Mover(mari.transform, destino, tempoPlano);
        pausaRonda = 1.2f;
        objetivoTexto.text = "Espere junto dos sacos e observe para onde o guarda está olhando antes de atravessar.";
    }
}
