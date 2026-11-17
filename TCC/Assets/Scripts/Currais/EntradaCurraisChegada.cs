using System;
using System.Collections;
using UnityEngine;

// Cenas 1–3 do roteiro alternativo. Objetos, marcas e falas permanecem na Hierarchy.
public partial class EntradaCurrais
{
    [Header("Chegada alternativa — escolta")]
    [SerializeField] private Transform guardaRetaguarda, deslocamentoEscolta, cameraRevista;
    [SerializeField] private Transform guardaSeparacao, guardaGolpe, paiReacao;
    [SerializeField] private Fala[] ordemSeparar, protestoPai, reacaoGolpe;

    [Header("Chegada alternativa — revista e diário")]
    [SerializeField] private Collider2D areaFiscalEntrada, areaDiarioEntrada;
    [SerializeField] private GameObject diarioEntrada;
    [SerializeField] private Fala[] diarioFalas;
    [SerializeField, TextArea] private string objetivoRevista;

    [Header("Chegada alternativa — acomodar o pai")]
    [SerializeField] private Collider2D areaPanoAbrigo;
    [SerializeField] private GameObject panoAbrigo, panoNoPai;
    [SerializeField] private ItemData itemPanoAbrigo;
    [SerializeField] private Fala[] panoDisponivel, paiAcomodado;
    [SerializeField] private Fala[] luziaCurral, luziaSaida, luziaIrma;
    [SerializeField, TextArea] private string objetivoPano;

    private bool ReferenciasChegadaValidas()
    {
        UnityEngine.Object[] refs = { guardaRetaguarda, deslocamentoEscolta, guardaSeparacao,
            guardaGolpe, paiReacao, cameraRevista, areaFiscalEntrada, areaDiarioEntrada, diarioEntrada,
            areaPanoAbrigo, panoAbrigo, panoNoPai, itemPanoAbrigo };
        string[] nomes = { nameof(guardaRetaguarda), nameof(deslocamentoEscolta), nameof(guardaSeparacao),
            nameof(guardaGolpe), nameof(paiReacao), nameof(cameraRevista), nameof(areaFiscalEntrada), nameof(areaDiarioEntrada), nameof(diarioEntrada),
            nameof(areaPanoAbrigo), nameof(panoAbrigo), nameof(panoNoPai), nameof(itemPanoAbrigo) };
        bool valido = true;
        for (int i = 0; i < refs.Length; i++) valido &= ConferirReferencia(refs[i], nomes[i]);
        foreach (Fala[] falas in new[] { ordemSeparar, protestoPai, reacaoGolpe, diarioFalas,
            panoDisponivel, paiAcomodado, luziaCurral, luziaSaida, luziaIrma }) valido &= FalasValidas(falas);
        return valido;
    }

    private void PrepararChegadaAlternativa()
    {
        // Saves que já chegaram à exploração continuam dali; não recebem uma nova obrigação.
        if (Dados.curraisEtapaEntrada >= 4)
        {
            Dados.curraisDiarioRecuperado = true;
            Dados.curraisPaiAcomodado = true;
        }
        lacoChao.SetActive(false);
        lacoPanela.SetActive(false);
        AtualizarVisuaisChegada();
    }

    private IEnumerator MarcharEscolta()
    {
        Transform[] grupo = { cabo, mari.transform, pai, mae, baleia, irma, guardaRetaguarda };
        Vector3[] origens = new Vector3[grupo.Length];
        // A marca é um deslocamento local editável: todos percorrem a mesma distância.
        Vector3 delta = deslocamentoEscolta.localPosition;
        for (int i = 0; i < grupo.Length; i++) origens[i] = grupo[i].position;
        float tempo = 0f;
        while (tempo < tempoSeparacao)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                for (int i = 0; i < grupo.Length; i++)
                {
                    grupo[i].position = Vector3.Lerp(origens[i], origens[i] + delta, Mathf.Clamp01(tempo / tempoSeparacao));
                    AnimarEscolta(grupo[i], true);
                }
                Rigidbody2D corpo = mari.GetComponent<Rigidbody2D>();
                if (corpo != null) { corpo.position = mari.transform.position; corpo.linearVelocity = Vector2.zero; }
            }
            else foreach (Transform pessoa in grupo) AnimarEscolta(pessoa, false);
            yield return null;
        }
        foreach (Transform pessoa in grupo) AnimarEscolta(pessoa, false);
        Physics2D.SyncTransforms();
    }

    private static void AnimarEscolta(Transform pessoa, bool andando)
    {
        Animator animador = pessoa.GetComponent<Animator>();
        if (animador != null && animador.enabled)
        {
            animador.SetBool("IsWalking", andando);
            animador.SetBool("IsRunning", false);
        }
    }

    private Collider2D[] AlvosChegada() => Dados.curraisEtapaEntrada == 2
        ? new[] { areaDiarioEntrada, areaDiarioRetido, areaFiscalEntrada, areaMesaRevista }
        : new[] { areaPanoAbrigo, areaLuzia, areaMae, areaPaiAbrigo };

    private bool PonteiroSobreChegada(Vector2 ponto)
    {
        foreach (Collider2D area in AlvosChegada())
            if (area != null && area.enabled && area.gameObject.activeInHierarchy && area.OverlapPoint(ponto)) return true;
        return false;
    }

    private void AtualizarInteracaoChegada()
    {
        if (ocupado || menuAberto || Time.frameCount <= quadroBloqueado) return;
        Collider2D alvo = null;
        if (Input.GetKeyDown(KeyCode.E) && !InventarioAbertoMemoria())
        {
            float menor = alcanceInteracao;
            foreach (Collider2D area in AlvosChegada())
            {
                if (area == null || !area.enabled || !area.gameObject.activeInHierarchy) continue;
                float distancia = Mathf.Abs(mari.transform.position.x - area.bounds.center.x);
                if (distancia < menor) { alvo = area; menor = distancia; }
            }
        }
        else if (Input.GetMouseButtonDown(0) && !SobreInterface() &&
            InvestigationGuard.TryGetPointerPosition(out Vector2 ponto))
        {
            foreach (Collider2D area in AlvosChegada())
                if (area != null && area.enabled && area.gameObject.activeInHierarchy && area.OverlapPoint(ponto))
                { alvo = area; break; }
        }
        if (alvo == null) return;

        if (alvo == areaFiscalEntrada || alvo == areaMesaRevista) MenuRevista();
        else if (alvo == areaDiarioRetido) Iniciar(Dizer(cadernoEmConferencia));
        else if (alvo == areaDiarioEntrada) Iniciar(RecuperarDiarioEntrada());
        else if (alvo == areaPanoAbrigo) Iniciar(RecolherPanoAbrigo());
        else if (alvo == areaLuzia) MenuPrimeiroAbrigo();
        else if (alvo == areaMae) MenuMaePrimeiroAbrigo();
        else if (alvo == areaPaiAbrigo) Iniciar(ConversarPaiPrimeiroAbrigo());
    }

    private void MenuRevista() => MenuConferenciaRevista();

    public bool SoltarItemNoCenario(ItemData item, Vector2 posicaoTela)
    {
        if (!isActiveAndEnabled || !pronto || ocupado || menuAberto || dialogando || encerrando ||
            PauseHelper.BlockInput() || Time.timeScale <= 0f || item == null || !TemItem(item) ||
            SobreInterface(posicaoTela) || Time.frameCount <= quadroBloqueado || PainelAuxiliarAberto()) return false;
        if (Dados.curraisEtapaEntrada == 2) return SoltarFerramentaNaRevista(item, posicaoTela);
        if (mari == null || cameraCena == null) return false;
        Ray raio = cameraCena.ScreenPointToRay(posicaoTela);
        Plane plano = new Plane(Vector3.forward, mari.transform.position);
        if (!plano.Raycast(raio, out float distancia)) return false;
        Vector2 ponto = raio.GetPoint(distancia);
        Collider2D[] alvos = Dados.curraisEtapaEntrada == 3 ? AlvosChegada() : AlvosAtuais();
        foreach (Collider2D alvo in alvos)
        {
            if (alvo == null || !alvo.enabled || !alvo.gameObject.activeInHierarchy || !alvo.OverlapPoint(ponto)) continue;

            InventoryManager.Instance.SelectItem(item);
            if (Dados.curraisEtapaEntrada == 3 && alvo == areaMae)
            {
                if (item != itemPanoAbrigo) return false;
                Iniciar(AcomodarPai()); return true;
            }
            if (Dados.curraisEtapaEntrada == 4)
            {
                if (!Dados.curraisPanelaConcluida &&
                    ((alvo == areaAgua && item != cabacaVaziaBusca) ||
                     ((alvo == areaMae || alvo == areaLuzia) && item != cabacaCheiaBusca))) return false;
                return InteragirBusca(ponto);
            }
            return false;
        }
        return false;
    }

    public bool SoltarFerramentaNaRevista(ItemData item, Vector2 posicaoTela)
    {
        // O inventário deve permanecer aberto durante o arraste; pausa e diálogos bloqueiam.
        if (!isActiveAndEnabled || !pronto || ocupado || menuAberto || dialogando || encerrando ||
            PauseHelper.BlockInput() || Time.timeScale <= 0f || Dados.curraisEtapaEntrada != 2 ||
            Dados.curraisPertencesRetidos || Time.frameCount <= quadroBloqueado || PainelAuxiliarAberto() ||
            SobreInterface(posicaoTela) || item == null || !TemItem(item) || mari == null ||
            areaFiscalEntrada == null || !areaFiscalEntrada.enabled ||
            !areaFiscalEntrada.gameObject.activeInHierarchy) return false;

        Camera camera = cameraCena;
        if (camera == null || camera.gameObject.scene != gameObject.scene) return false;
        Ray raio = camera.ScreenPointToRay(posicaoTela);
        Plane plano = new Plane(Vector3.forward, Vector3.zero);
        if (!plano.Raycast(raio, out float distancia) ||
            (!areaFiscalEntrada.OverlapPoint(raio.GetPoint(distancia)) &&
             (areaMesaRevista == null || !areaMesaRevista.enabled ||
              !areaMesaRevista.OverlapPoint(raio.GetPoint(distancia))))) return false;


        Iniciar(EntregarFerramenta(item));
        return true;
    }

    private IEnumerator EntregarFerramenta(ItemData item)
    {
        yield return EntregarPertenceRevista(item);
    }

    private IEnumerator ConcluirRevista()
    {
        yield return ConferirTodosOsPertences();
    }

    private IEnumerator RecuperarDiarioEntrada()
    {
        if (!Dados.curraisPertencesRetidos || Dados.curraisDiarioRecuperado || revistaEmConferencia) yield break;
        yield return Escolher("Zefinha", "O Fiscal deixou meu caderno separado.",
            new[] { "Levar o caderno", "Deixar por enquanto" });
        if (escolhaRecebida != 0) yield break;
        yield return Dizer(diarioFalas);
        Dados.curraisDiarioRecuperado = true;
        // O diário usa a leitura da pausa; não cria uma segunda cópia do item do presente.
        Dados.curraisEtapaEntrada = 3;
        AtualizarVisuaisChegada();
        Salvar();
        yield return ChegarAbrigo();
    }

    private void MenuPrimeiroAbrigo()
    {
        MostrarOpcoes("Luzia", "Pode perguntar, Josefa. Sua mãe tá aqui com seu pai.",
            new[] { "Por que chamam aqui de curral?", "Quem pode sair?", "A senhora viu minha irmã?", "Sair" },
            new Action[] { () => Iniciar(PerguntarAbrigo(1, luziaCurral)),
                () => Iniciar(PerguntarAbrigo(2, luziaSaida)), () => Iniciar(PerguntarAbrigo(4, luziaIrma)), FecharMenu });
    }

    private IEnumerator PerguntarAbrigo(int bit, Fala[] falas)
    {
        yield return Dizer(falas);
        Dados.curraisPerguntasAbrigo |= bit;
        Salvar();
        MenuPrimeiroAbrigo();
    }

    private IEnumerator RecolherPanoAbrigo()
    {
        if (Dados.curraisPanoAbrigoRecolhido || Dados.curraisPaiAcomodado) yield break;
        yield return ExaminarOuColetar(itemPanoAbrigo);
        if (!coletaDoCenarioConfirmada) yield break;
        if (!TemItem(itemPanoAbrigo)) InventoryManager.Instance.AddItem(itemPanoAbrigo);
        Dados.curraisPanoAbrigoRecolhido = true;
        AtualizarVisuaisChegada();
        Salvar();
        yield return Dizer(panoDisponivel);
        objetivoTexto.text = string.Empty;
    }

    private IEnumerator AcomodarPai()
    {
        if (Dados.curraisPaiAcomodado) { yield return Dizer(paiAcomodado); yield break; }
        if (!TemItem(itemPanoAbrigo))
        {
            yield return Dizer(maePrecisaPano);
            yield break;
        }
        yield return Mover(mae, maeCuidadoAbrigo.position, tempoPlano);
        if (!TemItem(itemPanoAbrigo)) { yield return Dizer(maePrecisaPano); yield break; }
        InventoryManager.Instance.RemoveItem(itemPanoAbrigo);
        Dados.curraisPaiAcomodado = true;
        AtualizarVisuaisChegada();
        Dados.curraisEtapaEntrada = 4;
        AtualizarEstadoBusca();
        Salvar();
        yield return Dizer(paiAcomodado);
    }

    private void AtualizarVisuaisChegada()
    {
        bool revistaAtiva = Dados.curraisEtapaEntrada == 2;
        areaFiscalEntrada.enabled = revistaAtiva;
        diarioEntrada.SetActive(revistaAtiva && Dados.curraisPertencesRetidos && !Dados.curraisDiarioRecuperado && !revistaEmConferencia);
        areaDiarioEntrada.enabled = diarioEntrada.activeSelf;
        panoAbrigo.SetActive(Dados.curraisEtapaEntrada == 3 && !Dados.curraisPanoAbrigoRecolhido && !Dados.curraisPaiAcomodado);
        areaPanoAbrigo.enabled = panoAbrigo.activeSelf;
        panoNoPai.SetActive(Dados.curraisPaiAcomodado && (Dados.curraisEtapaBusca < 1 ||
            (Dados.curraisEtapaBusca >= 4 && Dados.curraisEtapaBusca <= 5)));
        AtualizarMesaRevista();
        AtualizarAbrigoVisual();
    }

}
