using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Primeiro abrigo — Examine e conversas em sequência")]
    [SerializeField] private ExamineObject examePrimeiroCliqueLuzia;
    [SerializeField] private Fala[] conversaIntimaPaiAbrigo, conversaIntimaMaeAbrigo;
    [SerializeField] private Fala[] noticiaIrmaAbrigo, conviteAguaAbrigo;
    private bool exameLuziaEmCurso;
    private int quadroExamineLuzia;
    private Action continuidadeContatoLuzia;

    // Os identificadores usam a lista persistente existente, inclusive em saves antigos.
    private bool ConviteAguaConhecido => ViuConversaEntrada("abrigo-convite-agua") ||
        Dados.curraisDistribuicaoIniciada || Dados.curraisAguaRecebida || Dados.curraisPanelaConcluida;
    private bool PedidoPanoConhecido => ViuConversaEntrada("abrigo-pedido-pano") ||
        Dados.curraisPanoAbrigoRecolhido || Dados.curraisPaiAcomodado || ConviteAguaConhecido;
    private bool NoticiaIrmaConhecida => ViuConversaEntrada("luzia-irma") || ConviteAguaConhecido;
    private bool PerguntaLugarConhecida => (Dados.curraisPerguntasAbrigo & 3) != 0;
    private bool ConversaIntimaEncerrada => ViuConversaEntrada("abrigo-intima-pai") ||
        ViuConversaEntrada("abrigo-intima-mae") || NoticiaIrmaConhecida;

    private bool ReferenciasAbrigoLinearValidas() =>
        ConferirReferencia(examePrimeiroCliqueLuzia, nameof(examePrimeiroCliqueLuzia)) &&
        ConferirReferencia(examePrimeiroCliqueLuzia.examinePanel, "Painel de Examine de Luzia") &&
        ConferirReferencia(examePrimeiroCliqueLuzia.examineText, "Texto de Examine de Luzia") &&
        FalasValidas(conversaIntimaPaiAbrigo) && FalasValidas(conversaIntimaMaeAbrigo) &&
        FalasValidas(noticiaIrmaAbrigo) && FalasValidas(conviteAguaAbrigo);

    private void SolicitarContatoLuzia(Action continuar)
    {
        continuidadeContatoLuzia = continuar;
        if (!ViuConversaEntrada("abrigo-examine-luzia") && !Dados.curraisLuziaApresentada)
        {
            FecharDialogo(); PausarExploracao();
            if (InventoryTabController.Instance != null) InventoryTabController.Instance.Close();
            exameLuziaEmCurso = true;
            quadroExamineLuzia = Time.frameCount;
            examePrimeiroCliqueLuzia.ShowPanel();
            RegistrarConversaEntrada("abrigo-examine-luzia"); Salvar();
            return;
        }
        MostrarApresentacaoLuzia();
    }

    private bool AtualizarExamineLuzia()
    {
        if (!exameLuziaEmCurso) return false;
        PausarExploracao();
        if (examePrimeiroCliqueLuzia.PanelIsOpen)
        {
            if (Time.frameCount > quadroExamineLuzia && Input.GetMouseButtonDown(0) && !SobreControle())
                examePrimeiroCliqueLuzia.HidePanel();
            return true;
        }
        exameLuziaEmCurso = false;
        MostrarApresentacaoLuzia();
        return true;
    }

    private void MostrarApresentacaoLuzia()
    {
        MostrarOpcoes("Mulher", string.Empty, new[] { "Se apresentar", "Sair" },
            new Action[] { () => Iniciar(ApresentarLuzia(continuidadeContatoLuzia)), FecharMenu });
    }

    private void MenuLuziaAbrigoLinear()
    {
        if (!Dados.curraisFamiliaParou) return;
        if (!Dados.curraisLuziaApresentada)
        { SolicitarContatoLuzia(MenuLuziaAbrigoLinear); return; }
        if (!PedidoPanoConhecido)
        { Iniciar(ApresentarLuzia(MenuLuziaAbrigoLinear)); return; }
        if (!Dados.curraisPaiAcomodado) return;
        if (!NoticiaIrmaConhecida)
        { Iniciar(ContarSobreMariquinhaAbrigo()); return; }
        if (!ConviteAguaConhecido)
        {
            if (PerguntaLugarConhecida) Iniciar(ConvidarParaAguaAbrigo());
            else MostrarPerguntasLugarAbrigo();
            return;
        }
        // Durante a distribuição, Luzia acompanha sem reiniciar assuntos encerrados.
    }

    private void MenuParenteAbrigo(bool falarComPai)
    {
        if (!Dados.curraisFamiliaParou) return;
        if (!Dados.curraisPaiAcomodado)
        {
            if (!falarComPai && PedidoPanoConhecido)
                AbrirAssuntosEntrada("mae-ajuda-pano", "Mãe", string.Empty,
                    new[] { "mae-pano" }, new[] { "Como posso ajudar?" },
                    new Func<IEnumerator>[] { () => Dizer(maePrecisaPano) });
            return;
        }
        if (ConversaIntimaEncerrada) return;
        MostrarMenuEntrada(falarComPai ? "abrigo-intima-pai" : "abrigo-intima-mae",
            falarComPai ? "Pai" : "Mãe", falarComPai ? "O que foi, Josefa?" : "Venha cá, minha filha.",
            new[] { falarComPai ? "Tá doendo muito?" : "E se Mariquinha procurar nós?", "Sair" },
            new Action[] { () => Iniciar(ConversarIntimamenteAbrigo(falarComPai)), FecharMenu });
    }

    private IEnumerator ConversarIntimamenteAbrigo(bool falarComPai)
    {
        if (ConversaIntimaEncerrada) yield break;
        yield return Dizer(falarComPai ? conversaIntimaPaiAbrigo : conversaIntimaMaeAbrigo);
        RegistrarConversaEntrada(falarComPai ? "abrigo-intima-pai" : "abrigo-intima-mae"); Salvar();
    }

    private IEnumerator ContarSobreMariquinhaAbrigo()
    {
        if (!NoticiaIrmaConhecida)
        {
            yield return Dizer(noticiaIrmaAbrigo);
            RegistrarConversaEntrada("luzia-irma");
            Dados.curraisPerguntasAbrigo |= 4; Salvar();
        }
        MostrarPerguntasLugarAbrigo();
    }

    private void MostrarPerguntasLugarAbrigo()
    {
        MostrarMenuEntrada("abrigo-lugar", "Luzia", string.Empty,
            new[] { "Por que chamam aqui de curral?", "Quem pode sair?", "Sair" },
            new Action[] { () => Iniciar(PerguntarLugarAbrigo(1)),
                () => Iniciar(PerguntarLugarAbrigo(2)), FecharMenu });
    }

    private IEnumerator PerguntarLugarAbrigo(int bit)
    {
        if (PerguntaLugarConhecida) yield break;
        yield return Dizer(bit == 1 ? luziaCurral : luziaSaida);
        Dados.curraisPerguntasAbrigo |= bit; Salvar();
        yield return ConvidarParaAguaAbrigo();
    }

    private IEnumerator ConvidarParaAguaAbrigo()
    {
        if (ConviteAguaConhecido) yield break;
        yield return Dizer(conviteAguaAbrigo);
        RegistrarConversaEntrada("abrigo-convite-agua");
        RegistrarConversaEntrada("luzia-buscar-agua");
        Dados.curraisDistribuicaoIniciada = true;
        AtualizarEstadoBusca(); Salvar();
    }

    private void MostrarPaginaAssuntos(string pessoa, string abertura, string[] rotulos, Action[] acoes, int indice)
    {
        // Um assunto por página, navegação e a saída original: nunca quatro botões.
        int ultimo = rotulos.Length - 1;
        MostrarOpcoes(pessoa, abertura, new[] { rotulos[indice], "Outros assuntos", rotulos[ultimo] },
            new Action[] { acoes[indice],
                () => MostrarPaginaAssuntos(pessoa, string.Empty, rotulos, acoes, (indice + 1) % ultimo), acoes[ultimo] });
    }
}
