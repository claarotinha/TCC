using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Quarto de atendimento — Examine")]
    [SerializeField] private ExamineObject examePassagemQuarto, exameMulherQuarto, exameHomemQuarto;

    private bool InvestigouQuarto => Dados.curraisEtapaPosto >= 2;
    private bool VoltouDoQuarto => Dados.curraisEtapaPosto >= 3 || ViuConversaEntrada("quarto-retorno-familia");

    private bool ReferenciasQuartoLinearValidas()
    {
        bool ok = true;
        foreach (ExamineObject exame in new[] { examePassagemQuarto, exameMulherQuarto, exameHomemQuarto })
            ok &= ConferirReferencia(exame, "Examine do quarto") &&
                ConferirReferencia(exame.examinePanel, "Painel de Examine do quarto") &&
                ConferirReferencia(exame.examineText, "Texto de Examine do quarto");
        return ok;
    }

    private bool AtualizarVozQuarto()
    {
        if (ocupado || menuAberto || InventarioAbertoMemoria() || !PostoDisponivel ||
            Dados.curraisEscolhaPosto == 0 || InvestigouQuarto || ViuConversaEntrada("quarto-voz-ouvida")) return false;
        Vector2 perto = areaPortaQuarto.ClosestPoint(mari.transform.position);
        if (Vector2.Distance(perto, mari.transform.position) > alcanceInteracao) return false;
        Iniciar(OuvirVozQuarto()); return true;
    }

    private IEnumerator OuvirVozQuarto()
    {
        if (InvestigouQuarto || ViuConversaEntrada("quarto-voz-ouvida")) yield break;
        yield return Dizer(quartoVoz);
        RegistrarConversaEntrada("quarto-voz-ouvida"); Salvar();
    }

    private IEnumerator ClicarPassagemQuarto()
    {
        if (Dados.curraisEscolhaPosto == 0) yield break;
        if (InvestigouQuarto)
        {
            if (!VoltouDoQuarto)
            {
                MenuAssuntoNPC("Zefinha", string.Empty, "Voltar para contar à família", VoltarFamiliaQuarto);
                yield break;
            }
            yield return ExaminarPosto("", examePassagemQuarto,
                "As mesmas pessoas continuam no quarto. Mariquinha não está entre elas.");
            yield break;
        }
        yield return OuvirVozQuarto();
        yield return ExaminarPosto("quarto-passagem-examinada", examePassagemQuarto);
        MenuAssuntoNPC("Zefinha", string.Empty, "Perguntar pela menina", InvestigarQuarto);
    }

    private IEnumerator InvestigarQuartoLinear()
    {
        if (Dados.curraisEscolhaPosto == 0 || InvestigouQuarto) yield break;
        yield return OuvirVozQuarto();
        yield return FalarPosto("Zefinha", "Tinha uma menina falando aqui. Posso olhar?",
            "Cabo", "Olhe da porta. Num atrapalhe o atendimento.");
        if (!Dados.curraisQuartoAberto) yield return AbrirPortaDoQuarto();
        yield return FalarPosto("Zefinha", "Mariquinha?", "Trabalhadora",
            "Aqui num tem menina, minha filha. Essas pessoas chegaram de manhã.");
        yield return FalarPosto("Zefinha (em pensamento)", "Eu podia jurar que ouvi a voz dela.");
        yield return FalarPosto("Cabo", "Já olhou. Deixe o atendimento seguir.",
            "Zefinha (em pensamento)", "Ele tá vendo que eu tô procurando. Ele sabe.");
        if (Dados.curraisEscolhaPosto == 1)
            yield return Mover(areaRaimundo.transform, raimundoSentado.position, tempoPlano);
        Dados.curraisEtapaPosto = 2;
        GameProgress.Instance.UnlockDiaryPage("currais-quarto");
        AtualizarEstadoBusca(); Salvar();
    }

    private IEnumerator ClicarDoenteQuarto(bool mulher)
    {
        if (!InvestigouQuarto) yield break;
        string id = mulher ? "quarto-mulher" : "quarto-homem";
        yield return ExaminarPosto(id + "-examine", mulher ? exameMulherQuarto : exameHomemQuarto);
        MenuAssuntoNPC(mulher ? "Mulher" : "Homem", string.Empty,
            mulher ? "Como a senhora tá?" : "O senhor viu uma menina?", () => ConversarDoenteQuarto(mulher));
    }

    private IEnumerator ConversarDoenteQuarto(bool mulher)
    {
        string id = mulher ? "quarto-mulher-conversa" : "quarto-homem-conversa";
        if (mulher) yield return Dizer(doenteUmFalas);
        else if (Dados.curraisAjudouAtendimentoQuarto)
            yield return FalarPosto("Zefinha", "O senhor viu uma menina parecida comigo?", "Homem", "Num vi, não…");
        else
        {
            yield return FalarPosto("Zefinha", "O senhor viu uma menina parecida comigo?",
                "Homem", "Num vi, não… Água… por favor.", "Trabalhadora", "Eu já trago. Espere um pouco.");
        }
        RegistrarConversaEntrada(id); Salvar();
    }

    private void MenuTrabalhadoraLinear()
    {
        if (!InvestigouQuarto) return;
        var rotulos = new List<string>(); var acoes = new List<Action>();
        if (!ViuConversaEntrada("quarto-nome-informado"))
        { rotulos.Add("Deixar o nome de Mariquinha"); acoes.Add(() => Iniciar(InformarNomeQuarto())); }
        if (!Dados.curraisAjudouAtendimentoQuarto)
        { rotulos.Add("O que tá faltando?"); acoes.Add(() => Iniciar(PedirRecipienteQuarto())); }
        if (!VoltouDoQuarto && rotulos.Count == 0)
        {
            rotulos.Add("Voltar para contar à família");
            acoes.Add(() => Iniciar(VoltarFamiliaQuarto()));
        }
        rotulos.Add("Sair"); acoes.Add(FecharMenu);
        if (rotulos.Count > 1) MostrarOpcoes("Trabalhadora", string.Empty, rotulos.ToArray(), acoes.ToArray());
    }

    private IEnumerator InformarNomeQuarto()
    {
        if (ViuConversaEntrada("quarto-nome-informado")) yield break;
        yield return FalarPosto("Zefinha", "O nome dela é Maria Severina. Nós chama ela de Mariquinha.");
        yield return FalarPosto("Zefinha", "Separaram ela de nós na entrada. Se ouvir esse nome, pode me procurar.",
            "Trabalhadora", "Procuro, sim.");
        if (Dados.curraisIdentidadeConfirmada)
            yield return FalarPosto("Zefinha", "Minha irmã tava com Nair Pereira. A senhora viu ela por aqui?",
                "Trabalhadora", "De nome eu num conheço. Vou perguntar por ela também.");
        yield return FalarPosto("Zefinha", "Obrigada.");
        RegistrarConversaEntrada("quarto-nome-informado"); Salvar();
    }

    private IEnumerator PedirRecipienteQuarto()
    {
        if (Dados.curraisAjudouAtendimentoQuarto) yield break;
        yield return Dizer(quartoSemCabaca);
        RegistrarConversaEntrada("quarto-pedido-recipiente"); Salvar();
    }

    private IEnumerator ReverCaboQuarto()
    {
        if (!InvestigouQuarto) { yield return ClicarPassagemQuarto(); yield break; }
        if (ViuConversaEntrada("quarto-cabo-revisita")) yield break;
        yield return FalarPosto("Cabo", "Deixe a passagem livre.");
        RegistrarConversaEntrada("quarto-cabo-revisita"); Salvar();
    }

    private IEnumerator VoltarFamiliaQuarto()
    {
        if (!InvestigouQuarto || VoltouDoQuarto) yield break;
        yield return FalarPosto("Zefinha (em pensamento)", "Vou procurar meus pais perto das esteiras.");
        yield return Escurecer(1f);
        RegistrarConversaEntrada("quarto-retorno-familia");
        Posicionar(pai, paiAbrigo); Posicionar(mae, maeAbrigo); Posicionar(luzia.transform, luziaRepouso);
        Vector3 chegada = pai.position + Vector3.left;
        chegada.y = mari.transform.position.y;
        mari.transform.position = chegada;
        Physics2D.SyncTransforms(); AtualizarEstadoBusca(); Salvar();
        yield return Escurecer(0f);
        yield return ContarAosPais();
    }

    private IEnumerator RelatarQuartoFamilia()
    {
        if (ViuConversaEntrada("quarto-noticia-contada")) yield break;
        RegistrarConversaEntrada("quarto-retorno-familia");
        yield return FalarPosto("Pai", "Tu descobriu alguma coisa sobre tua irmã?");
        string id = "posto-noticia-destino-" + Dados.curraisDestinoConfirmado;
        if (!ViuConversaEntrada(id))
        {
            if (Dados.curraisDestinoConfirmado)
                yield return FalarPosto("Zefinha", Dados.curraisEscolhaPosto == 2 ? "No encaminhamento tava escrito Lagoa do Meio." :
                    "Quando perguntei pelo carro, disseram Lagoa do Meio.",
                    "Pai", "E tua irmã?", "Zefinha", "O nome dela num tava no que eu vi. Ainda preciso perguntar.");
            else if (!Dados.curraisIdentidadeConfirmada)
                yield return FalarPosto("Zefinha", "Raimundo viu uma menina parecida com ela, mas ainda num confirmou quem tava junto.");
            RegistrarConversaEntrada(id);
        }
        yield return FalarPosto("Pai", "Então nós continua procurando, minha filha.");
        RegistrarConversaEntrada("quarto-noticia-contada"); Salvar();
    }
}
