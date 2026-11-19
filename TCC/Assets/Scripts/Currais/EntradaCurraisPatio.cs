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
        return ReferenciasAtendimentoBentoValidas() & ReferenciasPatioLinearValidas() & ok;
    }

    private Collider2D[] ComAlvosDoPatio(Collider2D[] alvos)
    {
        Collider2D[] resultado = new Collider2D[alvos.Length + 7];
        Array.Copy(alvos, resultado, alvos.Length);
        resultado[alvos.Length] = areaPaiPatio;
        resultado[alvos.Length + 1] = areaBentoPatio;
        resultado[alvos.Length + 2] = areaFiscalPatio;
        resultado[alvos.Length + 3] = areaPanoLimpoApoio;
        resultado[alvos.Length + 4] = areaMae;
        resultado[alvos.Length + 5] = areaEmbrulhoBento;
        resultado[alvos.Length + 6] = areaFamiliarBento;
        return resultado;
    }

    private bool InteragirPessoasDoPatio(Vector2 ponto)
    {
        if (!Dados.curraisPanelaConcluida || Dados.curraisEtapaBusca > 5) return false;
        if (InteragirPertencesBento(ponto)) return true;
        if (areaPanoLimpoApoio.enabled && areaPanoLimpoApoio.gameObject.activeInHierarchy &&
            areaPanoLimpoApoio.OverlapPoint(ponto))
        { Iniciar(AjudarBento()); return true; }
        if ((Dados.curraisEtapaBusca == 4 || Dados.curraisEtapaBusca == 5) && Dados.curraisEscolhaAgua == 1 &&
            areaMae.enabled && areaMae.gameObject.activeInHierarchy && areaMae.OverlapPoint(ponto))
        { if (Dados.curraisEtapaBusca == 5) AbrirSocorroOuUsarItem(); else ConversarAcompanhantePatioLinear(); return true; }
        foreach (Collider2D alvo in new[] { areaPaiPatio, areaBentoPatio, areaFiscalPatio })
        {
            if (!alvo.enabled || !alvo.gameObject.activeInHierarchy || !alvo.OverlapPoint(ponto)) continue;
            if (Dados.curraisEtapaBusca == 5 && alvo != areaFiscalPatio)
            { AbrirSocorroOuUsarItem(); return true; }
            if (alvo == areaPaiPatio) Iniciar(AbrirPaiPatioLinear());
            else if (alvo == areaBentoPatio) Iniciar(AbrirBentoPatioLinear());
            else Iniciar(AbrirFiscalPatioLinear());
            return true;
        }
        return false;
    }

    private void ConversarLuziaNoPatio()
    {
        if (Dados.curraisEtapaBusca == 5) { AbrirSocorroOuUsarItem(); return; }
        if (Dados.curraisEscolhaAgua == 2) ConversarAcompanhantePatioLinear();
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

    private void MenuSocorroBento() => Iniciar(AbrirAtendimentoBentoLinear());

    private void MenuAguaDoPatio() => Iniciar(AbrirApoioPatioLinear());

    private void MenuApoioDoPatio() => Iniciar(AbrirApoioPatioLinear());

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
        cabacaPatioComMari.gameObject.SetActive(false);
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
