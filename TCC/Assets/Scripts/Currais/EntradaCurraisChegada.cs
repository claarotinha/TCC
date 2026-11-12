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
    [SerializeField] private ItemData[] ferramentasRevista;
    [SerializeField] private Fala[] diarioFalas;
    [SerializeField, TextArea] private string objetivoRevista;

    [Header("Chegada alternativa — acomodar o pai")]
    [SerializeField] private Collider2D areaPanoAbrigo;
    [SerializeField] private GameObject panoAbrigo, panoNoPai;
    [SerializeField] private Transform limiteAbrigoEsquerda, limiteAbrigoDireita;
    [SerializeField] private ItemData itemPanoAbrigo;
    [SerializeField] private Fala[] panoDisponivel, paiAcomodado;
    [SerializeField] private Fala[] luziaCurral, luziaSaida, luziaIrma;
    [SerializeField, TextArea] private string objetivoPano;

    private bool ReferenciasChegadaValidas()
    {
        UnityEngine.Object[] refs = { guardaRetaguarda, deslocamentoEscolta, guardaSeparacao,
            guardaGolpe, paiReacao, cameraRevista, areaFiscalEntrada, areaDiarioEntrada, diarioEntrada,
            areaPanoAbrigo, panoAbrigo, panoNoPai, itemPanoAbrigo, limiteAbrigoEsquerda, limiteAbrigoDireita };
        string[] nomes = { nameof(guardaRetaguarda), nameof(deslocamentoEscolta), nameof(guardaSeparacao),
            nameof(guardaGolpe), nameof(paiReacao), nameof(cameraRevista), nameof(areaFiscalEntrada), nameof(areaDiarioEntrada), nameof(diarioEntrada),
            nameof(areaPanoAbrigo), nameof(panoAbrigo), nameof(panoNoPai), nameof(itemPanoAbrigo),
            nameof(limiteAbrigoEsquerda), nameof(limiteAbrigoDireita) };
        bool valido = true;
        for (int i = 0; i < refs.Length; i++) valido &= ConferirReferencia(refs[i], nomes[i]);
        if (ferramentasRevista == null || ferramentasRevista.Length == 0) return false;
        foreach (ItemData item in ferramentasRevista) valido &= ConferirReferencia(item, nameof(ferramentasRevista));
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
        ? new[] { areaFiscalEntrada, areaDiarioEntrada }
        : new[] { areaPanoAbrigo, areaLuzia, areaMae };

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
        if (Input.GetKeyDown(KeyCode.E))
        {
            float menor = alcanceInteracao;
            foreach (Collider2D area in AlvosChegada())
            {
                if (area == null || !area.enabled || !area.gameObject.activeInHierarchy) continue;
                float distancia = Mathf.Abs(mari.transform.position.x - area.bounds.center.x);
                if (distancia < menor) { alvo = area; menor = distancia; }
            }
        }
        else if (Input.GetMouseButtonDown(0) && !SobreControle() &&
            InvestigationGuard.TryGetPointerPosition(out Vector2 ponto))
        {
            foreach (Collider2D area in AlvosChegada())
                if (area != null && area.enabled && area.gameObject.activeInHierarchy && area.OverlapPoint(ponto))
                { alvo = area; break; }
        }
        if (alvo == null) return;
        if (Mathf.Abs(mari.transform.position.x - alvo.bounds.center.x) > alcanceInteracao)
        { objetivoTexto.text = "Aproxime-se para interagir. " + (Dados.curraisEtapaEntrada == 2 ? objetivoRevista : objetivoPano); return; }
        if (alvo == areaFiscalEntrada) MenuRevista();
        else if (alvo == areaDiarioEntrada) Iniciar(RecuperarDiarioEntrada());
        else if (alvo == areaPanoAbrigo) Iniciar(RecolherPanoAbrigo());
        else if (alvo == areaLuzia) MenuPrimeiroAbrigo();
        else if (alvo == areaMae) Iniciar(AcomodarPai());
    }

    private void MenuRevista()
    {
        MostrarOpcoes("Fiscal", objetivoRevista,
            new[] { "Como entregar os pertences?", "Conferir a revista", "Sair" }, new Action[] {
                () => Iniciar(Dizer(new[] { new Fala { personagem = "Zefinha", texto =
                    "Vou abrir o inventário com TAB, arrastar cada ferramenta e soltar sobre o cabo. Depois confiro a revista com ele." } })),
                () => Iniciar(ConcluirRevista()), FecharMenu });
    }

    public bool SoltarItemNoCenario(ItemData item, Vector2 posicaoTela)
    {
        if (!isActiveAndEnabled || !pronto || ocupado || menuAberto || dialogando || encerrando ||
            PauseHelper.BlockInput() || Time.timeScale <= 0f || item == null || !TemItem(item) ||
            SobreControle(posicaoTela)) return false;
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
            if (Mathf.Abs(mari.transform.position.x - alvo.bounds.center.x) > alcanceInteracao)
            { objetivoTexto.text = "Aproxime-se para entregar o objeto."; return false; }
            InventoryManager.Instance.SelectItem(item);
            if (Dados.curraisEtapaEntrada == 3 && alvo == areaMae)
            { Iniciar(AcomodarPai()); return true; }
            if (Dados.curraisEtapaEntrada == 4) return InteragirBusca(ponto);
            return false;
        }
        return false;
    }

    public bool SoltarFerramentaNaRevista(ItemData item, Vector2 posicaoTela)
    {
        // O inventário deve permanecer aberto durante o arraste; pausa e diálogos bloqueiam.
        if (!isActiveAndEnabled || !pronto || ocupado || menuAberto || dialogando || encerrando ||
            PauseHelper.BlockInput() || Time.timeScale <= 0f || Dados.curraisEtapaEntrada != 2 ||
            Dados.curraisPertencesRetidos || item == null || ferramentasRevista == null ||
            Array.IndexOf(ferramentasRevista, item) < 0 || !TemItem(item) || mari == null ||
            areaFiscalEntrada == null || !areaFiscalEntrada.enabled ||
            !areaFiscalEntrada.gameObject.activeInHierarchy) return false;

        Camera camera = Camera.main;
        if (camera == null || camera.gameObject.scene != gameObject.scene) return false;
        Ray raio = camera.ScreenPointToRay(posicaoTela);
        Plane plano = new Plane(Vector3.forward, Vector3.zero);
        if (!plano.Raycast(raio, out float distancia) ||
            !areaFiscalEntrada.OverlapPoint(raio.GetPoint(distancia))) return false;
        if (Mathf.Abs(mari.transform.position.x - areaFiscalEntrada.bounds.center.x) > alcanceInteracao)
        {
            objetivoTexto.text = "Aproxime-se do cabo para entregar as ferramentas.";
            return false;
        }

        Iniciar(EntregarFerramenta(item));
        return true;
    }

    private IEnumerator EntregarFerramenta(ItemData item)
    {
        InventorySaveCatalog catalogo = Resources.Load<InventorySaveCatalog>("InventorySaveCatalog");
        if (catalogo == null || !catalogo.TryGetId(item, out string id))
        { Debug.LogError("Currais: ferramenta sem registro no catálogo; entrega cancelada.", this); yield break; }
        Dados.curraisItensRetidos.Add(id);
        InventoryManager.Instance.RemoveItem(item);
        objetivoTexto.text = objetivoRevista;
        Salvar();
        yield return Dizer(new[] { new Fala { personagem = "Fiscal", texto = item.itemName + ": deixe na mesa. O resto também vai passar pela revista." } });
    }

    private IEnumerator ConcluirRevista()
    {
        if (Dados.curraisPertencesRetidos)
        { objetivoTexto.text = "Recolha seu caderno na mesa do fiscal."; yield break; }
        foreach (ItemData ferramenta in ferramentasRevista)
            if (TemItem(ferramenta))
            {
                yield return Dizer(new[] { new Fala { personagem = "Fiscal", texto =
                    "Ainda tem ferramenta com você. Abra o inventário, arraste cada uma e solte sobre mim." } });
                yield break;
            }
        if (!ReterPertences())
        { objetivoTexto.text = "Não foi possível registrar os pertences. Eles continuam no inventário."; yield break; }
        yield return Dizer(revista);
        AtualizarVisuaisChegada();
        objetivoTexto.text = "O fiscal deixou seu caderno na mesa. Clique para recolher.";
        Salvar();
    }

    private IEnumerator RecuperarDiarioEntrada()
    {
        if (!Dados.curraisPertencesRetidos || Dados.curraisDiarioRecuperado) yield break;
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
        objetivoTexto.text = "O pano está no inventário. Arraste até sua mãe, ou selecione e clique nela.";
    }

    private IEnumerator AcomodarPai()
    {
        if (!TemItem(itemPanoAbrigo) || InventoryManager.Instance.SelectedItem != itemPanoAbrigo)
        {
            yield return Dizer(new[] { new Fala { personagem = "Mãe", texto =
                "Luzia deixou um pano pra nós. Pegue, selecione no inventário e traga pra cá, minha filha." } });
            yield break;
        }
        InventoryManager.Instance.RemoveItem(itemPanoAbrigo);
        Dados.curraisPaiAcomodado = true;
        AtualizarVisuaisChegada();
        yield return Dizer(paiAcomodado);
        Dados.curraisEtapaEntrada = 4;
        AtualizarEstadoBusca();
        Salvar();
    }

    private void AtualizarVisuaisChegada()
    {
        bool revistaAtiva = Dados.curraisEtapaEntrada == 2;
        areaFiscalEntrada.enabled = revistaAtiva;
        diarioEntrada.SetActive(revistaAtiva && Dados.curraisPertencesRetidos && !Dados.curraisDiarioRecuperado);
        areaDiarioEntrada.enabled = diarioEntrada.activeSelf;
        panoAbrigo.SetActive(Dados.curraisEtapaEntrada == 3 && !Dados.curraisPanoAbrigoRecolhido && !Dados.curraisPaiAcomodado);
        areaPanoAbrigo.enabled = panoAbrigo.activeSelf;
        panoNoPai.SetActive(Dados.curraisPaiAcomodado && Dados.curraisEtapaBusca < 1);
    }

    private void LimitarPrimeiroAbrigo()
    {
        BoxCollider2D limites = limiteSetor as BoxCollider2D;
        if (limites == null) return;
        // Guarda a borda original antes de restringir este pequeno trecho.
        if (limiteEsquerdoBusca == 0f) limiteEsquerdoBusca = limites.bounds.min.x;
        float esquerda = limiteAbrigoEsquerda.position.x, direita = limiteAbrigoDireita.position.x;
        Vector3 centro = limites.transform.InverseTransformPoint(new Vector3((esquerda + direita) * .5f,
            limites.bounds.center.y, limites.transform.position.z));
        limites.offset = new Vector2(centro.x, limites.offset.y);
        limites.size = new Vector2((direita - esquerda) / Mathf.Abs(limites.transform.lossyScale.x), limites.size.y);
        Physics2D.SyncTransforms();
        if (seguirCamera != null) seguirCamera.AtualizarLimites();
    }
}
