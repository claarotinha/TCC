using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Pátio e socorro — elementos editáveis")]
    [SerializeField] private Collider2D areaPaiPatio, areaBentoPatio, areaFiscalPatio;
    [SerializeField] private SpriteRenderer poseBentoSocorro, panoNoSocorro, pertencesBentoCobertos, cabacaPatioComMari;
    [SerializeField] private Fala[] bentoTrabalhoFalas, fiscalPatioFalas, patioVoltarPai;
    [SerializeField] private Fala[] luziaInicioPatio, luziaTarefasPatio, luziaAguaPatio, luziaPanoPatio;
    [SerializeField] private Fala[] patioFaltaAgua, patioFaltaPano, socorroFaltaAgua, socorroFaltaPano;
    [SerializeField] private Fala[] socorroFaltamAmbos, pertencesSemPano, despedidaPronta, cabacaGuardada, socorroGuardarCabaca;
    private bool agressaoPatioEmCurso, atendimentoBentoEmCurso, bentoEmSocorroEmCena;

    private bool ReferenciasPatioDetalhadoValidas()
    {
        UnityEngine.Object[] refs = { areaPaiPatio, areaBentoPatio, areaFiscalPatio,
            poseBentoSocorro, panoNoSocorro, pertencesBentoCobertos, cabacaPatioComMari };
        string[] nomes = { nameof(areaPaiPatio), nameof(areaBentoPatio), nameof(areaFiscalPatio),
            nameof(poseBentoSocorro), nameof(panoNoSocorro), nameof(pertencesBentoCobertos), nameof(cabacaPatioComMari) };
        bool ok = true;
        for (int i = 0; i < refs.Length; i++) ok &= ConferirReferencia(refs[i], nomes[i]);
        foreach (Fala[] falas in new[] { bentoTrabalhoFalas, fiscalPatioFalas, patioVoltarPai,
            luziaInicioPatio, luziaTarefasPatio, luziaAguaPatio, luziaPanoPatio,
            patioFaltaAgua, patioFaltaPano, socorroFaltaAgua, socorroFaltaPano,
            socorroFaltamAmbos, pertencesSemPano, despedidaPronta, cabacaGuardada, socorroGuardarCabaca }) ok &= FalasValidas(falas);
        return ok;
    }

    private Collider2D[] ComAlvosDoPatio(Collider2D[] alvos)
    {
        Collider2D[] resultado = new Collider2D[alvos.Length + 3];
        Array.Copy(alvos, resultado, alvos.Length);
        resultado[alvos.Length] = areaPaiPatio;
        resultado[alvos.Length + 1] = areaBentoPatio;
        resultado[alvos.Length + 2] = areaFiscalPatio;
        return resultado;
    }

    private bool InteragirPessoasDoPatio(Vector2 ponto)
    {
        if (!Dados.curraisPanelaConcluida || Dados.curraisEtapaBusca > 5) return false;
        foreach (Collider2D alvo in new[] { areaPaiPatio, areaBentoPatio, areaFiscalPatio })
        {
            if (!alvo.enabled || !alvo.gameObject.activeInHierarchy || !alvo.OverlapPoint(ponto)) continue;
            if (alvo == areaPaiPatio)
            {
                OlharParaMari(pai);
                if (Dados.curraisEtapaBusca == 4) MenuAssuntoNPC("Pai", "Diga, minha filha.", "Como tá o serviço?", TestemunharTrabalho);
                else AbrirSocorroOuUsarItem();
            }
            else if (alvo == areaBentoPatio)
            {
                if (Dados.curraisEtapaBusca <= 4) { OlharParaMari(bento); MenuAssuntoNPC("Bento", "Diga, menina.", "Como tá o senhor?", () => Dizer(bentoTrabalhoFalas)); }
                else AbrirSocorroOuUsarItem();
            }
            else { OlharParaMari(fiscalTrabalho); MenuAssuntoNPC("Fiscal", "Seja breve.", "Falar do trabalho", () => Dizer(fiscalPatioFalas)); }
            return true;
        }
        return false;
    }

    private void ConversarLuziaNoPatio()
    {
        if (Dados.curraisEtapaBusca == 5) { AbrirSocorroOuUsarItem(); return; }
        Fala[] falas = !Dados.curraisTrabalhoPatioIniciado ? luziaInicioPatio :
            !Dados.curraisAguaPatioPreparada && !Dados.curraisPanosPatioEntregues ? luziaTarefasPatio :
            !Dados.curraisAguaPatioPreparada ? luziaAguaPatio :
            !Dados.curraisPanosPatioEntregues ? luziaPanoPatio : patioVoltarPai;
        MenuAssuntoNPC("Luzia", "Tô aqui, Josefa.", "Sobre o que tá acontecendo", () => Dizer(falas));
    }

    private void AbrirSocorroOuUsarItem() => MenuSocorroBento();

    private Fala[] EstadoSocorroBento()
    {
        if (Dados.curraisBentoMorto)
            return Dados.curraisPanoSepultamentoEntregue && Dados.curraisCabacaSocorroRecolhida
                ? despedidaPronta : !Dados.curraisPanoSepultamentoEntregue ? pertencesSemPano : socorroGuardarCabaca;
        int entregues = Dados.curraisItensAtendimentoBento & 3;
        return entregues == 1 ? socorroFaltaAgua : entregues == 2 ? socorroFaltaPano : socorroFaltamAmbos;
    }

    private void MenuSocorroBento()
    {
        if (!Dados.curraisBentoMorto && (Dados.curraisItensAtendimentoBento & 3) == 3)
        {
            MenuAssuntoNPC("Luzia", "Josefa, venha cá.", "O que aconteceu?", ConcluirAtendimentoBento);
            return;
        }
        if (Dados.curraisBentoMorto && Dados.curraisPanoSepultamentoEntregue && Dados.curraisCabacaSocorroRecolhida)
        {
            MenuAssuntoNPC("Luzia", "A família dele tá esperando.", "Posso ir com vocês?", SepultarBento);
            return;
        }
        MenuAssuntoNPC("Luzia", "Diga, Josefa.", "Como posso ajudar?", () => Dizer(EstadoSocorroBento()));
    }

    private void MenuAguaDoPatio()
    {
        MenuAssuntoNPC("Trabalhador", "Diga, menina.", "Tem água pra eles?", () => Dizer(patioFaltaAgua));
    }

    private void MenuApoioDoPatio()
    {
        MenuAssuntoNPC("Trabalhador", "Tô ouvindo.", "O que tá faltando?", () => Dizer(patioFaltaPano));
    }

    private void AtualizarDetalhesPatio()
    {
        bool ativo = Dados.curraisEtapaEntrada == 4 && Dados.curraisPanelaConcluida;
        int etapa = Dados.curraisEtapaBusca;
        areaPaiPatio.enabled = ativo && (etapa == 4 || etapa == 5);
        areaBentoPatio.enabled = ativo && etapa <= 5;
        areaFiscalPatio.enabled = ativo && etapa <= 5;
        bool vivoNoSocorro = ativo && !Dados.curraisBentoMorto && (etapa == 5 || bentoEmSocorroEmCena);
        // Sem desenho atribuído, o ciclo original permanece como referência, sem girar o personagem.
        bool poseAtribuida = vivoNoSocorro && poseBentoSocorro.sprite != null;
        poseBentoSocorro.gameObject.SetActive(poseAtribuida);
        bento.GetComponent<SpriteRenderer>().enabled = !Dados.curraisBentoMorto && !poseAtribuida;
        panoNoSocorro.gameObject.SetActive(vivoNoSocorro && (Dados.curraisItensAtendimentoBento & 1) != 0);
        pertencesBentoCobertos.gameObject.SetActive(ativo && Dados.curraisBentoMorto &&
            Dados.curraisPanoSepultamentoEntregue && !Dados.curraisSepultamentoConcluido);
        bool aguaNaMao = ativo && Dados.curraisAguaPatioPreparada &&
            ((etapa == 4) || (etapa == 5 && !Dados.curraisBentoMorto && (Dados.curraisItensAtendimentoBento & 2) == 0));
        cabacaPatioComMari.gameObject.SetActive(aguaNaMao && TemItem(cabacaCheiaBusca));
        cabacaPatioComMari.color = new Color(.65f, .85f, 1f, 1f);
        cabacaSocorro.GetComponent<SpriteRenderer>().color = Dados.curraisBentoMorto
            ? Color.white : new Color(.65f, .85f, 1f, 1f);
    }

    private IEnumerator MostrarCoberturaBento(Color cor)
    {
        SpriteRenderer desenho = corpoCoberto.GetComponent<SpriteRenderer>();
        try
        {
            float tempo = 0f;
            while (tempo < .6f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
                {
                    tempo += Time.deltaTime;
                    desenho.color = new Color(cor.r, cor.g, cor.b, cor.a * Mathf.Clamp01(tempo / .6f));
                }
                yield return null;
            }
        }
        finally { desenho.color = cor; }
    }

    private void EncerrarPatioDetalhado()
    {
        agressaoPatioEmCurso = false;
        atendimentoBentoEmCurso = false;
        bentoEmSocorroEmCena = false;
    }
}
