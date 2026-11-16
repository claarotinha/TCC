using System;
using System.Collections;
using UnityEngine;

public partial class EntradaCurrais
{
    [Header("Primeiro abrigo — elementos editáveis")]
    [SerializeField] private Collider2D areaPaiAbrigo;
    [SerializeField] private Transform maeCuidadoAbrigo;
    [SerializeField] private SpriteRenderer cabacaComMari, cabacaComMae, cabacaComLuzia;
    [SerializeField] private Fala[] maePrecisaPano, paiRepouso, paiDepoisAgua;
    [SerializeField] private Fala[] luziaBuscarAgua, aguaAguardar, aguaApresentar, aguaJaRecebida;
    [SerializeField] private Fala[] maeEsperaAgua, familiaDepoisAgua, cabacaIndisponivel;
    private int recebendoAguaAbrigo;

    private bool ReferenciasAbrigoValidas()
    {
        return ConferirReferencia(areaPaiAbrigo, nameof(areaPaiAbrigo)) &
            ConferirReferencia(maeCuidadoAbrigo, nameof(maeCuidadoAbrigo)) &
            ConferirReferencia(cabacaComMari, nameof(cabacaComMari)) &
            ConferirReferencia(cabacaComMae, nameof(cabacaComMae)) &
            ConferirReferencia(cabacaComLuzia, nameof(cabacaComLuzia)) &
            FalasValidas(maePrecisaPano) & FalasValidas(paiRepouso) &
            FalasValidas(paiDepoisAgua) & FalasValidas(luziaBuscarAgua) &
            FalasValidas(aguaAguardar) & FalasValidas(aguaApresentar) &
            FalasValidas(aguaJaRecebida) & FalasValidas(maeEsperaAgua) &
            FalasValidas(familiaDepoisAgua) & FalasValidas(cabacaIndisponivel);
    }

    private void MenuMaePrimeiroAbrigo()
    {
        if (Dados.curraisPaiAcomodado)
        {
            if (Dados.curraisAguaRecebida && Dados.curraisEscolhaAgua == 0)
                Iniciar(DestinarAgua());
            else Iniciar(Dizer(Dados.curraisEscolhaAgua == 0 ? maeEsperaAgua : familiaDepoisAgua));
            return;
        }
        MostrarOpcoes("Mãe", maePrecisaPano[0].texto,
            new[] { "Entregar o pano", "Como ele está?", "Sair" },
            new Action[] { () => Iniciar(AcomodarPai()), () => Iniciar(Dizer(paiRepouso)), FecharMenu });
    }

    private IEnumerator ConversarPaiPrimeiroAbrigo()
    {
        yield return Dizer(Dados.curraisEscolhaAgua == 0 ? paiRepouso : paiDepoisAgua);
    }

    private IEnumerator IniciarBuscaAguaAbrigo()
    {
        yield return Dizer(luziaBuscarAgua);
        Dados.curraisDistribuicaoIniciada = true;
        AtualizarEstadoBusca();
        Salvar();
    }

    private void AtualizarAbrigoVisual()
    {
        bool primeiroAbrigo = Dados.curraisEtapaEntrada >= 3 &&
            Dados.curraisEtapaEntrada <= 4 && !Dados.curraisPanelaConcluida;
        areaPaiAbrigo.enabled = primeiroAbrigo;
        bool agua = primeiroAbrigo && Dados.curraisAguaRecebida;
        cabacaComMari.gameObject.SetActive(agua && Dados.curraisEscolhaAgua == 0 && recebendoAguaAbrigo == 0);
        cabacaComMae.gameObject.SetActive(agua && (recebendoAguaAbrigo == 1 || Dados.curraisEscolhaAgua == 1));
        cabacaComLuzia.gameObject.SetActive(agua && (recebendoAguaAbrigo == 2 || Dados.curraisEscolhaAgua == 2));
        cabacaComMari.color = new Color(.65f, .85f, 1f, 1f);
        Color corEntrega = recebendoAguaAbrigo != 0 ? cabacaComMari.color : Color.white;
        cabacaComMae.color = corEntrega;
        cabacaComLuzia.color = corEntrega;
    }

    private IEnumerator BeberAguaAbrigo(int destinatario)
    {
        SpriteRenderer visual = destinatario == 1 ? cabacaComMae : cabacaComLuzia;
        Quaternion original = visual.transform.localRotation;
        try
        {
            recebendoAguaAbrigo = destinatario;
            AtualizarAbrigoVisual();
            float tempo = 0f;
            while (tempo < .7f)
            {
                if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
                {
                    tempo += Time.deltaTime;
                    float inclinacao = Mathf.Sin(Mathf.Clamp01(tempo / .7f) * Mathf.PI) * 15f;
                    visual.transform.localRotation = original * Quaternion.Euler(0f, 0f, inclinacao);
                }
                yield return null;
            }
        }
        finally
        {
            visual.transform.localRotation = original;
            recebendoAguaAbrigo = 0;
            if (!encerrando && GameProgress.Instance != null) AtualizarAbrigoVisual();
        }
    }
}
