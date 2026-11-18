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
        return ReferenciasPassagemLinearValidas() && ReferenciasLavagemLinearValidas() && ok && FalasValidas(instrucoesLavagem) && FalasValidas(tiraPanoAutorizada) && FalasValidas(guardaRepreensao) && FalasValidas(raimundoTrabalho) && ReferenciasEncenacaoLavagemValidas();
    }

    private void PrepararEtapaLavagem()
    {
        // Saves anteriores já receberam as instruções antes de entrar nesta etapa.
        if (Dados.curraisEtapaBusca >= 2) RegistrarConversaEntrada("lavagem-instrucoes");
        if (Dados.curraisEtapaBusca >= 3)
        {
            Dados.curraisPanosSeparados = 3;
            if (!Dados.curraisTiraPanoRecolhida && !ConsultaDisponivel &&
                !Dados.curraisBaldeApoiado && !TemItem(baldeProtegidoItem))
            {
                if (!TemItem(tiraPanoLavagem)) InventoryManager.Instance.AddItem(tiraPanoLavagem);
                Dados.curraisTiraPanoRecolhida = true;
            }
        }
        if (Dados.curraisEtapaBusca >= 4) Dados.curraisPassagemVigiadaConcluida = true;
        if (Dados.curraisEtapaBusca == 2 && Dados.curraisTiraPanoRecolhida)
        {
            Dados.curraisEtapaBusca = 3;
            Dados.curraisPanosSeparados = 3;
        }
        // Uma lembrança concluída não deve ser reaberta por um checkpoint antigo.
        // Se houve interrupção antes do fim, o indicador permanece falso e a sequência retoma.
        if (Dados.curraisEtapaBusca == 3 && Dados.curraisPassagemVigiadaConcluida)
            Dados.curraisEtapaBusca = 4;
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
        guardaRonda.gameObject.SetActive(true);
        if (vigiaAtiva && !rondaPreparada)
        {
            // A ronda começa da posição existente, sem reaparecer em outra marca.
            rondaVaiAoFim = false;
            rondaPreparada = true;
        }
        AtualizarDetalhesLavagem();
    }

    private IEnumerator SepararPanoLavagem(bool atendimento)
    {
        if (Dados.curraisEtapaBusca != 2 || !ViuConversaEntrada("lavagem-instrucoes")) yield break;
        ItemData esperado = atendimento ? panoAtendimentoLavagem : panoUsoLavagem;
        ItemData selecionado = InventoryManager.Instance.SelectedItem;
        if (selecionado != esperado || !TemItem(esperado))
        { yield return CorrigirPilhaLavagem(selecionado); yield break; }
        int bit = atendimento ? 2 : 1;
        if ((Dados.curraisPanosSeparados & bit) != 0) yield break;
        bool primeira = Dados.curraisPanosSeparados == 0;
        yield return LavarPecaLavagem(atendimento);
        if (!TemItem(esperado)) yield break;
        Dados.curraisPanosSeparados |= bit;
        InventoryManager.Instance.RemoveItem(esperado);
        AtualizarEstadoBusca(); Salvar();
        if (primeira) yield return Dizer(atendimento ? lavagemAtendimentoRecebido : lavagemUsoRecebido);
        if (Dados.curraisPanosSeparados == 3) yield return ConcluirLavagemLinear();
    }

    private IEnumerator RecolherTiraLavagem()
    {
        if (!RetalhoAutorizado || Dados.curraisTiraPanoRecolhida) yield break;
        yield return ExaminarOuColetar(tiraPanoLavagem);
        if (!coletaDoCenarioConfirmada) yield break;
        Dados.curraisTiraPanoRecolhida = true;
        AvancarCampo(3);
        yield return Dizer(Dados.curraisEscolhaAgua == 1 ? saidaLavagemMae : saidaLavagemLuzia);
    }

    private void AtualizarPatrulha()
    {
        if (!Dados.curraisPanelaConcluida || Dados.curraisEtapaBusca < 3 || Dados.curraisEtapaBusca > 5 ||
            !guardaRonda.gameObject.activeInHierarchy) return;
        if (ocupado || menuAberto || dialogando || PainelAuxiliarAberto() || PauseHelper.BlockInput() || Time.timeScale <= 0f ||
            (InventoryTabController.Instance != null && InventoryTabController.Instance.IsOpen))
        { AnimarEscolta(guardaRonda, false); return; }
        if (pausaRonda > 0f) { pausaRonda -= Time.deltaTime; AnimarEscolta(guardaRonda, false); }
        else
        {
            Vector3 destino = rondaVaiAoFim ? rondaFim.position : rondaInicio.position;
            destino.y = guardaRonda.position.y;
            float direcao = destino.x - guardaRonda.position.x;
            guardaRonda.position = Vector3.MoveTowards(guardaRonda.position, destino, velocidadeRonda * Time.deltaTime);
            AnimarEscolta(guardaRonda, true);
            SpriteRenderer visual = guardaRonda.GetComponent<SpriteRenderer>();
            if (visual != null && Mathf.Abs(direcao) > .01f)
                visual.flipX = direcao * guardaRonda.lossyScale.x < 0f;
            if (Vector3.Distance(guardaRonda.position, destino) < .01f)
            { rondaVaiAoFim = !rondaVaiAoFim; pausaRonda = 1.2f; }
        }
        if (Dados.curraisEtapaBusca != 3 || !Dados.curraisTiraPanoRecolhida || Dados.curraisPassagemVigiadaConcluida) return;
        if (mari.transform.position.x >= inicioLembranca.position.x)
        {
            Iniciar(LembrarLenda());
            return;
        }

    }

    private IEnumerator VoltarDaRonda()
    {
        yield break;
    }
}
