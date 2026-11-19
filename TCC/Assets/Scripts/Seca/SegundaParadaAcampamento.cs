using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class SegundaParada
{
    private enum FaseAcampamento { Chegada, Materiais, Refeicao, Familia, Irmas, Pais, Amanhecer, Concluido }
    private FaseAcampamento faseAcampamento;
    [Header("Fogueira reconstruída - elementos da cena")]
    [SerializeField] private SpriteRenderer[] personagensFogueira;
    [SerializeField] private Transform enquadramentoFogueira;
    [SerializeField, Min(1f)] private float zoomFogueira = 4.7f;
    [SerializeField] private Image cortinaAcampamento;
    [SerializeField] private FogueiraEstradaVisual visualFogueira;
    private bool acampamentoConfigurado;

    private FaseAcampamento RestaurarFaseAcampamento()
    {
        if (acampamentoConcluido) return FaseAcampamento.Concluido;
        if (conversaPaisConcluida) return FaseAcampamento.Amanhecer;
        if (conversaIrmasConcluida) return FaseAcampamento.Pais;
        if (conversaFogueiraConcluida) return FaseAcampamento.Irmas;
        if (refeicaoPreparada) return FaseAcampamento.Familia;
        if (fogueiraPreparada) return FaseAcampamento.Refeicao;
        return introAcampamentoConcluida ? FaseAcampamento.Materiais : FaseAcampamento.Chegada;
    }

    private void PrepararFadeAcampamento()
    {
        cortinaAcampamento.gameObject.SetActive(true);
        cortinaAcampamento.transform.SetAsLastSibling();
        cortinaAcampamento.raycastTarget = true;
    }

    private IEnumerator EscurecerAcampamento(float alpha)
    {
        float inicio = cortinaAcampamento.color.a;
        float tempo = 0f;
        while (tempo < duracaoTransicao)
        {
            if (!PauseHelper.BlockInput()) tempo += Time.deltaTime;
            cortinaAcampamento.color = new Color(0f, 0f, 0f,
                Mathf.Lerp(inicio, alpha, Mathf.Clamp01(tempo / Mathf.Max(.1f, duracaoTransicao))));
            yield return null;
        }
        cortinaAcampamento.color = new Color(0f, 0f, 0f, alpha);
    }

    private void RestaurarFadeAcampamento()
    {
        if (cortinaAcampamento == null) return;
        cortinaAcampamento.color = Color.clear;
        cortinaAcampamento.gameObject.SetActive(false);
    }

    private void DefinirPersonagensFogueira(bool ativos)
    {
        if (personagensFogueira == null) return;
        foreach (SpriteRenderer pessoa in personagensFogueira)
            if (pessoa != null) pessoa.gameObject.SetActive(ativos);
    }

    private void PrepararAcampamento()
    {
        DefinirPersonagensFogueira(false);
        if (GameProgress.Instance != null)
        {
            GameProgressData d = GameProgress.Instance.Data;
            acampamentoIniciado = d.estradaAcampamentoIniciado;
            introAcampamentoConcluida = d.estradaIntroAcampamentoConcluida;
            materiaisAcampamento = d.estradaMateriaisAcampamento;
            fogueiraPreparada = d.estradaFogueiraPreparada;
            pedidoAlecrimConcluido = d.estradaPedidoAlecrimConcluido;
            refeicaoPreparada = d.estradaRefeicaoPreparada;
            conversaFogueiraConcluida = d.estradaConversaFogueiraConcluida;
            primeiraEscolhaNoturna = d.estradaPrimeiraEscolhaNoturna;
            segundaEscolhaNoturna = d.estradaSegundaEscolhaNoturna;
            conversaIrmasConcluida = d.estradaConversaIrmasConcluida;
            conversaPaisConcluida = d.estradaConversaPaisConcluida;
            acampamentoConcluido = d.estradaAcampamentoConcluido;
        }
        RecuperarReferenciasAcampamento();
        var faltando = new List<string>();
        void Exigir(UnityEngine.Object referencia, string campo)
        {
            if (referencia == null) faltando.Add(campo);
        }
        Exigir(quintaParada, nameof(quintaParada));
        Exigir(arvoreAcampamento, nameof(arvoreAcampamento));
        Exigir(fogueiraCenario, nameof(fogueiraCenario));
        Exigir(enquadramentoFogueira, nameof(enquadramentoFogueira));
        Exigir(cortinaAcampamento, nameof(cortinaAcampamento));
        Exigir(visualFogueira, nameof(visualFogueira));
        Exigir(galhosSecos, nameof(galhosSecos));
        Exigir(folhasSecas, nameof(folhasSecas));
        Exigir(galhosAlecrim, nameof(galhosAlecrim));
        Exigir(passaro, nameof(passaro));
        Exigir(prea, nameof(prea));
        Exigir(noiteTelaCheia, nameof(noiteTelaCheia));
        Exigir(imagemNoite, nameof(imagemNoite));
        Exigir(imagemIrmasDormindo, nameof(imagemIrmasDormindo));
        Exigir(imagemPaisDormindo, nameof(imagemPaisDormindo));
        if (personagensFogueira == null || personagensFogueira.Length != 4)
            faltando.Add("personagensFogueira (quatro personagens)");
        else
            for (int i = 0; i < personagensFogueira.Length; i++)
            {
                Exigir(personagensFogueira[i], $"personagensFogueira[{i}]");
                if (personagensFogueira[i] != null)
                    Exigir(personagensFogueira[i].sprite, personagensFogueira[i].name + ".sprite");
            }
        if (quadrosIrmasAcordadas == null || quadrosIrmasAcordadas.Length == 0)
            faltando.Add(nameof(quadrosIrmasAcordadas));
        if (quadrosIrmasDormindo == null || quadrosIrmasDormindo.Length == 0)
            faltando.Add(nameof(quadrosIrmasDormindo));
        if (quadrosPaisDeitados == null || quadrosPaisDeitados.Length == 0)
            faltando.Add(nameof(quadrosPaisDeitados));
        acampamentoConfigurado = faltando.Count == 0;
        if (!acampamentoConfigurado)
        {
            Debug.LogError("Fogueira: referências ausentes: " + string.Join(", ", faltando), this);
            return;
        }
        faseAcampamento = RestaurarFaseAcampamento();

        RestaurarFadeAcampamento();
        noiteTelaCheia.SetActive(false);
        AtualizarMateriaisAcampamento();
    }

    private void RecuperarReferenciasAcampamento()
    {
        // Recupera componentes dos próprios objetos editados na Hierarchy, sem criar outra cena.
        if (visualFogueira == null && fogueiraCenario != null)
            visualFogueira = fogueiraCenario.GetComponent<FogueiraEstradaVisual>();
        if (enquadramentoFogueira == null && quintaParada != null && quintaParada.parent != null)
            enquadramentoFogueira = quintaParada.parent.Find("Câmera da fogueira - EDITAR enquadramento");
        if (cortinaAcampamento == null && noiteTelaCheia != null)
        {
            Transform cortina = noiteTelaCheia.transform.parent.Find("Transição da fogueira - cortina preta");
            if (cortina != null) cortinaAcampamento = cortina.GetComponent<Image>();
        }
    }

    private bool AcampamentoEmAndamento()
    {
        ControleJornada j = ControleJornada.Instance;
        return j != null && j.ParadaAtiva && j.ParadaAtual == 5 && !acampamentoConcluido;
    }

    private void AtualizarMateriaisAcampamento()
    {
        for (int i = 0; i < materiaisCenario.Length; i++)
            if (materiaisCenario[i] != null)
                materiaisCenario[i].SetActive(!acampamentoConcluido &&
                    (materiaisAcampamento & (1 << i)) == 0);
        if (visualFogueira != null)
            visualFogueira.Mostrar(fogueiraPreparada, refeicaoPreparada, acampamentoConcluido, conversaFogueiraConcluida);
    }

    private void MostrarFogueira(Sprite estado)
    {
        visualFogueira.Mostrar(fogueiraPreparada, refeicaoPreparada, acampamentoConcluido, estado == fogueiraBrasas);
    }

    private void AtualizarPanoramasJornada()
    {
        if (fundosDaJornada == null || fundosDaJornada.Length < 5 || panoramasAcampamento == null) return;
        // Os demais trechos já possuem suas artes repetidas e distribuídas na Hierarchy.
        // Só a última parada acompanha a refeição, a noite e o amanhecer, sem trocar toda a estrada.
        int indice = acampamentoConcluido ? 2 : refeicaoPreparada ? 4 : 3;
        Sprite fundo = fundosDaJornada[indice];
        if (fundo == null) return;
        foreach (SpriteRenderer panorama in panoramasAcampamento)
            if (panorama != null) panorama.sprite = fundo;
    }


    private void AguardarFamiliaAcampamento()
    {
        if (familiaEsperando) return;
        maeSeguia = mae.enabled; paiSeguia = pai.enabled;
        baleiaSeguia = seguidorBaleia != null && seguidorBaleia.enabled;
        mae.enabled = false; pai.enabled = false;
        if (seguidorBaleia != null) seguidorBaleia.enabled = false;
        familiaEsperando = true;
    }

    private void PosicionarPaisAcampamento()
    {
        float x = quintaParada.position.x;
        Vector3 posicao = mae.transform.position; posicao.x = x; Posicionar(mae.transform, posicao);
        posicao = pai.transform.position; posicao.x = x - 1.4f; Posicionar(pai.transform, posicao);
        posicao = baleia.position; posicao.x = x - 2.7f; Posicionar(baleia, posicao);
    }

    private void AtualizarAcampamento()
    {
        if (!acampamentoConfigurado || restaurandoFamilia || !AcampamentoEmAndamento() || fogueiraCenario == null) return;
        if (noiteEmAndamento) return;
        if (conversando || Time.frameCount <= quadro || dialogueUI == null || dialogueUI.activeInHierarchy || InvestigationGuard.Blocked) return;
        if (!retomouAcampamento)
        {
            retomouAcampamento = true;
            AguardarFamiliaAcampamento();
            if (introAcampamentoConcluida) PosicionarPaisAcampamento();
        }
        if (!acampamentoIniciado)
        {
            acampamentoIniciado = true;
            AtualizarMateriaisAcampamento(); SalvarAcampamento();
        }
        if (!introAcampamentoConcluida)
            Falar(new[] { "Pai", "Mãe" }, new[] {
                "Já tamo quase na entrada em Fortaleza, vamo fazer uma pausa, descansar uma última noite.",
                "Zefinha, vá junto de tua irmã procurar uns galhos secos e folhas secas, vamo montar uma fogueira debaixo daquele juazeiro."
            }, () => StartCoroutine(MontarAcampamento()));
        else if (refeicaoPreparada) IniciarNoiteAcampamento();
    }

    private IEnumerator MontarAcampamento()
    {
        sequenciaAbrigo = true; BloquearMovimento(); CapturarGrupoAbrigo();
        AudioManager.PlayMoment("fogueira");
        AudioManager.Play("aveAmbiente", 0.35f);
        PrepararFadeAcampamento(); yield return EscurecerAcampamento(1f);
        Vector3 lugar = mari.transform.position; lugar.x = quintaParada.position.x + 3f; Posicionar(mari.transform, lugar);
        lugar = mariquinha.transform.position; lugar.x = mari.transform.position.x - mariquinha.DistanciaAtras; Posicionar(mariquinha.transform, lugar);
        PosicionarPaisAcampamento();
        introAcampamentoConcluida = true; faseAcampamento = FaseAcampamento.Materiais;
        AtualizarMateriaisAcampamento();
        RestaurarCamera();
        yield return EscurecerAcampamento(0f); RestaurarFadeAcampamento();
        RestaurarGrupoAbrigo(); RestaurarInventarioAbrigo(); LiberarMovimento();
        sequenciaAbrigo = false; quadro = Time.frameCount;
        SalvarAcampamento();
    }

    private bool MaterialAoAlcance(string identificador)
    {
        if (mari == null) return false;
        foreach (GameObject material in materiaisCenario)
        {
            if (material == null || IdentificadorCenario(material) != identificador) continue;
            Collider2D area = material.GetComponent<Collider2D>();
            if (area == null || !area.enabled || !material.activeInHierarchy) return false;
            // A distância é até a área clicável, não até um pivô fora do desenho.
            return Vector2.Distance(mari.transform.position,
                area.ClosestPoint(mari.transform.position)) <= 5f;
        }
        return false;
    }

    private void ColetarMaterialAcampamento(int indiceMaterial)
    {
        if (indiceMaterial < 0 || indiceMaterial > 2 || !AcampamentoEmAndamento() ||
            !introAcampamentoConcluida || noiteEmAndamento || InvestigationGuard.Blocked ||
            (materiaisAcampamento & (1 << indiceMaterial)) != 0 || InventoryManager.Instance == null) return;
        ItemData[] itens = { galhosSecos, folhasSecas, galhosAlecrim };
        CollectPrompt coleta = FindAnyObjectByType<CollectPrompt>();
        if (coleta != null) coleta.Show(itens[indiceMaterial], () =>
        {
            materiaisAcampamento |= 1 << indiceMaterial;
            AtualizarMateriaisAcampamento(); quadro = Time.frameCount; SalvarAcampamento();
        }, alreadyExamined: true);
    }

    private bool AcertouMaeAcampamento(Vector3 ponto)
    {
        if (mae == null) return false;
        SpriteRenderer desenho = mae.GetComponent<SpriteRenderer>();
        if (desenho == null || !desenho.enabled || !desenho.gameObject.activeInHierarchy) return false;
        Bounds area = desenho.bounds; area.Expand(new Vector3(0.25f, 0.25f, 0f));
        return ponto.x >= area.min.x && ponto.x <= area.max.x && ponto.y >= area.min.y && ponto.y <= area.max.y;
    }

    public bool SoltarNoAcampamento(ItemData item, Vector2 posicaoTela)
    {
        if (item == null || !AcampamentoEmAndamento() || noiteEmAndamento || Camera.main == null ||
            (item != galhosSecos && item != folhasSecas && item != materialFogueira && item != galhosAlecrim && item != passaro && item != prea)) return false;
        Ray raio = Camera.main.ScreenPointToRay(posicaoTela); float distancia;
        Plane plano = new Plane(Vector3.forward, Vector3.zero);
        if (!plano.Raycast(raio, out distancia)) return false;
        Vector3 ponto = raio.GetPoint(distancia);
        Collider2D area = fogueiraCenario != null ? fogueiraCenario.GetComponent<Collider2D>() : null;
        if (!AcertouMaeAcampamento(ponto) && (area == null || !area.OverlapPoint(ponto))) return false;
        return InteragirAcampamento(true);
    }

    private bool ConsumirItensAcampamento(params ItemData[] usados)
    {
        InventoryManager inventario = InventoryManager.Instance;
        if (inventario == null) return false;
        var itens = new List<ItemData>(inventario.Items);
        foreach (ItemData item in usados)
            if (item == null || !itens.Remove(item)) return false;
        inventario.ReplaceItems(itens);
        return true;
    }

    private bool InteragirAcampamento(bool peloInventario = false)
    {
        if (!acampamentoConfigurado || !AcampamentoEmAndamento() ||
            (faseAcampamento != FaseAcampamento.Materiais && faseAcampamento != FaseAcampamento.Refeicao) ||
            !introAcampamentoConcluida || noiteEmAndamento ||
            conversando || sequenciaAbrigo || PauseHelper.BlockInput() || Time.timeScale == 0f ||
            Time.frameCount <= quadro || (!peloInventario && InvestigationGuard.Blocked) ||
            mari == null || mae == null || !PertoDoAcampamento()) return false;
        InventoryTabController.Instance?.Close();
        if (!fogueiraPreparada)
        {
            bool combinados = materialFogueira != null && Possui(materialFogueira);
            if (!combinados && (!Possui(galhosSecos) || !Possui(folhasSecas)))
            {
                Falar(new[] { "Mãe" }, new[] {
                    "Zefinha, vá junto de tua irmã procurar uns galhos secos e folhas secas, vamo montar uma fogueira debaixo daquele juazeiro."
                }, null);
                return true;
            }
            if (combinados)
            {
                if (!ConsumirItensAcampamento(materialFogueira)) return false;
            }
            else if (!ConsumirItensAcampamento(galhosSecos, folhasSecas)) return false;
            fogueiraPreparada = true; faseAcampamento = FaseAcampamento.Refeicao;
            AtualizarMateriaisAcampamento(); SalvarAcampamento();
        }
        if (!pedidoAlecrimConcluido || !Possui(galhosAlecrim))
        {
            Falar(new[] { "Mãe" }, new[] {
                "Agora vá atrás de alguns galhos de alecrim, pra gente conseguir colocar os bicho no espeto, vamo assar na fogueira."
            }, () =>
            {
                pedidoAlecrimConcluido = true; SalvarAcampamento();
                if (Possui(galhosAlecrim)) PrepararRefeicaoAcampamento();
            });
        }
        else PrepararRefeicaoAcampamento();
        return true;
    }

    private bool PertoDoAcampamento()
    {
        Vector2 posicao = mari.transform.position;
        if (Vector2.Distance(posicao, mae.transform.position) <= 5f) return true;
        Collider2D area = fogueiraCenario != null ? fogueiraCenario.GetComponent<Collider2D>() : null;
        return area != null && Vector2.Distance(posicao, area.ClosestPoint(posicao)) <= 5f;
    }

    private void PrepararRefeicaoAcampamento()
    {
        if (refeicaoPreparada || !fogueiraPreparada || !pedidoAlecrimConcluido) return;
        if (!ConsumirItensAcampamento(galhosAlecrim, passaro, prea))
        {
            Falar(new[] { "Objetivo" }, new[] { "Para preparar a refeição, reúna os galhos de alecrim, o pássaro e o preá no inventário." }, null);
            return;
        }
        refeicaoPreparada = true; faseAcampamento = FaseAcampamento.Familia;
        AtualizarMateriaisAcampamento(); SalvarAcampamento();
        IniciarNoiteAcampamento();
    }

    private void EnquadrarNoite()
    {
        if (seguirCamera != null) seguirCamera.enabled = false;
        cameraAbrigo.orthographicSize = zoomFogueira;
        cameraAbrigo.transform.position = enquadramentoFogueira.position;
    }

    private void MostrarImagemNoite(Sprite[] quadros, Image alvo = null)
    {
        if (noiteTelaCheia == null || quadros == null || quadros.Length == 0) return;
        imagemNoiteAtual = alvo != null ? alvo : imagemNoite;
        if (imagemNoiteAtual == null) return;
        imagemNoite.gameObject.SetActive(imagemNoite == imagemNoiteAtual);
        imagemIrmasDormindo.gameObject.SetActive(imagemIrmasDormindo == imagemNoiteAtual);
        imagemPaisDormindo.gameObject.SetActive(imagemPaisDormindo == imagemNoiteAtual);
        quadrosNoiteAtuais = quadros;
        tempoImagemNoite = 0f;
        imagemNoiteAtual.sprite = quadros[0];
        imagemNoiteAtual.preserveAspect = true;
        DefinirPersonagensFogueira(false);

        noiteTelaCheia.SetActive(true);
        noiteTelaCheia.transform.SetAsLastSibling();
        dialogueUI.transform.SetAsLastSibling();
        if (cortinaAcampamento.gameObject.activeSelf) cortinaAcampamento.transform.SetAsLastSibling();
    }

    private void MostrarImagemPais()
    {
        MostrarImagemNoite(quadrosPaisDeitados, imagemPaisDormindo);
    }

    private void AtualizarImagemNoite()
    {
        if (noiteTelaCheia == null || !noiteTelaCheia.activeInHierarchy || imagemNoiteAtual == null ||
            quadrosNoiteAtuais == null || quadrosNoiteAtuais.Length == 0 || PauseHelper.BlockInput()) return;
        tempoImagemNoite += Time.deltaTime;
        imagemNoiteAtual.sprite = quadrosNoiteAtuais[Mathf.FloorToInt(tempoImagemNoite / .24f) % quadrosNoiteAtuais.Length];
    }

    private void IniciarNoiteAcampamento()
    {
        if (!acampamentoConfigurado || noiteEmAndamento || !refeicaoPreparada || acampamentoConcluido) return;
        faseAcampamento = RestaurarFaseAcampamento();
        noiteEmAndamento = true; sequenciaAbrigo = true;
        BloquearMovimento(); CapturarGrupoAbrigo();
        if (seguirCamera != null) seguirCamera.enabled = false;

        StartCoroutine(MostrarNoiteAcampamento());
    }

    private IEnumerator MostrarNoiteAcampamento()
    {
        PrepararFadeAcampamento(); yield return EscurecerAcampamento(1f);
        Vector3 lugar = mari.transform.position; lugar.x = quintaParada.position.x + 2f; Posicionar(mari.transform, lugar);
        for (int i = 0; i < desenhosAbrigo.Length; i++)
            if (desenhosAbrigo[i] != null) desenhosAbrigo[i].enabled = false;

        // A refeição já consumiu todos os itens; revela os sentados sob a tela escura.
        DefinirPersonagensFogueira(!conversaFogueiraConcluida);
        MostrarFogueira(fogueiraAssando);

        if (conversaFogueiraConcluida)
        {
            MostrarFogueira(fogueiraBrasas);
            if (conversaIrmasConcluida || conversaPaisConcluida) MostrarImagemPais();
            else MostrarImagemNoite(quadrosIrmasAcordadas);
        }
        EnquadrarNoite();
        yield return EscurecerAcampamento(0f); RestaurarFadeAcampamento();
        if (!conversaFogueiraConcluida)
        {
            yield return new WaitForSeconds(2.5f);
            sequenciaAbrigo = false;
            Falar(new[] { "Mariquinha", "Mãe", "Mariquinha", "Mãe", "Mariquinha", "Mãe", "Zefinha", "Mãe", "Mariquinha", "Pai" }, new[] {
                "Será que dueu ou foi num sopro? igual voinho?",
                "Tá falando de que Severina?",
                "Do homem na estrada mainha.",
                "Num sei minha filha, esqueça isso, guardar tanta desgraça faz mal para o coração.",
                "Num guarda é pio, num quero esquecer as coisa que a gente passou, vou escrever um livro mainha, tu vai vê, Zefinha vai me ensinar a ler e escrever.",
                "E desde quando tu sabe ler e escrever Maria Jozefa?",
                "A sinhazinha da fazenda que painho trabalhava me ensinou às escondidas mainha, sei um pouco.",
                "Hora essa! Livro num enche bucho não Severina, tu devia é aprender a bordar, isso sim pode te ajudar no futuro…",
                "Num quero, quero mesmo ser inteligente igual gente que escreve coisa, num é painho? isso num da dinheiro?",
                "Sabe… Tem hora que eu acho… Que Deus fez o sertanejo diferente. Gente quebra. Gente desiste. Mas nóis… Nóis é bicho. Bicho apanha. Bicho passa fome. Bicho sangra. Mas continua andando. Enquanto eu respirá… Ocês num morre. Bicho trabaia Mariquinha, trabaia para não passar fome, num tem tempo pra escrever não."
            }, () =>
            {
                conversaFogueiraConcluida = true; MostrarFogueira(fogueiraBrasas); SalvarAcampamento();
                IniciarCenaIrmas();
            });
        }
        else
        {
            sequenciaAbrigo = false;
            if (conversaPaisConcluida) IniciarFimNoite();
            else if (conversaIrmasConcluida) IniciarCenaPais();
            else IniciarCenaIrmas();
        }
    }

    private void IniciarCenaIrmas()
    {
        faseAcampamento = FaseAcampamento.Irmas;
        sequenciaAbrigo = true; BloquearMovimento();
         StartCoroutine(MostrarCenaIrmas());
    }

    private IEnumerator MostrarCenaIrmas()
    {
        PrepararFadeAcampamento(); yield return EscurecerAcampamento(1f);
        MostrarFogueira(fogueiraBrasas);

        MostrarImagemNoite(quadrosIrmasAcordadas);
        yield return EscurecerAcampamento(0f); RestaurarFadeAcampamento(); sequenciaAbrigo = false;
        RetomarConversaIrmas();
    }

    private void RetomarConversaIrmas()
    {
        if (primeiraEscolhaNoturna == 0)
            Falar(new[] { "Mariquinha" }, new[] { "Zefinha… Tu tá acordada?" }, () => MostrarEscolhaNoturna(false));
        else ContinuarPrimeiraEscolhaNoturna();
    }

    private void MostrarEscolhaNoturna(bool segunda)
    {
        dialogueUI.transform.SetAsLastSibling();
        LimparEscolhas();
        foreach (Transform filho in choicesContainer.transform)
        {
            Button botao = filho.GetComponent<Button>();
            if (botao != null) botoes.Add(botao);
        }
        string[] opcoes = segunda ? new[] { "Eu prometo.", "Vai dar tudo certo.", "Ficar em silêncio." }
            : new[] { "Ficar em silêncio.", "Responder." };
        if (botoes.Count < opcoes.Length)
        {
            Debug.LogError("SegundaParada: faltam os botoes existentes para as escolhas noturnas.", this);
            LimparEscolhas(); LiberarMovimento(); return;
        }
        escolhaNoturnaAberta = true; segundaEscolhaNoturnaAberta = segunda;
        conversando = true; esperandoEscolha = true; depois = null; BloquearMovimento();
        dialogueUI.SetActive(true); dialoguePanel.SetActive(true); choicesContainer.SetActive(true);
        Grupo(dialogueUI); Grupo(dialoguePanel); Grupo(choicesContainer);
        characterNameText.text = "Escolha";
        dialogueText.text = segunda ? "Promete que nós nunca vai se largá?" : "Zefinha… Tu tá acordada?";
        if (portraitImage != null) portraitImage.enabled = false;
        for (int i = 0; i < botoes.Count; i++)
        {
            Button botao = botoes[i]; botao.gameObject.SetActive(i < opcoes.Length);
            if (i >= opcoes.Length) continue;
            TMP_Text texto = botao.GetComponentInChildren<TMP_Text>(true);
            if (texto != null) texto.text = opcoes[i];
            int opcao = i + 1;
            botao.onClick.RemoveAllListeners(); botao.onClick.AddListener(() => EscolherNoite(opcao));
            botao.interactable = true;
        }
        quadro = Time.frameCount;
    }

    private void EscolherNoite(int opcao)
    {
        if (!escolhaNoturnaAberta || !esperandoEscolha || PauseHelper.BlockInput() || Time.timeScale == 0f ||
            opcao < 1 || opcao > (segundaEscolhaNoturnaAberta ? 3 : 2)) return;
        bool segunda = segundaEscolhaNoturnaAberta;
        escolhaNoturnaAberta = false; esperandoEscolha = false; conversando = false;
        LimparEscolhas(); choicesContainer.SetActive(false); dialogueUI.SetActive(false);
        InvestigationGuard.BlockCurrentClick();
        if (segunda) segundaEscolhaNoturna = opcao;
        else primeiraEscolhaNoturna = opcao;
        SalvarAcampamento();
        if (segunda) ContinuarSegundaEscolhaNoturna();
        else ContinuarPrimeiraEscolhaNoturna();
    }

    private void ContinuarPrimeiraEscolhaNoturna()
    {
        if (primeiraEscolhaNoturna == 1)
        {
            Falar(new[] { "Mariquinha" }, new[] { "Acho que tu já dormiu…" }, () =>
                StartCoroutine(PausarConversaNoite(0.8f, () => Falar(new[] { "Mariquinha", "Mariquinha" }, new[] {
                    "Eu queria tanto que a gente já tivesse chegado…", "Boa noite, irmã."
                }, () => StartCoroutine(EncerrarConversaIrmas())))));
            return;
        }
        if (segundaEscolhaNoturna != 0) { ContinuarSegundaEscolhaNoturna(); return; }
        Falar(new[] { "Zefinha", "Mariquinha", "Zefinha", "Mariquinha" }, new[] {
            "Tô.", "Posso conta um segredo?", "Pode.",
            "Eu num quero sê bicho não… Quero sê gente. Quero brincá. Quero estudá. Quero comê até encher a barriga. Quero vê mainha rindo de novo. Quero ser inteligente igual tu. Os porco pode sê bicho. As galinha. Os boi. Mas nós não. Promete que nós nunca vai se largá?"
        }, () => MostrarEscolhaNoturna(true));
    }

    private IEnumerator PausarConversaNoite(float duracao, Action continuacao)
    {
        sequenciaAbrigo = true; BloquearMovimento();
        yield return new WaitForSeconds(duracao);
        sequenciaAbrigo = false;
        continuacao?.Invoke();
    }

    private void ContinuarSegundaEscolhaNoturna()
    {
        MostrarImagemNoite(segundaEscolhaNoturna == 1 ? quadrosIrmasPromessa :
            segundaEscolhaNoturna == 3 ? quadrosIrmasSilencio : quadrosIrmasAcordadas);
        if (segundaEscolhaNoturna == 1)
            Falar(new[] { "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha" }, new[] {
                "Eu prometo.",
                "E tu promete vai me ensina? Pra eu fica famosa com meu livro, igual as moça de vestido bunito que anda na cidade.",
                "Prometo.", "Até eu aprender igualzinho a tu.", "Igualzinho a mim não… Melhor.",
                "Melhor que tu?", "Muito melhor.", "Promessa?", "Promessa.",
                "Você é a melhor Zefinha, uma irma boa, sabe que te amo num sabe?", "Eu sei."
            }, () => StartCoroutine(EncerrarConversaIrmas()));
        else if (segundaEscolhaNoturna == 2)
            Falar(new[] { "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Zefinha", "Mariquinha", "Mariquinha", "Zefinha" }, new[] {
                "Vai dar tudo certo, Mariquinha.", "Tu acha mesmo?",
                "Acho. A gente vai chegar na cidade, painho vai arrumar trabalho… As coisa vão melhorar.",
                "Eu queria acreditar assim também…", "Então acredita comigo.", "Tá bom… Vou tentar.",
                "Boa noite, Zefinha.", "Boa noite, irmã."
            }, () => StartCoroutine(EncerrarConversaIrmas()));
        else if (segundaEscolhaNoturna == 3)
        {
            Falar(new[] { "Mariquinha" }, new[] { "Tu num precisa prometer se num quiser…" }, () =>
                StartCoroutine(PausarConversaNoite(0.8f, () => Falar(new[] {
                    "Mariquinha", "Zefinha", "Mariquinha", "Mariquinha"
                }, new[] {
                    "Eu só tenho medo de perder vocês.", "…", "Desculpa… Num queria te aperrear.", "Boa noite, Zefinha."
                }, ConcluirSilencioNoturno))));
        }
    }

    private void ConcluirSilencioNoturno()
    {
        Falar(new[] { "Zefinha (em pensamentos)" }, new[] {
            "Eu queria prometer… Mas nem sei o que vai acontecer amanhã."
        }, () => StartCoroutine(EncerrarConversaIrmas()));
    }

    private IEnumerator EncerrarConversaIrmas()
    {
        sequenciaAbrigo = true; BloquearMovimento();
        if (segundaEscolhaNoturna == 3) yield return new WaitForSeconds(1.8f);
        // A imagem de dormir aparece só depois da última fala das irmãs.
        PrepararFadeAcampamento(); yield return EscurecerAcampamento(1f);
        MostrarImagemNoite(quadrosIrmasDormindo, imagemIrmasDormindo);
        yield return EscurecerAcampamento(0f); RestaurarFadeAcampamento();
        yield return new WaitForSeconds(3f);
        conversaIrmasConcluida = true; SalvarAcampamento();
        sequenciaAbrigo = false; IniciarCenaPais();
    }

    private void IniciarCenaPais()
    {
        faseAcampamento = FaseAcampamento.Pais;
        sequenciaAbrigo = true; BloquearMovimento();
        StartCoroutine(MostrarCenaPais());
    }

    private IEnumerator MostrarCenaPais()
    {
        PrepararFadeAcampamento(); yield return EscurecerAcampamento(1f);
        MostrarImagemPais();
        yield return EscurecerAcampamento(0f); RestaurarFadeAcampamento(); sequenciaAbrigo = false;
        ConversarPaisNoite();
    }

    private void ConversarPaisNoite()
    {
        Falar(new[] { "Pai", "Mãe", "Pai", "Mãe" }, new[] {
            "Elas num merece isso.", "Eu sei.", "Queria te deixado alguma coisa.", "Mas tu deixou. Tu ficou."
        }, () =>
        {
            conversaPaisConcluida = true; SalvarAcampamento(); IniciarFimNoite();
        });
    }

    private void IniciarFimNoite()
    {
        faseAcampamento = FaseAcampamento.Amanhecer;
        sequenciaAbrigo = true; BloquearMovimento();
        StartCoroutine(AmanhecerAcampamento());
    }

    private IEnumerator AmanhecerAcampamento()
    {
        AudioManager.StopMoment("fogueira");
        // A fotografia da casa pertence apenas ao abrigo da segunda parada.
        MostrarImagemPais();
        yield return new WaitForSeconds(2.5f);
        PrepararFadeAcampamento(); yield return EscurecerAcampamento(1f);
        yield return new WaitForSeconds(1.4f);
        if (noiteTelaCheia != null) noiteTelaCheia.SetActive(false);
        quadrosNoiteAtuais = null;
        MostrarFogueira(fogueiraBrasas);
        Vector3 lugar = mari.transform.position; lugar.x = quintaParada.position.x + 2.8f; Posicionar(mari.transform, lugar);
        foreach (SeguidorFamilia familiar in seguidoresAbrigo)
        {
            if (familiar == null) continue;
            lugar = familiar.transform.position; lugar.x = mari.transform.position.x - familiar.DistanciaAtras;
            Posicionar(familiar.transform, lugar);
        }
        foreach (SpriteRenderer desenho in desenhosAbrigo) if (desenho != null) desenho.flipX = false;
        RestaurarGrupoAbrigo(); RetomarFamilia(); AplicarFerimento();
        Physics2D.SyncTransforms(); RestaurarCamera();
        yield return EscurecerAcampamento(0f); RestaurarFadeAcampamento();
        ControleJornada jornada = ControleJornada.Instance;
        // Registra o amanhecer mesmo se um checkpoint restaurado perdeu o
        // estado transitorio do gatilho. Nunca deixa a ultima caminhada presa.
        acampamentoConcluido = true; faseAcampamento = FaseAcampamento.Concluido;
        if (jornada != null) jornada.ConcluirAcampamento();
        noiteEmAndamento = false; sequenciaAbrigo = false;
        RestaurarInventarioAbrigo(); LiberarMovimento();
        quadro = Time.frameCount; SalvarAcampamento();
    }

    private void SalvarAcampamento()
    {
        if (GameProgress.Instance == null || GameSaveSystem.IsRestoring) return;
        GameProgressData d = GameProgress.Instance.Data;
        d.estradaAcampamentoIniciado = acampamentoIniciado;
        d.estradaIntroAcampamentoConcluida = introAcampamentoConcluida;
        d.estradaMateriaisAcampamento = materiaisAcampamento;
        d.estradaFogueiraPreparada = fogueiraPreparada;
        d.estradaPedidoAlecrimConcluido = pedidoAlecrimConcluido;
        d.estradaRefeicaoPreparada = refeicaoPreparada;
        d.estradaConversaFogueiraConcluida = conversaFogueiraConcluida;
        d.estradaPrimeiraEscolhaNoturna = primeiraEscolhaNoturna;
        d.estradaSegundaEscolhaNoturna = segundaEscolhaNoturna;
        d.estradaConversaIrmasConcluida = conversaIrmasConcluida;
        d.estradaConversaPaisConcluida = conversaPaisConcluida;
        d.estradaAcampamentoConcluido = acampamentoConcluido;
        SalvarAguaTerceira();
    }

}
