using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Pátio — Examine, pano do pai e preparação do apoio")]
    [SerializeField] private ExamineObject examePaiTrabalho, exameBentoTrabalho, exameFiscalTrabalho, exameApoioTrabalho;
    [SerializeField] private Collider2D areaPanoLimpoApoio;
    [SerializeField, Min(1f)] private float alcanceLevarAguaPatio = 5f;
    [SerializeField, Min(1f)] private float alcanceChegadaPatio = 7f;
    [SerializeField] private Fala[] patioChegadaMae, patioChegadaLuzia;
    [SerializeField] private Fala[] patioPaiAbertura, patioProtestoMae, patioProtestoLuzia, patioFiscalReplica;
    [SerializeField] private Fala[] patioApresentarBento, patioPaiEntregaPano, patioBentoAjuda, patioFiscalPedido;
    [SerializeField] private Fala[] patioApoioPedido, patioAguaRecebida, patioPanoEntregue, patioInseguranca;
    [SerializeField] private Fala[] patioLevarAgua, patioAntesQueda, patioDepoisQueda, patioPensamentoMari;
    [SerializeField] private Fala[] patioReacaoMae, patioReacaoLuzia, patioAutorizarPano;

    private string NomeAcompanhantePatio => Dados.curraisEscolhaAgua == 1 ? "Mãe" : "Luzia";
    private bool BentoApresentadoPatio => ViuConversaEntrada("patio-bento-apresentado") ||
        Dados.curraisTrabalhoPatioIniciado || Dados.curraisEtapaBusca >= 5;
    private bool ApoioPatioAutorizado => Dados.curraisTrabalhoPatioIniciado &&
        (ViuConversaEntrada("patio-apoio-autorizado") || Dados.curraisAguaPatioPreparada || Dados.curraisPanosPatioEntregues);
    private bool PaiNoAlcancePatio => Mathf.Abs(mari.transform.position.x - pai.position.x) <= alcanceLevarAguaPatio;
    private bool PanoDoApoioDisponivel => Dados.curraisEtapaBusca == 5 && !Dados.curraisBentoMorto &&
        Dados.curraisPanosPatioEntregues && !ViuConversaEntrada("patio-pano-retomado") && PrecisaRecolherPanoSocorro;

    private bool ReferenciasPatioLinearValidas()
    {
        bool valido = ConferirReferencia(areaPanoLimpoApoio, nameof(areaPanoLimpoApoio));
        foreach (ExamineObject exame in new[] { examePaiTrabalho, exameBentoTrabalho, exameFiscalTrabalho, exameApoioTrabalho })
            valido &= ConferirReferencia(exame, "Examine do pátio") &&
                ConferirReferencia(exame.examinePanel, "Painel de Examine do pátio") &&
                ConferirReferencia(exame.examineText, "Texto de Examine do pátio");
        foreach (Fala[] falas in new[] { patioChegadaMae, patioChegadaLuzia, patioPaiAbertura,
            patioProtestoMae, patioProtestoLuzia, patioFiscalReplica, patioApresentarBento,
            patioPaiEntregaPano, patioBentoAjuda, patioFiscalPedido, patioApoioPedido,
            patioAguaRecebida, patioPanoEntregue, patioInseguranca, patioLevarAgua, patioAntesQueda,
            patioDepoisQueda, patioPensamentoMari, patioReacaoMae, patioReacaoLuzia, patioAutorizarPano })
            valido &= FalasValidas(falas);
        return valido && panoAuxilio != tiraPanoLavagem;
    }

    private IEnumerator AbrirPaiPatioLinear()
    {
        yield return ExamineNomesUmaVez("patio-exame-pai", examePaiTrabalho);
        if (Dados.curraisEtapaBusca != 4) yield break;
        if (!Dados.curraisTrabalhoPatioIniciado)
            MostrarOpcoes("Pai", string.Empty, new[] { "Falar com o pai", "Sair" },
                new Action[] { () => Iniciar(TestemunharTrabalho()), FecharMenu });
        else MostrarRetornoPaiPatio();
    }

    private IEnumerator ConversarPaiPatioLinear()
    {
        if (Dados.curraisEtapaBusca != 4 || Dados.curraisTrabalhoPatioIniciado) yield break;
        yield return ComentarChegadaPatio();
        yield return DizerEntradaUmaVez("patio-pai-abertura", patioPaiAbertura);
        yield return DizerEntradaUmaVez("patio-protesto-acompanhante",
            Dados.curraisEscolhaAgua == 1 ? patioProtestoMae : patioProtestoLuzia);
        yield return DizerEntradaUmaVez("patio-fiscal-replica", patioFiscalReplica);
        yield return DizerEntradaUmaVez("patio-bento-apresentado", patioApresentarBento);
        yield return DizerEntradaUmaVez("patio-pai-entrega-pano", patioPaiEntregaPano);
        // Este pano é do pai. O retalho destinado à alça de Raimundo não é consumido.
        if (!TemItem(panoAuxilio)) InventoryManager.Instance.AddItem(panoAuxilio);
        Dados.curraisTrabalhoPatioIniciado = true;
        Dados.curraisPanosPatioEntregues = true; // Pano reservado no inventário, antes da queda.
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator AbrirBentoPatioLinear()
    {
        yield return ExamineNomesUmaVez("patio-exame-bento", exameBentoTrabalho);
        if (Dados.curraisEtapaBusca != 4 || !BentoApresentadoPatio) yield break;
        AbrirAssuntosEntrada("patio-bento-ajuda", "Bento", string.Empty,
            new[] { "patio-bento-ajuda" }, new[] { "O senhor precisa de ajuda?" },
            new Func<IEnumerator>[] { () => Dizer(patioBentoAjuda) });
    }

    private IEnumerator AbrirFiscalPatioLinear()
    {
        yield return ExamineNomesUmaVez("patio-exame-fiscal", exameFiscalTrabalho);
        if (Dados.curraisEtapaBusca != 4 || !Dados.curraisTrabalhoPatioIniciado) yield break;
        AbrirAssuntosEntrada("patio-fiscal-pedido", "Fiscal", string.Empty,
            new[] { "patio-fiscal-pedido" }, new[] { "Pedir que deixem o pai descansar" },
            new Func<IEnumerator>[] { () => Dizer(patioFiscalPedido) });
    }

    private IEnumerator AbrirApoioPatioLinear()
    {
        yield return ExamineNomesUmaVez("patio-exame-apoio", exameApoioTrabalho);
        if (Dados.curraisEtapaBusca != 4 || !Dados.curraisTrabalhoPatioIniciado) yield break;
        if (!ApoioPatioAutorizado)
        {
            MostrarOpcoes("Trabalhador", string.Empty, new[] { "Perguntar pelo apoio", "Sair" },
                new Action[] { () => Iniciar(AutorizarApoioPatioLinear()), FecharMenu });
            yield break;
        }
        MostrarEntregasDoApoio();
    }

    private void MostrarEntregasDoApoio()
    {
        var rotulos = new System.Collections.Generic.List<string>();
        var acoes = new System.Collections.Generic.List<Action>();
        if (!Dados.curraisPanosPatioEntregues && TemItem(panoAuxilio))
        { rotulos.Add("Entregar o pano do pai"); acoes.Add(() => Iniciar(EntregarPanoNoApoio())); }
        if (!Dados.curraisAguaPatioPreparada && TemItem(cabacaVaziaBusca))
        { rotulos.Add("Encher a cabaça"); acoes.Add(() => Iniciar(EncherCabacaNoApoio())); }
        rotulos.Add("Sair"); acoes.Add(FecharMenu);
        MostrarOpcoes("Trabalhador", Dados.curraisPanosPatioEntregues
            ? "O pano já tá aqui no apoio." : "Pode deixar o pano aqui no apoio.",
            rotulos.ToArray(), acoes.ToArray());
    }

    private IEnumerator AutorizarApoioPatioLinear()
    {
        if (ApoioPatioAutorizado || !Dados.curraisTrabalhoPatioIniciado) yield break;
        yield return Dizer(patioApoioPedido);
        RegistrarConversaEntrada("patio-apoio-autorizado"); Salvar();
    }

    private void ConversarAcompanhantePatioLinear()
    {
        if (Dados.curraisEtapaBusca != 4 || !Dados.curraisTrabalhoPatioIniciado) return;
        AbrirAssuntosEntrada("patio-inseguranca", NomeAcompanhantePatio, string.Empty,
            new[] { "patio-inseguranca" }, new[] { "Isso vai ajudar?" },
            new Func<IEnumerator>[] { () => Dizer(ComAcompanhante(patioInseguranca)) });
    }

    private void MostrarRetornoPaiPatio()
    {
        if (!PaiNoAlcancePatio || !Dados.curraisAguaPatioPreparada ||
            !Dados.curraisPanosPatioEntregues || !TemItem(cabacaCheiaBusca)) return;
        MostrarOpcoes("Pai", string.Empty, new[] { "Levar a água", "Sair" },
            new Action[] { () => Iniciar(AgressaoNoPatio()), FecharMenu });
    }

    private bool AtualizarAproximacaoPatio(bool livre)
    {
        if (!livre || Dados.curraisEtapaBusca != 4 || !Dados.curraisPanelaConcluida ||
            PainelAuxiliarAberto() || InventarioAbertoMemoria()) return false;
        if (!ViuConversaEntrada("patio-chegada") &&
            Mathf.Abs(mari.transform.position.x - pai.position.x) <= alcanceChegadaPatio)
        { Iniciar(ComentarChegadaPatio()); return true; }
        if (Dados.curraisAguaPatioPreparada && Dados.curraisPanosPatioEntregues &&
            PaiNoAlcancePatio && TemItem(cabacaCheiaBusca) && !ViuConversaEntrada("patio-retorno-oferecido"))
        {
            RegistrarConversaEntrada("patio-retorno-oferecido"); Salvar();
            MostrarRetornoPaiPatio(); return true;
        }
        return false;
    }

    private IEnumerator ComentarChegadaPatio() => DizerEntradaUmaVez("patio-chegada",
        Dados.curraisEscolhaAgua == 1 ? patioChegadaMae : patioChegadaLuzia);
}
