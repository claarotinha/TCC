using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EntradaCurrais
{
    private bool TemCadernoNaNoite => !Dados.curraisDiarioRecusado &&
        (Dados.curraisDiarioRecuperado || Dados.diaryCollected);
    private bool TemLembrancaRefeicao =>
        Dados.secaFoodChoice >= 1 && Dados.secaFoodChoice <= 3;
    private bool TemAssuntosCampo => true;
    private bool TemAssuntosCasa => true;

    private IEnumerator EncerrarRelatoDoQuarto()
    {
        if (ViuConversaEntrada("familia-amanha")) yield break;
        yield return FalarPosto("Pai", "Amanhã eu vou perguntar com tu.",
            "Mãe", "Tu mal consegue levantar esse braço.", "Pai", "Eu consigo falar.");
        RegistrarConversaEntrada("familia-amanha"); Salvar();
    }

    private IEnumerator AbrirConversaFamilia()
    {
        yield return NoticiasNovasFamilia();
        yield return EncerrarRelatoDoQuarto();
        MenuAbrigo();
    }

    private IEnumerator NoticiasNovasFamilia()
    {
        // O marcador anterior também é reconhecido: não repete a notícia do quarto/posto.
        if (Dados.curraisDestinoConfirmado && !ViuConversaEntrada("posto-noticia-destino-True"))
        {
            yield return FalarPosto("Zefinha", Dados.curraisEscolhaPosto == 2 ?
                "No encaminhamento tava escrito Lagoa do Meio." :
                "Quando perguntei pelo carro, disseram Lagoa do Meio.");
            RegistrarConversaEntrada("posto-noticia-destino-True"); Salvar();
        }
        if (Dados.curraisIdentidadeConfirmada && !ViuConversaEntrada("familia-nair-contada"))
        {
            yield return FalarPosto("Zefinha", "A mulher se chama Nair Pereira. Raimundo confirmou com quem conhece ela.");
            RegistrarConversaEntrada("familia-nair-contada"); Salvar();
        }
    }

    private void MenuFamiliaLinear()
    {
        var rotulos = new List<string>(); var acoes = new List<Action>();
        if (TemAssuntosCampo) { rotulos.Add("Conversar sobre o campo"); acoes.Add(MenuCampoFamilia); }
        if (TemAssuntosCasa) { rotulos.Add("Lembranças de casa"); acoes.Add(MenuLembrancasFamilia); }
        rotulos.Add("Sair"); acoes.Add(FecharMenu);
        MostrarOpcoes("Família", string.Empty, rotulos.ToArray(), acoes.ToArray());
    }

    private void MenuCampoFamilia()
    {
        var rotulos = new List<string>(); var acoes = new List<Action>();
        { rotulos.Add("O que prometeram?"); acoes.Add(() => Iniciar(ConversarFamilia(1, familiaPromessas))); }
        { rotulos.Add("O trabalho e Bento"); acoes.Add(() => Iniciar(ConversarFamilia(2, familiaTrabalho))); }
        rotulos.Add("Voltar"); acoes.Add(MenuAbrigo);
        MostrarOpcoes("Família", string.Empty, rotulos.ToArray(), acoes.ToArray());
    }

    private void MenuCasaFamilia()
    {
        var rotulos = new List<string>(); var acoes = new List<Action>();
        if (TemLembrancaRefeicao)
        { rotulos.Add("Lembrar da refeição"); acoes.Add(() => Iniciar(RelembrarRefeicao())); }
        { rotulos.Add(TemCadernoNaNoite ? "O caderno" : "As coisas de casa"); acoes.Add(() => Iniciar(ConversarCasaFamilia())); }
        rotulos.Add("Voltar"); acoes.Add(MenuAbrigo);
        MostrarOpcoes("Família", string.Empty, rotulos.ToArray(), acoes.ToArray());
    }

    private IEnumerator ConversarCasaFamilia()
    {
        if (TemCadernoNaNoite) yield return Dizer(familiaCaderno);
        else yield return FalarPosto("Zefinha", "Eu queria lembrar das coisas de casa. Agora só penso no que vi hoje.",
            "Mãe", "Pode me contar. Num precisa guardar tudo sozinha.", "Zefinha", "E se eu esquecer?",
            "Mãe", "Nós vai lembrando junto. Uma conta o que a outra num lembrar.");
        Dados.curraisPerguntasFamilia |= 4;
        Salvar(); MenuLembrancasFamilia();
    }

    private IEnumerator PrepararUltimaConversaNoite()
    {
        if (ViuConversaEntrada("noite-introducao-concluida")) yield break;
        yield return FalarPosto("Mãe", "Venha. Teu lugar tá aqui, perto de mim.");
        int promessa = Dados.estradaSegundaEscolhaNoturna;
        if (promessa >= 1 && promessa <= 3)
            yield return Dizer(promessa == 1 ? noitePromessa : promessa == 2 ? noiteTranquilizou : noiteSilencio);
        if (TemCadernoNaNoite) yield return Dizer(noiteCaderno);
        else if (TemItem(anotacaoPosto))
            yield return FalarPosto("Zefinha", "Vou guardar esse papel junto de mim.", "Mãe", "Guarde onde num perca.");
        else yield return FalarPosto("Zefinha", "Eu num quero esquecer dos nomes.",
            "Mãe", "Quando amanhecer, tu me conta de novo. Nós vai lembrando junto.");
        yield return Dizer(noiteInicio);
        RegistrarConversaEntrada("noite-introducao-concluida"); Salvar();
    }

    private IEnumerator PassarNoiteLinear()
    {
        if (!PostoDisponivel || Dados.curraisEtapaPosto != 3) yield break;
        yield return ChapterCard.Show("josefa-11");
        Vector3 origem = mari.transform.position;
        yield return Escurecer(1f);
        try
        {
            noiteEmCurso = true;
            Posicionar(mari.transform, mariNoite);
            AtualizarPosto();
            yield return Escurecer(0f);
            yield return PrepararUltimaConversaNoite();
            yield return Escolher("Zefinha", perguntaNoite, opcoesNoite);
            int opcao = escolhaRecebida;
            // Sair preserva a introdução já ouvida, sem escolher ou concluir a noite.
            if (opcao < 0 || opcao > 1) yield break;
            yield return Dizer(opcao == 0 ? noiteContar : noitePerto);
            yield return Escurecer(1f);
            Dados.curraisEscolhaNoite = opcao + 1;
            Dados.curraisEtapaPosto = 4;
            noiteEmCurso = false;
            GameProgress.Instance.UnlockDiaryPage("currais-noite");
            AtualizarEstadoBusca(); Salvar();
            yield return Esperar(.5f);
            yield return Escurecer(0f);
        }
        finally
        {
            noiteEmCurso = false;
            if (Dados.curraisEtapaPosto < 4) mari.transform.position = origem;
            if (!encerrando) { AtualizarPosto(); CorFade(0f); }
        }
    }
}
