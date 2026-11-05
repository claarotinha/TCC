using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Ato 2, etapa 1. As artes, falas e posições são referências editáveis da cena.
public class EntradaCurrais : MonoBehaviour
{
    [Serializable]
    public class Fala
    {
        public string personagem;
        [TextArea(2, 6)] public string texto;
    }

    [Serializable]
    public class Exame
    {
        public Collider2D area;
        public string titulo;
        [TextArea(2, 5)] public string texto;
    }

    [Header("Sistemas existentes (também permitem testar esta cena diretamente)")]
    [SerializeField] private GameObject progressoPrefab;
    [SerializeField] private GameObject inventarioPrefab;
    [SerializeField] private PlayerMovement mari;
    [SerializeField] private Camera cameraCena;
    [SerializeField] private CameraFollow seguirCamera;
    [SerializeField] private Collider2D limiteSetor;

    [Header("Canvas reutilizado da Estrada")]
    [SerializeField] private GameObject dialogoUI, painelDialogo, escolhas;
    [SerializeField] private TMP_Text nomeTexto, falaTexto, objetivoTexto;
    [SerializeField] private Image retrato, fade;
    [SerializeField] private Button conversarBotao, sairBotao;

    [Header("Cenários separados na Hierarchy")]
    [SerializeField] private GameObject rua, campo;
    [SerializeField] private GameObject lacoChao, lacoPanela, outraFila;
    [SerializeField] private Collider2D areaLaco, areaLuzia;
    [SerializeField] private Transform pai, mae, baleia, irma, cabo, carroca;
    [SerializeField] private GameObject luzia;
    [SerializeField] private SeguidorFamilia[] seguidores;
    [SerializeField] private SpriteRenderer visualIrma;
    [SerializeField] private Sprite irmaFerida, irmaCurativo;

    [Header("Marcas de posição editáveis, sem coordenadas no código")]
    [SerializeField] private Transform cameraRua, cameraCampo;
    [SerializeField] private Transform irmaSeparada, irmaSaida, caboBarreira, carrocaSaida;
    [SerializeField] private Transform pontoColeta, mariEntrada, paiEntrada, maeEntrada, baleiaEntrada;
    [SerializeField] private Transform mariAbrigo, paiAbrigo, maeAbrigo, baleiaAbrigo;

    [Header("Tempos das cutscenes")]
    [SerializeField, Min(0.1f)] private float tempoFade = 0.7f;
    [SerializeField, Min(0.1f)] private float tempoPlano = 1.2f;
    [SerializeField, Min(0.1f)] private float tempoSeparacao = 2.5f;
    [SerializeField, Min(0.1f)] private float tempoCarroca = 2.8f;

    [Header("Falas do roteiro - editáveis no Inspector")]
    [SerializeField] private Fala[] antesSeparacao, barreira, depoisSeparacao;
    [SerializeField] private Fala promessa, semPromessa, respostaZefinha;
    [SerializeField] private Fala[] entrada, revista, abrigo;
    [SerializeField] private Exame[] exames;
    [SerializeField] private string objetivoBusca = "Descobrir para onde levaram Mariquinha.";
    [SerializeField] private string objetivoLaco = "Clique no pedaço do laço que caiu no chão.";

    private GameProgressData Dados => GameProgress.Instance.Data;
    private bool pronto, ocupado, dialogando, aguardandoClique, menuAberto, encerrando;
    private int quadroBloqueado;
    private Quaternion rotacaoPai;
    private float ajustePivoIrma;

    private void Awake()
    {
        if (seguirCamera != null) seguirCamera.enabled = false;
        if (mari != null) mari.enabled = false;
        if (seguidores != null)
            foreach (SeguidorFamilia seguidor in seguidores)
                if (seguidor != null) seguidor.enabled = false;
    }

    private IEnumerator Start()
    {
        if (!ReferenciasValidas())
        {
            Debug.LogError("EntradaCurrais: há referências obrigatórias vazias no Inspector.", this);
            LiberarMari();
            yield break;
        }
        if (GameProgress.Instance == null) Instantiate(progressoPrefab);
        if (InventoryManager.Instance == null) Instantiate(inventarioPrefab);
        bool restaurandoPartida = GameSaveSystem.IsRestoring;
        yield return null;
        while (GameSaveSystem.IsRestoring) yield return null;
        if (GameProgress.Instance == null || InventoryManager.Instance == null)
        {
            Debug.LogError("EntradaCurrais: confira os prefabs existentes de progresso e inventário.", this);
            LiberarMari();
            yield break;
        }
        Vector3 posicaoRestaurada = mari.transform.position;
        GameProgress.Instance.NormalizeData();
        rotacaoPai = pai.localRotation;
        if (Dados.estradaMariquinhaFerida && visualIrma != null)
        {
            Sprite pose = Dados.estradaCurativoAplicado ? irmaCurativo : irmaFerida;
            if (pose != null)
            {
                Animator animador = visualIrma.GetComponent<Animator>();
                if (animador != null) animador.enabled = false;
                float pe = visualIrma.bounds.min.y;
                float altura = visualIrma.sprite != null ? visualIrma.sprite.bounds.size.y : pose.bounds.size.y;
                float yAnterior = irma.position.y;
                visualIrma.sprite = pose;
                if (pose.bounds.size.y > 0f)
                    visualIrma.transform.localScale *= altura / pose.bounds.size.y;
                irma.position += Vector3.up * (pe - visualIrma.bounds.min.y);
                ajustePivoIrma = irma.position.y - yAnterior;
            }
        }
        FecharDialogo();
        conversarBotao.onClick.AddListener(RelembrarLuzia);
        sairBotao.onClick.AddListener(FecharMenu);
        pronto = true;
        int etapa = Mathf.Clamp(Dados.curraisEtapaEntrada, 0, 4);
        if (etapa == 0) Iniciar(Separar());
        else if (etapa == 1)
        {
            MostrarRuaDepoisDaSeparacao();
            CorFade(0f);
            objetivoTexto.text = objetivoLaco;
        }
        else if (etapa == 2) Iniciar(Entrar());
        else if (etapa == 3) Iniciar(ChegarAbrigo());
        else
        {
            MostrarAbrigo();
            if (restaurandoPartida)
            {
                Bounds limites = limiteSetor.bounds;
                posicaoRestaurada.x = Mathf.Clamp(posicaoRestaurada.x, limites.min.x + 1f, limites.max.x - 1f);
                mari.transform.position = posicaoRestaurada;
                Rigidbody2D corpo = mari.GetComponent<Rigidbody2D>();
                if (corpo != null) { corpo.position = posicaoRestaurada; corpo.linearVelocity = Vector2.zero; }
                Physics2D.SyncTransforms();
            }
            CorFade(0f);
            objetivoTexto.text = objetivoBusca;
            LiberarMari();
        }
    }

    private bool ReferenciasValidas()
    {
        if (progressoPrefab == null || inventarioPrefab == null || mari == null ||
            cameraCena == null || seguirCamera == null || limiteSetor == null ||
            dialogoUI == null || painelDialogo == null || escolhas == null ||
            nomeTexto == null || falaTexto == null || objetivoTexto == null || fade == null ||
            conversarBotao == null || sairBotao == null || rua == null || campo == null ||
            lacoChao == null || lacoPanela == null || outraFila == null || areaLaco == null ||
            areaLuzia == null || pai == null || mae == null || baleia == null || irma == null ||
            cabo == null || carroca == null || luzia == null || cameraRua == null ||
            cameraCampo == null || irmaSeparada == null || irmaSaida == null ||
            caboBarreira == null || carrocaSaida == null || pontoColeta == null ||
            mariEntrada == null || paiEntrada == null || maeEntrada == null || baleiaEntrada == null ||
            mariAbrigo == null || paiAbrigo == null || maeAbrigo == null || baleiaAbrigo == null)
            return false;
        return FalasValidas(antesSeparacao) && FalasValidas(barreira) &&
            FalasValidas(depoisSeparacao) && FalasValidas(entrada) &&
            FalasValidas(revista) && FalasValidas(abrigo) &&
            FalaValida(promessa) && FalaValida(semPromessa) && FalaValida(respostaZefinha);
    }

    private static bool FalaValida(Fala fala) => fala != null &&
        !string.IsNullOrWhiteSpace(fala.personagem) && !string.IsNullOrWhiteSpace(fala.texto);

    private static bool FalasValidas(Fala[] falas)
    {
        if (falas == null || falas.Length == 0) return false;
        foreach (Fala fala in falas) if (!FalaValida(fala)) return false;
        return true;
    }

    private void Update()
    {
        if (!pronto || PauseHelper.BlockInput() || Time.timeScale == 0f) return;
        if (dialogando)
        {
            if (Time.frameCount > quadroBloqueado && Input.GetMouseButtonDown(0) && !SobreControle())
            {
                aguardandoClique = false;
                quadroBloqueado = Time.frameCount;
            }
            return;
        }
        if (ocupado || menuAberto || Time.frameCount <= quadroBloqueado ||
            !Input.GetMouseButtonDown(0) || SobreControle()) return;
        Ray raio = cameraCena.ScreenPointToRay(Input.mousePosition);
        Plane plano = new Plane(Vector3.forward, mari.transform.position);
        if (!plano.Raycast(raio, out float distancia)) return;
        Vector2 ponto = raio.GetPoint(distancia);
        if (Dados.curraisEtapaEntrada == 1 && areaLaco.OverlapPoint(ponto))
        {
            Iniciar(RecolherLaco());
            return;
        }
        if (Dados.curraisEtapaEntrada != 4) return;
        if (areaLuzia.OverlapPoint(ponto))
        {
            AbrirMenuLuzia();
            return;
        }
        foreach (Exame exame in exames)
        {
            if (exame.area != null && exame.area.enabled && exame.area.OverlapPoint(ponto))
            {
                Iniciar(Examinar(exame));
                return;
            }
        }
    }

    private static bool SobreControle()
    {
        EventSystem sistema = EventSystem.current;
        if (sistema == null) return false;
        PointerEventData ponteiro = new PointerEventData(sistema) { position = Input.mousePosition };
        List<RaycastResult> resultados = new List<RaycastResult>();
        sistema.RaycastAll(ponteiro, resultados);
        foreach (RaycastResult resultado in resultados)
            if (resultado.gameObject != null && resultado.module is GraphicRaycaster &&
                (resultado.gameObject.GetComponentInParent<Selectable>() != null ||
                 resultado.gameObject.GetComponentInParent<InventorySlot>() != null)) return true;
        return false;
    }

    private void Iniciar(IEnumerator sequencia)
    {
        if (ocupado || encerrando) return;
        ocupado = true;
        mari.enabled = false;
        if (InventoryTabController.Instance != null) InventoryTabController.Instance.Close();
        StartCoroutine(Executar(sequencia));
    }

    private IEnumerator Executar(IEnumerator sequencia)
    {
        try { yield return sequencia; }
        finally
        {
            ocupado = false;
            if (!encerrando)
            {
                FecharDialogo();
                if (Dados.curraisEtapaEntrada == 4) LiberarMari();
            }
        }
    }

    private IEnumerator Separar()
    {
        rua.SetActive(true); campo.SetActive(false);
        luzia.SetActive(false); lacoChao.SetActive(false); lacoPanela.SetActive(false);
        cameraCena.transform.position = cameraRua.position;
        CorFade(1f);
        yield return Esperar(tempoPlano);
        yield return Escurecer(0f);
        yield return Dizer(antesSeparacao);
        yield return Mover(irma, irmaSeparada.position + Vector3.up * ajustePivoIrma, tempoSeparacao);
        yield return Mover(cabo, caboBarreira.position, tempoPlano);
        yield return Dizer(barreira);
        pai.localRotation = rotacaoPai * Quaternion.Euler(0f, 0f, -18f);
        yield return Esperar(tempoPlano);
        yield return Dizer(new[] { Dados.estradaSegundaEscolhaNoturna == 1 ? promessa : semPromessa, respostaZefinha });
        yield return PartirOutraFila();
        lacoChao.SetActive(true);
        yield return Dizer(depoisSeparacao);
        Dados.curraisPaiFerido = true;
        Dados.curraisEtapaEntrada = 1;
        objetivoTexto.text = objetivoLaco;
        Salvar();
    }

    private IEnumerator PartirOutraFila()
    {
        Vector3 origemIrma = irma.position, origemCarroca = carroca.position;
        Vector3 origemFila = outraFila.transform.position;
        Vector3 destinoIrma = irmaSaida.position + Vector3.up * ajustePivoIrma;
        Vector3 deslocamento = destinoIrma - origemIrma;
        float tempo = 0f;
        while (tempo < tempoCarroca)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                float t = Mathf.Clamp01(tempo / tempoCarroca);
                irma.position = Vector3.Lerp(origemIrma, destinoIrma, t);
                carroca.position = Vector3.Lerp(origemCarroca, carrocaSaida.position, t);
                outraFila.transform.position = origemFila + deslocamento * t;
            }
            yield return null;
        }
        irma.gameObject.SetActive(false);
        outraFila.SetActive(false);
    }

    private void MostrarRuaDepoisDaSeparacao()
    {
        rua.SetActive(true); campo.SetActive(false); luzia.SetActive(false);
        irma.gameObject.SetActive(false); outraFila.SetActive(false);
        carroca.position = carrocaSaida.position; cabo.position = caboBarreira.position;
        lacoChao.SetActive(true); lacoPanela.SetActive(false);
        pai.localRotation = rotacaoPai * Quaternion.Euler(0f, 0f, -18f);
        cameraCena.transform.position = cameraRua.position;
    }

    private IEnumerator RecolherLaco()
    {
        yield return Mover(mari.transform, pontoColeta.position, tempoPlano);
        lacoChao.SetActive(false);
        Dados.curraisLacoRecolhido = true;
        Dados.curraisEtapaEntrada = 2;
        Salvar();
        yield return Entrar();
    }

    private IEnumerator Entrar()
    {
        yield return Escurecer(1f);
        MostrarEntrada();
        yield return Escurecer(0f);
        yield return Dizer(entrada);
        if (!ReterPertences())
        {
            // Permite tentar novamente sem apagar os itens que não puderam ser registrados.
            yield return Dizer(new[] { new Fala { personagem = "Cena", texto =
                "Não foi possível registrar os pertences. Saia e continue a partida para tentar novamente." } });
            yield break;
        }
        lacoPanela.SetActive(true);
        yield return Dizer(revista);
        Dados.curraisLacoNaPanela = true;
        Dados.curraisEtapaEntrada = 3;
        Salvar();
        yield return ChegarAbrigo();
    }

    private void MostrarEntrada()
    {
        rua.SetActive(false); campo.SetActive(true);
        irma.gameObject.SetActive(false); outraFila.SetActive(false);
        luzia.SetActive(true); lacoChao.SetActive(false);
        lacoPanela.SetActive(Dados.curraisLacoNaPanela);
        Posicionar(mari.transform, mariEntrada); Posicionar(pai, paiEntrada);
        Posicionar(mae, maeEntrada); Posicionar(baleia, baleiaEntrada);
        cameraCena.transform.position = cameraCampo.position;
    }

    private bool ReterPertences()
    {
        if (Dados.curraisPertencesRetidos) return true;
        if (!InventorySaveCatalog.TryCapture(out List<string> ids, out string erro))
        {
            Debug.LogError("EntradaCurrais: pertences preservados no inventário. " + erro, this);
            return false;
        }
        Dados.curraisItensRetidos = new List<string>(ids);
        Dados.curraisPertencesRetidos = true;
        InventoryManager.Instance.ReplaceItems(new ItemData[0]);
        return true;
    }

    private IEnumerator ChegarAbrigo()
    {
        yield return Escurecer(1f);
        MostrarAbrigo();
        yield return Escurecer(0f);
        yield return Dizer(abrigo);
        Dados.curraisEtapaEntrada = 4;
        objetivoTexto.text = objetivoBusca;
        LiberarMari();
        Salvar();
    }

    private void MostrarAbrigo()
    {
        MostrarEntrada();
        Posicionar(mari.transform, mariAbrigo); Posicionar(pai, paiAbrigo);
        Posicionar(mae, maeAbrigo); Posicionar(baleia, baleiaAbrigo);
        lacoPanela.SetActive(Dados.curraisLacoNaPanela);
        pai.localRotation = rotacaoPai * Quaternion.Euler(0f, 0f, -18f);
    }

    private static void Posicionar(Transform alvo, Transform marca)
    {
        alvo.position = marca.position;
        Rigidbody2D corpo = alvo.GetComponent<Rigidbody2D>();
        if (corpo != null) { corpo.position = alvo.position; corpo.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }

    private void LiberarMari()
    {
        if (mari != null) mari.enabled = true;
        if (seguirCamera != null) seguirCamera.enabled = true;
    }

    private IEnumerator Dizer(Fala[] falas)
    {
        dialogando = true;
        escolhas.SetActive(false);
        dialogoUI.SetActive(true); painelDialogo.SetActive(true);
        if (retrato != null) retrato.enabled = false;
        foreach (Fala fala in falas)
        {
            nomeTexto.text = fala.personagem;
            falaTexto.text = fala.texto;
            aguardandoClique = true;
            quadroBloqueado = Time.frameCount;
            while (aguardandoClique) yield return null;
        }
        FecharDialogo();
    }

    private void FecharDialogo()
    {
        dialogando = false; aguardandoClique = false;
        if (escolhas != null) escolhas.SetActive(false);
        if (dialogoUI != null) dialogoUI.SetActive(false);
        quadroBloqueado = Time.frameCount;
    }

    private void AbrirMenuLuzia()
    {
        menuAberto = true; mari.enabled = false;
        dialogoUI.SetActive(true); painelDialogo.SetActive(true);
        nomeTexto.text = "Luzia"; falaTexto.text = objetivoBusca;
        if (retrato != null) retrato.enabled = false;
        escolhas.SetActive(true);
        conversarBotao.gameObject.SetActive(true); sairBotao.gameObject.SetActive(true);
    }

    private void RelembrarLuzia()
    {
        if (!menuAberto || PauseHelper.BlockInput()) return;
        FecharMenu();
        Iniciar(Dizer(abrigo));
    }

    private void FecharMenu()
    {
        if (PauseHelper.BlockInput()) return;
        menuAberto = false; FecharDialogo(); LiberarMari();
    }

    private IEnumerator Examinar(Exame exame)
    {
        yield return Dizer(new[] { new Fala { personagem = exame.titulo, texto = exame.texto } });
    }

    private IEnumerator Mover(Transform alvo, Vector3 destino, float duracao)
    {
        Vector3 origem = alvo.position;
        float tempo = 0f;
        Animator animador = alvo.GetComponent<Animator>();
        if (animador != null && animador.enabled) animador.SetBool("IsWalking", true);
        while (tempo < duracao)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                alvo.position = Vector3.Lerp(origem, destino, Mathf.Clamp01(tempo / duracao));
            }
            yield return null;
        }
        if (animador != null && animador.enabled) animador.SetBool("IsWalking", false);
        Rigidbody2D corpo = alvo.GetComponent<Rigidbody2D>();
        if (corpo != null) { corpo.position = alvo.position; corpo.linearVelocity = Vector2.zero; }
        Physics2D.SyncTransforms();
    }

    private IEnumerator Esperar(float duracao)
    {
        float tempo = 0f;
        while (tempo < duracao)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f) tempo += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Escurecer(float alpha)
    {
        float origem = fade.color.a, tempo = 0f;
        fade.gameObject.SetActive(true);
        fade.raycastTarget = false;
        while (tempo < tempoFade)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                CorFade(Mathf.Lerp(origem, alpha, Mathf.Clamp01(tempo / tempoFade)));
            }
            yield return null;
        }
        CorFade(alpha);
    }

    private void CorFade(float alpha)
    {
        fade.gameObject.SetActive(true);
        fade.color = new Color(0f, 0f, 0f, alpha);
        fade.raycastTarget = false;
    }

    private void Salvar()
    {
        if (GameSaveSystem.IsRestoring || !Dados.CanSave) return;
        GameSaveSystem sistema = GameProgress.Instance.GetComponent<GameSaveSystem>();
        if (sistema != null) sistema.TrySaveCheckpoint(gameObject.scene.name);
    }

    private void OnDisable()
    {
        encerrando = true;
        StopAllCoroutines();
        FecharDialogo();
        if (mari != null) mari.enabled = true;
        if (seguirCamera != null) seguirCamera.enabled = true;
        if (conversarBotao != null) conversarBotao.onClick.RemoveListener(RelembrarLuzia);
        if (sairBotao != null) sairBotao.onClick.RemoveListener(FecharMenu);
    }
}
