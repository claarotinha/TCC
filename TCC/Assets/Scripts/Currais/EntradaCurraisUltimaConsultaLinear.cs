using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Última consulta — Examine da mesa")]
    [SerializeField] private ExamineObject exameMesaUltimaConsulta;

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
        yield return ExamineNomesUmaVez("consulta-mesa-examinada", exameMesaUltimaConsulta);
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
        yield return ExamineNomesUmaVez("consulta-mesa-examinada", exameMesaUltimaConsulta);
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
            Posicionar(mari.transform, mariNoite); Posicionar(pai, paiNoite);
            Posicionar(mae, maeNoite); Posicionar(luzia.transform, luziaNoite);
            AplicarNoite(true);
            Physics2D.SyncTransforms();
            yield return Escurecer(0f);
            yield return FalarPosto("Luzia", "Sente um pouco. Conseguiram ouvir alguma coisa?",
                "Pai", "Ouviram nós no portão. Sair, num deixaram.");
            RegistrarConversaEntrada("consulta-retorno-chegada"); Salvar();
        }
        if (ViuConversaEntrada("consulta-relato-final"))
            yield return DizerEntradaUmaVez("encerramento-abertura", aberturaEncerramento);
        MenuRelatoFinal();
    }

    private void MenuRelatoFinal()
    {
        if (ViuConversaEntrada("consulta-relato-final")) { MenuNomesFinais(); return; }
        MenuAssuntoNPC("Luzia", string.Empty, "Contar o que ficou confirmado", ConcluirRetorno);
    }

    private IEnumerator RelatoFinalLinear()
    {
        if (Dados.curraisEtapaConsulta != 3 || ViuConversaEntrada("consulta-relato-final")) yield break;
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
        RegistrarConversaEntrada("consulta-relato-final");
        AtualizarEstadoBusca(); Salvar();
        yield return DizerEntradaUmaVez("encerramento-abertura", aberturaEncerramento);
    }

    private bool RegistroOrganizado => ViuConversaEntrada("consulta-registro-organizado");

    // As seis ordens possíveis permitem guardar e retomar o puzzle no save existente.
    private static readonly int[][] ordensFolhasRegistro = {
        new[] { 2, 0, 1 }, new[] { 0, 1, 2 }, new[] { 0, 2, 1 },
        new[] { 1, 0, 2 }, new[] { 1, 2, 0 }, new[] { 2, 1, 0 }
    };
    private static readonly string[] folhasRegistro = {
        "ADMISSÃO — Família recebida; irmãs juntas.",
        "TRIAGEM — Menina separada com acompanhante.",
        "REMESSA — Grupo enviado; destino no anexo."
    };

    private string TextoFolhasRegistro()
    {
        int[] ordem = ordensFolhasRegistro[Mathf.Clamp(Dados.curraisOrdemFolhasRegistro, 0, 5)];
        return "Organize os registros, do primeiro ao último.\n\n" +
            "1. " + folhasRegistro[ordem[0]] + "\n" +
            "2. " + folhasRegistro[ordem[1]] + "\n" +
            "3. " + folhasRegistro[ordem[2]];
    }

    private void TrocarFolhasRegistro(int primeiro, int segundo)
    {
        int[] ordem = (int[])ordensFolhasRegistro[Mathf.Clamp(Dados.curraisOrdemFolhasRegistro, 0, 5)].Clone();
        int folha = ordem[primeiro]; ordem[primeiro] = ordem[segundo]; ordem[segundo] = folha;
        Dados.curraisOrdemFolhasRegistro = Array.FindIndex(ordensFolhasRegistro,
            candidata => candidata[0] == ordem[0] && candidata[1] == ordem[1] && candidata[2] == ordem[2]);
        Salvar();
    }

    private IEnumerator OrganizarFolhasRegistro()
    {
        if (RegistroOrganizado) yield break;
        yield return DizerEntradaUmaVez("consulta-puzzle-apresentado", new[] {
            new Fala { personagem = "Escrivão", texto = "Essas folhas se misturaram. Separa a ordem dos registros enquanto eu procuro o anexo." },
            new Fala { personagem = "Zefinha (em pensamento)", texto = "Primeiro nós chegou junto. Foi depois que separaram Mariquinha. Preciso seguir o caminho que fizeram com ela." }
        });
        while (!RegistroOrganizado)
        {
            yield return Escolher("Mesa do registro", TextoFolhasRegistro(),
                new[] { "Trocar as folhas", "Conferir a ordem", "Sair" });
            int acao = escolhaRecebida;
            if (acao == 2) { FecharMenu(); yield break; }
            if (acao == 0)
            {
                yield return Escolher("Mesa do registro", TextoFolhasRegistro(),
                    new[] { "Trocar 1 e 2", "Trocar 2 e 3", "Voltar" });
                if (escolhaRecebida == 0) TrocarFolhasRegistro(0, 1);
                else if (escolhaRecebida == 1) TrocarFolhasRegistro(1, 2);
            }
            else if (acao == 1)
            {
                if (Dados.curraisOrdemFolhasRegistro == 1)
                {
                    yield return FalarPosto("Zefinha", "Admissão, triagem e remessa... Foi depois de separar ela de nós que mandaram o grupo embora.",
                        "Escrivão", "É essa a sequência. Agora posso conferir o anexo ou chamar quem viu a menina.");
                    RegistrarConversaEntrada("consulta-registro-organizado"); Salvar();
                }
                else
                    yield return FalarPosto("Zefinha (em pensamento)",
                        "Ainda num tá certo. A família chegou junta. A separação veio depois, e só então o grupo foi mandado embora.");
            }
        }
    }

}
