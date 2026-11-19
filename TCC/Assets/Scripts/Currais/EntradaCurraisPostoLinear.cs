using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Posto — observação e anotação avulsa")]
    [SerializeField] private ExamineObject exameMesaPosto, exameRaimundoPosto;
    [SerializeField] private Collider2D areaAnotacaoPosto;
    [SerializeField] private ItemData anotacaoPosto;
    private bool recipientePostoGuardado, recipientePostoApoiado;
    private Vector3 recipientePostoOrigem;

    private bool PapelAvulsoPostoPendente => Dados.curraisEscolhaPosto == 2 &&
        Dados.curraisDiarioRecusado && !ViuConversaEntrada("posto-anotacao-recolhida") && !TemItem(anotacaoPosto);

    private IEnumerator FalarPosto(params string[] falas)
    {
        Fala[] linhas = new Fala[falas.Length / 2];
        for (int i = 0; i < linhas.Length; i++)
            linhas[i] = new Fala { personagem = falas[i * 2], texto = falas[i * 2 + 1] };
        yield return Dizer(linhas);
    }

    private IEnumerator RetomarBuscaPosto()
    {
        if (ViuConversaEntrada("posto-retorno")) yield break;
        yield return Esperar(tempoPlano);
        yield return FalarPosto("Zefinha (em pensamento)",
            "Raimundo tá no posto, junto da mesa dos papéis. Vou perguntar pra ele.");
        RegistrarConversaEntrada("posto-retorno"); Salvar();
    }

    private IEnumerator ExaminarPosto(string id, ExamineObject exame, string mensagem = null)
    {
        if (mensagem == null && ViuConversaEntrada(id)) yield break;
        string anterior = exame.message;
        if (mensagem != null) exame.message = mensagem;
        // Reutiliza o fechamento e o bloqueio de clique do Examine existente.
        try
        {
            if (mensagem != null)
            {
                FecharDialogo(); examineNomesAtual = exame; quadroExamineNomes = Time.frameCount;
                exame.ShowPanel();
                while (exame.PanelIsOpen) yield return null;
                examineNomesAtual = null;
            }
            else yield return ExamineNomesUmaVez(id, exame);
        }
        finally { exame.message = anterior; }
    }

    private IEnumerator ExaminarMesaPosto()
    {
        if (Dados.curraisEscolhaPosto != 0)
        {
            yield return ExaminarPosto("", exameMesaPosto, Dados.curraisEscolhaPosto == 1 ?
                "A folha foi recolhida enquanto vocês apoiavam o recipiente." :
                "O funcionário levou o original. A informação copiada ficou com Zefinha.");
            yield break;
        }
        yield return RetomarBuscaPosto();
        yield return ExaminarPosto("posto-mesa-examinada", exameMesaPosto);
    }

    private void ClicarRaimundoPosto()
    {
        if (Dados.curraisEscolhaPosto != 0) return;
        Iniciar(ApresentarRaimundoPosto());
    }

    private IEnumerator ApresentarRaimundoPosto()
    {
        yield return RetomarBuscaPosto();
        yield return ExaminarPosto("posto-raimundo-examinado", exameRaimundoPosto);
        MenuAssuntoNPC("Raimundo", "Você chegou numa hora apertada, menina.", "O que houve?", EscolherNoPosto);
    }

    private void AtualizarCompanhiaPosto()
    {
        if (!PostoDisponivel || ConsultaDisponivel || Dados.curraisEtapaPosto > 2 || VoltouDoQuarto) return;
        seguidoraLuzia.enabled = false;
        seguidoraMae.enabled = false;
        Posicionar(luzia.transform, luziaRepouso);
        if (!PaisNoFuneral)
        {
            Posicionar(pai, paiAbrigo);
            Posicionar(mae, maeAbrigo);
        }
    }

    private void AtualizarRecipientePosto()
    {
        if (!recipientePostoGuardado)
        {
            recipientePostoOrigem = baldesApoiadosPosto.transform.position;
            recipientePostoGuardado = true;
        }
        bool apoiado = recipientePostoApoiado || Dados.curraisEscolhaPosto != 0;
        baldesApoiadosPosto.transform.position = recipientePostoOrigem +
            (apoiado ? Vector3.zero : Vector3.up * .45f);
        areaAnotacaoPosto.enabled = PostoDisponivel && PapelAvulsoPostoPendente;
    }

    private IEnumerator ApoiarRecipientePosto(bool zefinhaAjuda)
    {
        if (zefinhaAjuda)
            yield return Mover(mari.transform, baldesApoiadosPosto.transform.position + Vector3.left * .7f, .45f);
        yield return DeslocarFolhaRegistro(baldesApoiadosPosto.transform, recipientePostoOrigem, .65f);
        recipientePostoApoiado = true;
    }

    private IEnumerator RecolherFolhaPosto()
    {
        yield return DeslocarFolhaRegistro(papelPosto.transform,
            papelPostoLeitura.position + Vector3.right * 1.5f, .5f);
        papelPosto.SetActive(false);
    }

    private IEnumerator EscolherPostoLinear()
    {
        if (Dados.curraisEscolhaPosto != 0) yield break;
        yield return RetomarBuscaPosto();
        yield return ExaminarPosto("posto-raimundo-examinado", exameRaimundoPosto);
        yield return ExaminarPosto("posto-mesa-examinada", exameMesaPosto);
        yield return FalarPosto("Zefinha", "O senhor num consegue baixar isso?",
            "Raimundo", "Se eu soltar de uma vez, derrama.", "Zefinha", "Espere. Eu ajudo.",
            "Funcionário", "Vou recolher estas folhas.", "Zefinha", Dados.curraisDestinoConfirmado ?
            "Aquele papel tem a marca do carro. Pode confirmar o que disseram pra nós." :
            "Aquele papel tem a marca do carro que levou o outro grupo.",
            "Raimundo", "É dos encaminhamentos. Se precisa olhar, pergunte agora.");
        yield return Escolher("Zefinha", string.Empty,
            new[] { "Ajudar Raimundo", "Copiar o registro", "Sair" });
        int opcao = escolhaRecebida;
        if (opcao < 0 || opcao > 1) yield break;
        bool destinoAnterior = Dados.curraisDestinoConfirmado;
        if (opcao == 0)
        {
            yield return FalarPosto("Zefinha", "Eu seguro desse lado.",
                "Raimundo", "Devagar. Num bote o pé embaixo.", "Zefinha", "Pode baixar. Eu tô segurando.");
            yield return ApoiarRecipientePosto(true);
            yield return FalarPosto("Zefinha", "A mão abriu de novo?", "Raimundo", "Abriu. Obrigado por num me deixar largar tudo.");
            if (Dados.curraisBaldeApoiado)
                yield return FalarPosto("Zefinha", "Até com o pano tá machucando?", "Raimundo", "Hoje foi o peso. A mão ainda num fechou.");
            else if (TemItem(tiraPanoLavagem))
                yield return FalarPosto("Zefinha", "A lavadeira me deu um retalho pra essa alça.", "Raimundo", "Guarde. Depois nós ajeita.");
            yield return RecolherFolhaPosto();
            yield return FalarPosto("Raimundo", "Nair Pereira. A lavadeira confirmou. Tua irmã tava com ela.",
                "Zefinha", "E pra onde Nair foi?", "Raimundo", "Isso eu num vi. Mas agora tu tem um nome pra perguntar.");
            if (destinoAnterior) yield return FalarPosto("Zefinha", "Levaram a folha, mas eu sei o lugar. Lagoa do Meio.",
                "Raimundo", "Ainda dá pra perguntar por esse lugar.");
            else yield return FalarPosto("Zefinha", "Agora levaram o papel…", "Raimundo", "Num era o único encaminhamento. Vamos procurar outra notícia.");
        }
        else
        {
            yield return FalarPosto("Zefinha", "Espere, moço. Posso copiar esse lugar?", "Funcionário", "Copie logo. A folha vai comigo.");
            yield return Mover(mari.transform, papelPostoLeitura.position + Vector3.left * .6f, .45f);
            yield return LerFolhaDoPosto();
            yield return FalarPosto("Zefinha", "Lagoa do Meio…", "Zefinha", destinoAnterior ?
                "É o mesmo lugar do canhoto." : "Foi pra lá que mandaram o grupo.",
                "Zefinha (em pensamento)", "Só tem o lugar e o grupo. Cadê o nome de Mariquinha?");
            yield return ApoiarRecipientePosto(false);
            yield return FalarPosto("Zefinha", "Desculpe, seu Raimundo. Eu precisava olhar antes que levassem.",
                "Raimundo", "Eu sei, menina. Conseguiu ler?", "Zefinha", "Lagoa do Meio.", "Raimundo", "Então guarde esse nome.");
            yield return RecolherFolhaPosto();
        }
        yield return IndicarQuartoPosto();
        // A mudança de setor é mostrada antes de registrar a nova posição.
        yield return Mover(areaRaimundo.transform, (opcao == 0 ? raimundoApoiadoPosto : raimundoOutroSetor).position, tempoPlano);
        Dados.curraisEscolhaPosto = opcao + 1;
        Dados.curraisEtapaPosto = 1;
        if (opcao == 0) Dados.curraisIdentidadeConfirmada = true;
        else
        {
            Dados.curraisDestinoConfirmado = true;
            if (!Dados.curraisDiarioRecusado) GameProgress.Instance.UnlockDiaryPage("currais-copia");
        }
        AtualizarEstadoBusca(); Salvar();
        if (PapelAvulsoPostoPendente) yield return RecolherAnotacaoPosto();
    }

    private IEnumerator RecolherAnotacaoPosto()
    {
        if (!PapelAvulsoPostoPendente) yield break;
        yield return ExaminarOuColetar(anotacaoPosto);
        if (coletaDoCenarioConfirmada) { RegistrarConversaEntrada("posto-anotacao-recolhida"); Salvar(); }
        AtualizarDetalhesPosto();
    }

    private IEnumerator IndicarQuartoPosto()
    {
        if (ViuConversaEntrada("posto-quarto-indicado")) yield break;
        yield return FalarPosto("Zefinha", "Tem mais algum lugar onde eu possa perguntar pela minha irmã?",
            "Raimundo", "Tem um quarto de atendimento logo à esquerda da mesa. Pergunte lá.");
        RegistrarConversaEntrada("posto-quarto-indicado");
    }

    private void MenuNoticiasPosto()
    {
        // A notícia pode ter sido salva antes de a conversa avançar para a noite.
        // Retoma a conclusão sem repetir as falas já registradas.
        if (Dados.curraisEtapaPosto != 2) { MenuAbrigo(); return; }
        MenuAssuntoNPC("Pai", string.Empty, ViuConversaEntrada("quarto-noticia-contada") ?
            "Continuar a conversa" : "Contar o que descobriu", ContarAosPais);
    }

    private IEnumerator NoticiasPostoLinear()
    {
        string id = "posto-noticia-destino-" + Dados.curraisDestinoConfirmado;
        if (ViuConversaEntrada(id)) yield break;
        if (Dados.curraisDestinoConfirmado)
            yield return FalarPosto("Zefinha", "O grupo foi encaminhado pra Lagoa do Meio.",
                "Pai", "E tua irmã?", "Zefinha", "O nome dela num tava no que eu vi. Ainda preciso perguntar.");
        else if (!Dados.curraisIdentidadeConfirmada)
            yield return FalarPosto("Zefinha", "Raimundo viu uma menina parecida com ela, mas ainda num confirmou quem tava junto.",
            "Pai", "Nós continua procurando, minha filha.");
        RegistrarConversaEntrada(id); Salvar();
    }
}
