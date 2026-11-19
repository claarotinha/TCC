using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Última consulta — Examine da mesa")]
    [SerializeField] private ExamineObject exameMesaUltimaConsulta;
    [SerializeField] private PuzzleRegistro puzzleRegistro;

    private bool FamiliaNaConsulta => Dados.curraisConsultaApresentada || ViuConversaEntrada("consulta-familia-reunida");
    private bool MaeNaConsulta => Dados.curraisEscolhaAgua == 1 ||
        ViuConversaEntrada("consulta-familia-apos-semanas") || ViuConversaEntrada("consulta-familia-portao");

    private IEnumerator PrepararMesaConsulta()
    {
        if (ViuConversaEntrada("consulta-familia-reunida") || Dados.curraisEscolhaConsulta != 0) yield break;
        yield return ChapterCard.Show("josefa-10");
        yield return FalarPosto("Pai", "Vou perguntar contigo agora.", "Zefinha", "Vamos, painho.");
        if (Dados.curraisEscolhaAgua == 1)
            yield return FalarPosto("Mãe", "Eu vou com vocês.", "Luzia", "Eu guardo o lugar aqui.");
        else yield return FalarPosto("Luzia", "Vou ficar com tua mãe. Depois vocês contam o que conseguiram.",
            "Zefinha", "Nós volta pra cá.");
        Posicionar(luzia.transform, luziaRepouso);
        yield return Mover(pai, mari.transform.position + Vector3.left * seguidorPaiConsulta.DistanciaAtras, tempoPlano);
        if (Dados.curraisEscolhaAgua == 1)
            yield return Mover(mae, mari.transform.position + Vector3.left * (seguidoraMae.DistanciaAtras + 1f), tempoPlano);
        else Posicionar(mae, maeAbrigo);
        RegistrarConversaEntrada("consulta-familia-reunida");
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator ClicarMesaUltimaConsulta()
    {
        if (Dados.curraisEscolhaConsulta != 0)
        {
            yield return ExaminarPosto("", exameMesaUltimaConsulta, "A relação foi levada. A consulta daquela manhã terminou.");
            yield break;
        }
        if (Dados.curraisEtapaConsulta != 1 || !areaUltimaConsulta.enabled) yield break;
        yield return PrepararMesaConsulta();
        if (!ViuConversaEntrada("consulta-ordens-ouvidas"))
        {
            yield return Dizer(manhaOrdens);
            RegistrarConversaEntrada("consulta-ordens-ouvidas"); Salvar();
        }
        if (!ViuConversaEntrada("consulta-mesa-examinada"))
            yield return ExaminarPosto("consulta-mesa-examinada", exameMesaUltimaConsulta,
                "As folhas do registro estão misturadas sobre a mesa. O escrivão espera que Zefinha organize a sequência.");
        yield return OrganizarFolhasRegistro();
        if (!RegistroOrganizado) yield break;
        if (Dados.curraisConsultaApresentada) { yield return ConsultaFinalLinear(); yield break; }
        MostrarAssuntosEscrivao();
    }

    private void MostrarAssuntosEscrivao()
    {
        if (Dados.curraisEscolhaConsulta != 0) return;
        var rotulos = new List<string> { "Perguntar por Mariquinha" };
        var acoes = new List<Action> { () => Iniciar(FazerUltimaConsulta()) };
        if (TemItem(itemCanhoto) && !ViuConversaEntrada("consulta-canhoto-mostrado"))
        { rotulos.Add("Mostrar o canhoto"); acoes.Add(() => Iniciar(ApresentarCanhotoConsulta())); }
        rotulos.Add("Sair"); acoes.Add(FecharMenu);
        MostrarOpcoes("Escrivão", string.Empty, rotulos.ToArray(), acoes.ToArray());
    }

    private IEnumerator MostrarCanhotoLinear()
    {
        if (Dados.curraisEtapaConsulta != 1 || Dados.curraisEscolhaConsulta != 0 ||
            !TemItem(itemCanhoto) || ViuConversaEntrada("consulta-canhoto-mostrado")) yield break;
        yield return PrepararMesaConsulta();
        papelEmConsulta = true; canhotoApresentadoConsulta.SetActive(true);
        try { yield return Dizer(papelApresentado); }
        finally { EncerrarDetalhesConsulta(); }
        RegistrarConversaEntrada("consulta-canhoto-mostrado"); Salvar();
        MostrarAssuntosEscrivao();
    }

    private IEnumerator ConsultaFinalLinear()
    {
        if (Dados.curraisEtapaConsulta != 1 || Dados.curraisEscolhaConsulta != 0) yield break;
        yield return PrepararMesaConsulta();
        if (!ViuConversaEntrada("consulta-ordens-ouvidas"))
        {
            yield return Dizer(manhaOrdens);
            RegistrarConversaEntrada("consulta-ordens-ouvidas"); Salvar();
        }
        if (!ViuConversaEntrada("consulta-mesa-examinada"))
            yield return ExaminarPosto("consulta-mesa-examinada", exameMesaUltimaConsulta,
                "As folhas do registro estão misturadas sobre a mesa. O escrivão espera que Zefinha organize a sequência.");
        yield return OrganizarFolhasRegistro();
        if (!RegistroOrganizado) yield break;
        if (!Dados.curraisConsultaApresentada)
        {
            yield return FalarPosto("Pai", "Separaram minha filha da irmã na entrada. Nós tá tentando saber pra onde levaram ela.");
            yield return Dizer(consultaEscrivao);
            Dados.curraisConsultaApresentada = true; Salvar();
        }
        string[] rotulos = (string[])opcoesConsulta.Clone();
        if (Dados.curraisDestinoConfirmado) rotulos[0] += " (já confirmado)";
        if (Dados.curraisIdentidadeConfirmada) rotulos[1] += " (já confirmado)";
        yield return Escolher("Escrivão", InformacoesConsulta() + "\n" + perguntaConsulta, rotulos);
        int opcao = escolhaRecebida;
        if (opcao < 0 || opcao > 1) yield break;
        bool conhecida = opcao == 0 ? Dados.curraisDestinoConfirmado : Dados.curraisIdentidadeConfirmada;
        if (opcao == 0)
        {
            yield return Esperar(.5f);
            yield return Dizer(consultaDestino);
            if (conhecida) yield return FalarPosto("Zefinha", "É o mesmo lugar que nós tinha ouvido.", "Pai", "Agora nós viu na relação também.");
        }
        else
        {
            testemunhaConsulta.SetActive(true);
            yield return Dizer(consultaNome);
            if (conhecida) yield return FalarPosto("Zefinha", "Foi esse nome que seu Raimundo conseguiu.", "Trabalhadora", "Eu conheço ela. É esse o nome.");
        }
        yield return Dizer(consultaConcluida);
        yield return FalarPosto("Zefinha", "Mas eu ainda num sei onde minha irmã tá agora.",
            "Escrivão", "Num tenho outra informação aqui.",
            "Pai", "Então vamos pedir pra sair. Nós mesmo procura por ela.");
        Dados.curraisEscolhaConsulta = opcao + 1;
        if (opcao == 0) Dados.curraisDestinoConfirmado = true;
        else Dados.curraisIdentidadeConfirmada = true;
        Dados.curraisEtapaConsulta = 2;
        GameProgress.Instance.UnlockDiaryPage(opcao == 0 ? "currais-ultima-destino" : "currais-ultima-nome");
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator ReunirFamiliaPortao()
    {
        if (ViuConversaEntrada("consulta-familia-portao")) yield break;
        if (MaeNaConsulta)
            yield return FalarPosto("Pai", "Vamos falar com o cabo.", "Mãe", "Nós vai junto.");
        else
        {
            yield return Escurecer(1f);
            Posicionar(pai, paiAbrigo); Posicionar(mae, maeAbrigo); Posicionar(luzia.transform, luziaRepouso);
            Vector3 perto = pai.position + Vector3.left; perto.y = mari.transform.position.y;
            mari.transform.position = perto; Physics2D.SyncTransforms();
            yield return Escurecer(0f);
            yield return FalarPosto("Pai", "Vamos pedir pra sair e procurar nossa menina.",
                "Mãe", "Eu vou com vocês.", "Luzia", "Eu fico aqui com Baleia.");
        }
        RegistrarConversaEntrada("consulta-familia-portao"); Salvar();
    }

    private bool AtualizarChegadaAbrigoFinal()
    {
        if (ocupado || menuAberto || InventarioAbertoMemoria() || !ConsultaDisponivel ||
            Dados.curraisEtapaConsulta != 3 || ViuConversaEntrada("consulta-retorno-chegada")) return false;
        if (Vector2.Distance(areaRetornoConsulta.ClosestPoint(mari.transform.position), mari.transform.position) > alcanceInteracao) return false;
        Iniciar(ChegarAbrigoFinal()); return true;
    }

    private IEnumerator ChegarAbrigoFinal()
    {
        if (!ViuConversaEntrada("consulta-retorno-chegada"))
        {
            yield return Escurecer(1f);
            yield return ChapterCard.Show("josefa-11");
            ReunirFamiliaAbrigoFinal(true);
            AplicarNoite(false);
            Physics2D.SyncTransforms();
            seguirCamera.RestaurarEnquadramentoDoAlvo();
            yield return Escurecer(0f);
            yield return FalarPosto("Luzia", "Sente um pouco. Conseguiram ouvir alguma coisa?",
                "Pai", "Ouviram nós no portão. Sair, num deixaram.");
            RegistrarConversaEntrada("consulta-retorno-chegada"); Salvar();
        }
        if (ViuConversaEntrada("consulta-relato-final"))
            yield return DizerEntradaUmaVez("encerramento-abertura", aberturaEncerramento);
        MenuRelatoFinal();
    }

    private void ReunirFamiliaAbrigoFinal(bool incluirZefinha)
    {
        seguidorPaiConsulta.enabled = false;
        seguidoraMae.enabled = false;
        seguidoraLuzia.enabled = false;
        EncerrarNoiteFamilia();
        if (incluirZefinha) Posicionar(mari.transform, mariAbrigo);
        Posicionar(pai, paiAbrigo); Posicionar(mae, maeAbrigo);
        Posicionar(baleia, baleiaAbrigo); Posicionar(luzia.transform, luziaRepouso);
        // Mantém todos no mesmo plano junto do barracão, sem espalhar a família.
        Vector3 ponto = pai.position; ponto.x = mariAbrigo.position.x - 2f; pai.position = ponto;
        ponto = mae.position; ponto.x = mariAbrigo.position.x + 2f; mae.position = ponto;
        ponto = baleia.position; ponto.x = mariAbrigo.position.x - 3.5f; baleia.position = ponto;
        Transform[] pessoas = { mari.transform, pai, mae, baleia, luzia.transform };
        foreach (Transform pessoa in pessoas)
        {
            pessoa.gameObject.SetActive(true);
            SpriteRenderer desenho = pessoa.GetComponent<SpriteRenderer>();
            if (desenho != null) { desenho.enabled = true; Color cor = desenho.color; cor.a = 1f; desenho.color = cor; }
        }
        foreach (SpriteRenderer desenho in personagensNoAbrigo)
            if (desenho != null) desenho.enabled = true;
        foreach (SpriteRenderer pose in posesDeRepouso)
            if (pose != null) pose.gameObject.SetActive(false);
    }

    private void MenuRelatoFinal()
    {
        if (ViuConversaEntrada("consulta-relato-final")) { MenuNomesFinais(); return; }
        MenuAssuntoNPC("Luzia", string.Empty, "Contar o que ficou confirmado", ConcluirRetorno);
    }

    private IEnumerator RelatoFinalLinear()
    {
        if (Dados.curraisEtapaConsulta != 3 || ViuConversaEntrada("consulta-relato-final")) yield break;
        if (!ViuConversaEntrada("negacao-relato-contado"))
        {
            if (Dados.curraisDestinoConfirmado && Dados.curraisIdentidadeConfirmada) yield return Dizer(retornoDuasPistas);
            else if (Dados.curraisDestinoConfirmado) yield return Dizer(retornoDestino);
            else if (Dados.curraisIdentidadeConfirmada) yield return Dizer(retornoNome);
            if (TemCadernoNaNoite)
                yield return FalarPosto("Zefinha", "Meu caderno tá comigo. Vou guardar o que conseguimos saber.",
                    "Mãe", "E eu guardo contigo. Tu num precisa carregar isso sozinha.");
            else if (TemItem(anotacaoPosto))
                yield return FalarPosto("Zefinha", "O papel tá comigo. O resto eu vou lembrando.", "Mãe", "Me conte também. Nós guarda junto.");
            else yield return FalarPosto("Zefinha", "Eu num quero esquecer o que disseram.", "Mãe", "Conte pra nós. Tu num precisa lembrar sozinha.");
            yield return FalarPosto("Zefinha", "Eu prometi que ia buscar ela, mainha. E agora nem deixam eu sair.",
                "Mãe", "Eu sei, minha filha.", "Zefinha", "E se ela achar que eu esqueci?",
                "Pai", "Tu num esqueceu. Nós sabe o quanto tu procurou.",
                "Zefinha", "Mas ela num sabe, painho. Ela num sabe.");
            yield return Esperar(2f);
            RegistrarConversaEntrada("negacao-relato-contado"); Salvar();
        }
        yield return NegarFimDaBusca();
        if (!NegacaoAcolhida) yield break;
        RegistrarConversaEntrada("consulta-relato-final");
        AtualizarEstadoBusca(); Salvar();
        yield return DizerEntradaUmaVez("encerramento-abertura", aberturaEncerramento);
    }

    private bool RegistroOrganizado => ViuConversaEntrada("consulta-registro-visual-organizado");

    private IEnumerator OrganizarFolhasRegistro()
    {
        if (RegistroOrganizado) yield break;
        yield return DizerEntradaUmaVez("consulta-puzzle-apresentado", new[] {
            new Fala { personagem = "Escrivão", texto = "Essas folhas se misturaram. Separa a ordem dos registros enquanto eu procuro o anexo." }
        });
        FecharDialogo();
        menuAberto = false;
        InventoryTabController inventario = InventoryTabController.Instance;
        bool inventarioHabilitado = inventario != null && inventario.enabled;
        if (inventario != null) { inventario.Close(); inventario.enabled = false; }
        bool concluido = false;
        try
        {
            puzzleRegistro.Abrir(Dados.curraisOrdemFolhasRegistro, ordem => {
                Dados.curraisOrdemFolhasRegistro = ordem; Salvar();
            });
            while (puzzleRegistro.Aberto) yield return null;
            concluido = puzzleRegistro.Concluido;
        }
        finally
        {
            puzzleRegistro.Fechar();
            if (inventario != null) inventario.enabled = inventarioHabilitado;
            quadroBloqueado = Time.frameCount;
            InvestigationGuard.BlockCurrentClick();
        }
        if (!concluido) yield break;
        RegistrarConversaEntrada("consulta-registro-visual-organizado"); Salvar();
        yield return FalarPosto("Zefinha", "Foi depois de separar ela de nós que mandaram o grupo embora.",
            "Escrivão", "É essa a sequência. Agora posso conferir o anexo ou chamar quem viu a menina.");
    }
}
