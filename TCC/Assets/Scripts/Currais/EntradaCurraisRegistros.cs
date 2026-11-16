using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Criança e registros — elementos editáveis")]
    [SerializeField] private Collider2D areaLataMenino, areaCanhotoMesa, areaFolhasRegistro;
    [SerializeField] private GameObject folhasAntesRegistro, folhasDepoisRegistro, lotePanosComMari;
    [SerializeField] private Transform folhaEmConsulta, folhaConsultaFim;
    [SerializeField] private Fala[] meninoReencontro, meninoNomeLembrado, meninoDepoisRegistro;
    [SerializeField] private Fala[] lataMeninoFalas, mesaAntesAgua, folhasRegistroFalas, lotePanosComVoce;
    private bool consultandoFolhaRegistro;

    private bool ReferenciasRegistrosValidas()
    {
        return ConferirReferencia(areaLataMenino, nameof(areaLataMenino)) &
            ConferirReferencia(areaCanhotoMesa, nameof(areaCanhotoMesa)) &
            ConferirReferencia(areaFolhasRegistro, nameof(areaFolhasRegistro)) &
            ConferirReferencia(folhasAntesRegistro, nameof(folhasAntesRegistro)) &
            ConferirReferencia(folhasDepoisRegistro, nameof(folhasDepoisRegistro)) &
            ConferirReferencia(lotePanosComMari, nameof(lotePanosComMari)) &
            ConferirReferencia(folhaEmConsulta, nameof(folhaEmConsulta)) &
            ConferirReferencia(folhaConsultaFim, nameof(folhaConsultaFim)) &
            FalasValidas(meninoReencontro) & FalasValidas(meninoNomeLembrado) &
            FalasValidas(meninoDepoisRegistro) & FalasValidas(lataMeninoFalas) &
            FalasValidas(mesaAntesAgua) & FalasValidas(folhasRegistroFalas) &
            FalasValidas(lotePanosComVoce);
    }

    private Collider2D[] ComCriancaERegistros(Collider2D[] alvos)
    {
        Collider2D[] resultado = new Collider2D[alvos.Length + 4];
        Array.Copy(alvos, resultado, alvos.Length);
        resultado[alvos.Length] = areaLataMenino;
        resultado[alvos.Length + 1] = areaMenino;
        resultado[alvos.Length + 2] = areaCanhotoMesa;
        resultado[alvos.Length + 3] = areaFolhasRegistro;
        return resultado;
    }

    private void OlharParaMari(Transform pessoa)
    {
        SpriteRenderer visual = pessoa.GetComponent<SpriteRenderer>();
        if (visual != null && Mathf.Abs(mari.transform.position.x - pessoa.position.x) > .01f)
            visual.flipX = (mari.transform.position.x - pessoa.position.x) * pessoa.lossyScale.x < 0f;
    }

    private IEnumerator ReencontrarMenino()
    {
        OlharParaMari(meninoBusca);
        yield return Dizer(Dados.curraisRegistroConcluido ? meninoDepoisRegistro :
            (Dados.curraisPerguntasMenino & 2) != 0 ? meninoNomeLembrado : meninoReencontro);
        AbrirMenuMenino();
    }

    private IEnumerator ExaminarLataMenino()
    {
        OlharParaMari(meninoBusca);
        yield return Dizer(lataMeninoFalas);
        Dados.curraisPerguntasMenino |= 1;
        GameProgress.Instance.UnlockDiaryPage("currais-menino-espera");
        Salvar();
    }

    private bool InteragirCriancaERegistros(Vector2 ponto)
    {
        if (areaLataMenino.enabled && areaLataMenino.gameObject.activeInHierarchy && areaLataMenino.OverlapPoint(ponto))
        { Iniciar(ExaminarLataMenino()); return true; }
        if (areaMenino.enabled && areaMenino.gameObject.activeInHierarchy && areaMenino.OverlapPoint(ponto))
        {
            Iniciar(Dados.curraisMeninoVisitado ? ReencontrarMenino() : ConhecerMenino());
            return true;
        }
        if (areaCanhotoMesa.enabled && areaCanhotoMesa.gameObject.activeInHierarchy && areaCanhotoMesa.OverlapPoint(ponto))
        { Iniciar(EscolherPertence()); return true; }
        if (areaFolhasRegistro.enabled && areaFolhasRegistro.gameObject.activeInHierarchy && areaFolhasRegistro.OverlapPoint(ponto))
        {
            Iniciar(Dizer(ComAcompanhante(Dados.curraisEscolhaAgua == 0 ? mesaAntesAgua : folhasRegistroFalas)));
            return true;
        }
        return false;
    }

    private void AtualizarRegistrosVisuais()
    {
        bool presente = Dados.curraisEtapaEntrada == 4;
        folhasAntesRegistro.SetActive(presente && !Dados.curraisRegistroConcluido);
        folhasDepoisRegistro.SetActive(presente && Dados.curraisRegistroConcluido);
        areaFolhasRegistro.enabled = presente && !Dados.curraisPanelaConcluida;
        areaCanhotoMesa.enabled = presente && Dados.curraisLotePanosRecolhido && !Dados.curraisPanelaConcluida;
        lotePanosComMari.SetActive(presente && Dados.curraisLotePanosRecolhido &&
            !Dados.curraisLotePanosEntregue && TemItem(itemLotePanos));
        areaLataMenino.enabled = presente && Dados.curraisEscolhaAgua != 0;
    }

    private IEnumerator ConsultarFolhaNaMesa()
    {
        if (consultandoFolhaRegistro) yield break;
        Transform folha = Dados.curraisRegistroConcluido ? folhasDepoisRegistro.transform : folhaEmConsulta;
        Vector3 origem = folha.position;
        try
        {
            consultandoFolhaRegistro = true;
            OlharParaMari(areaRegistro.transform);
            yield return DeslocarFolhaRegistro(folha, folhaConsultaFim.position, .45f);
            yield return Esperar(.25f);
            yield return Mover(folha, origem, .35f);
        }
        finally
        {
            consultandoFolhaRegistro = false;
            folha.position = origem;
        }
    }
    private IEnumerator DeslocarFolhaRegistro(Transform folha, Vector3 destino, float duracao)
    {
        Vector3 origem = folha.position;
        float tempo = 0f;
        while (tempo < duracao)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                folha.position = Vector3.Lerp(origem, destino, Mathf.Clamp01(tempo / duracao));
            }
            yield return null;
        }
    }

}
