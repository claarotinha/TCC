using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[Serializable]
public class GameSaveFile
{
    public int version = 2;
    public string checkpointScene;
    public GameProgressData progress;
    public List<string> inventoryItemIds = new List<string>();
    public bool inventoryCaptured;
    public bool hasPlayerPosition;
    public Vector3 playerPosition;
}

public class GameSaveSystem : MonoBehaviour
{
    [Header("Aviso de salvamento")]
    [SerializeField] private GameObject saveNotice;
    [SerializeField] private TMP_Text saveText;
    [SerializeField] private Image saveIcon;
    [SerializeField, Min(0f)] private float saveIconDegreesPerSecond = 90f;
    [SerializeField, Min(0.5f)] private float saveNoticeDuration = 3f;

    private GameProgress progress;
    private Coroutine noticeRoutine;

    private bool knownTutorialCompleted;
    private bool knownChestOpened;
    private bool knownMotherWorkCompleted;
    private bool knownKeyCollected;
    private bool knownQuartinhoUnlocked;
    private bool knownDiaryCollected;
    private bool knownMotherConversationCompleted;
    private bool knownNightCompleted;

    private bool waitingForMorningScene;
    private string pendingCheckpointScene;
    private GameSaveFile pendingRestore;
    private List<ItemData> pendingInventory;

    public static bool IsRestoring
    {
        get
        {
            if (GameProgress.Instance == null) return false;
            GameSaveSystem system = GameProgress.Instance.GetComponent<GameSaveSystem>();
            return system != null && system.pendingRestore != null;
        }
    }

    private void Awake()
    {
        progress = GetComponent<GameProgress>();
    }

    // Chamado antes de LoadScene, depois de validar todos os itens do arquivo.
    public void PrepareRestore(GameSaveFile save, List<ItemData> restoredItems)
    {
        pendingRestore = save;
        pendingInventory = restoredItems;
    }

    public string SavePath =>
        Path.Combine(
            Application.persistentDataPath,
            "currais-save.json"
        );

    private bool IsOwner =>
        progress != null &&
        GameProgress.Instance == progress &&
        progress.gameObject == gameObject;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (saveNotice != null && noticeRoutine == null)
            saveNotice.SetActive(false);

        progress = GetComponent<GameProgress>();

        if (!IsOwner)
            return;

        RememberProgress();
        progress.ProgressChanged += OnProgressChanged;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        // Continua girando mesmo quando o checkpoint ocorre durante uma pausa.
        if (saveIcon != null && saveIcon.gameObject.activeInHierarchy)
            saveIcon.rectTransform.Rotate(0f, 0f, -saveIconDegreesPerSecond * Time.unscaledDeltaTime);

        if (saveNotice != null && saveNotice.activeInHierarchy)
            PosicionarAviso();

        TutorialManager tutorial = TutorialManager.Instance;

        if (!progress.Data.tutorialCompleted &&
            tutorial != null &&
            tutorial.IsCompleted)
        {
            // A mudança gera o primeiro checkpoint.
            progress.MarkTutorialCompleted();
        }
    }

    private void OnProgressChanged()
    {
        if (!IsOwner)
            return;

        GameProgressData data = progress.Data;

        bool tutorialJustCompleted =
            data.tutorialCompleted && !knownTutorialCompleted;

        bool chestJustOpened =
            data.chestOpened && !knownChestOpened;

        bool diaryJustCollected =
            data.diaryCollected && !knownDiaryCollected;

        bool conversationJustCompleted =
            data.motherNightConversationCompleted &&
            !knownMotherConversationCompleted;

        bool nightJustCompleted =
            data.nightCompleted && !knownNightCompleted;

        bool houseCheckpoint =
            (data.motherWorkConversationCompleted && !knownMotherWorkCompleted) ||
            (data.keyCollected && !knownKeyCollected) ||
            (data.quartinhoUnlocked && !knownQuartinhoUnlocked);

        RememberProgress();

        if (!data.CanSave)
            return;

        if (nightJustCompleted)
        {
            // SleepBed registra o fim da noite antes de trocar de cena.
            // Esperamos a próxima cena carregar para salvar o destino certo.
            waitingForMorningScene = true;
            return;
        }

        if (tutorialJustCompleted)
        {
            SaveCheckpoint("Casa_Manha");
            return;
        }

        if (houseCheckpoint || chestJustOpened ||
            diaryJustCollected ||
            conversationJustCompleted)
        {
            SaveCheckpoint(
                SceneManager.GetActiveScene().name
            );
        }
    }

    private void RememberProgress()
    {
        if (progress == null)
            return;

        GameProgressData data = progress.Data;

        knownTutorialCompleted = data.tutorialCompleted;
        knownChestOpened = data.chestOpened;
        knownMotherWorkCompleted = data.motherWorkConversationCompleted;
        knownKeyCollected = data.keyCollected;
        knownQuartinhoUnlocked = data.quartinhoUnlocked;
        knownDiaryCollected = data.diaryCollected;

        knownMotherConversationCompleted =
            data.motherNightConversationCompleted;

        knownNightCompleted = data.nightCompleted;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsOwner || mode != LoadSceneMode.Single) return;

        if (pendingRestore != null && scene.name == pendingRestore.checkpointScene)
        {
            StartCoroutine(RestoreAfterSceneStart(scene));
            return;
        }

        if (waitingForMorningScene || pendingCheckpointScene == scene.name)
        {
            waitingForMorningScene = false;
            pendingCheckpointScene = null;
            StartCoroutine(SaveAfterSceneStart(scene));
        }
    }

    private IEnumerator SaveAfterSceneStart(Scene scene)
    {
        // Aguarda a inicialização da personagem na cena de destino.
        yield return null;
        if (IsOwner && SceneManager.GetActiveScene() == scene)
            SaveCheckpoint(scene.name);
    }

    private IEnumerator RestoreAfterSceneStart(Scene scene)
    {
        yield return null;
        if (!IsOwner || SceneManager.GetActiveScene() != scene) yield break;

        GameSaveFile save = pendingRestore;
        if (InventoryManager.Instance == null)
            new GameObject("InventoryManager").AddComponent<InventoryManager>();
        InventoryManager.Instance.ReplaceItems(pendingInventory);
        pendingInventory = null;

        if (scene.name == "Estrada" && save.progress != null && save.progress.estradaVersaoPercurso < 2)
        {
            if (save.hasPlayerPosition && ControleJornada.Instance != null)
                save.playerPosition = ControleJornada.Instance.MigrarPosicaoSalva(save.playerPosition, save.progress);
            save.progress.estradaVersaoPercurso = 2;
        }

        if (scene.name == "Currais" && save.progress != null && save.progress.curraisVersaoCenario < 1)
        {
            if (save.hasPlayerPosition)
                save.playerPosition = EntradaCurrais.AtualizarPosicaoCenario(save.playerPosition, save.progress);
            save.progress.curraisVersaoCenario = 1;
        }

        if (scene.name == "Currais" && save.progress != null && save.progress.curraisEtapaEntrada == 2)
            save.playerPosition.x = 33.45f;

        if (save.hasPlayerPosition)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.scene == scene)
            {
                player.transform.position = save.playerPosition;
                Rigidbody2D body = player.GetComponent<Rigidbody2D>();
                if (body != null)
                {
                    body.position = new Vector2(save.playerPosition.x, save.playerPosition.y);
                    body.linearVelocity = Vector2.zero;
                    body.angularVelocity = 0f;
                }
                Physics2D.SyncTransforms();
            }
        }

        pendingRestore = null;
        InvestigationGuard.BlockCurrentClick();
        Debug.Log("Checkpoint carregado: " + scene.name, this);
    }

    public void SaveCheckpoint(string checkpointScene)
    {
        TrySaveCheckpoint(checkpointScene);
    }

    public bool TrySaveCheckpoint(string checkpointScene)
    {
        if (!IsOwner)
        {
            Debug.LogError(
                "GameSaveSystem deve estar no mesmo objeto " +
                "do GameProgress ativo.",
                this
            );
            return false;
        }

        if (!progress.Data.CanSave)
        {
            Debug.Log(
                "O salvamento começa após o tutorial ou a conclusão da Seca.",
                this
            );
            return false;
        }

        if (string.IsNullOrWhiteSpace(checkpointScene) ||
            !Application.CanStreamedLevelBeLoaded(checkpointScene))
        {
            Debug.LogError(
                "Checkpoint inválido: confira o nome da cena " +
                "e a lista de cenas da build.",
                this
            );
            return false;
        }

        if (!InventorySaveCatalog.TryCapture(out List<string> inventoryIds, out string inventoryError))
        {
            Debug.LogError(inventoryError, this);
            ShowResult(false);
            return false;
        }

        if (SceneManager.GetActiveScene().name == "Estrada") progress.Data.estradaVersaoPercurso = 2;
        if (SceneManager.GetActiveScene().name == "Currais") progress.Data.curraisVersaoCenario = 1;
        progress.SyncDiaryPages();
        GameSaveFile save = new GameSaveFile
        {
            checkpointScene = checkpointScene,
            progress = progress.Data,
            inventoryItemIds = inventoryIds,
            inventoryCaptured = true
        };

        Scene activeScene = SceneManager.GetActiveScene();
        // Quando o checkpoint aponta para outra cena, usa o ponto inicial dela.
        if (activeScene.name == checkpointScene)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && player.scene == activeScene)
            {
                save.hasPlayerPosition = true;
                save.playerPosition = player.transform.position;
            }
        }

        string temporaryPath = SavePath + ".tmp";

        try
        {
            string json = JsonUtility.ToJson(save, true);

            Directory.CreateDirectory(
                Application.persistentDataPath
            );

            File.WriteAllText(temporaryPath, json);

            if (File.Exists(SavePath))
            {
                File.Replace(
                    temporaryPath,
                    SavePath,
                    SavePath + ".bak"
                );
            }
            else
            {
                File.Move(temporaryPath, SavePath);
            }

            Debug.Log(
                "Checkpoint salvo: " + checkpointScene +
                "\nArquivo: " + SavePath,
                this
            );

            pendingCheckpointScene = activeScene.name != checkpointScene ? checkpointScene : null;
            ShowResult(true);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Não foi possível salvar: " + exception.Message,
                this
            );

            ShowResult(false);
            return false;
        }
    }

    private void ShowResult(bool success)
    {
        if (noticeRoutine != null)
            StopCoroutine(noticeRoutine);

        noticeRoutine = StartCoroutine(DisplayNotice(success));
    }

    private IEnumerator DisplayNotice(bool success)
    {
        if (saveText != null)
        {
            saveText.enabled = !success;
            saveText.text = success ? string.Empty : "Não foi possível salvar";
        }

        if (saveIcon != null)
        {
            saveIcon.rectTransform.localRotation = Quaternion.identity;
            saveIcon.color = Color.white;
            saveIcon.gameObject.SetActive(success);
        }

        if (saveNotice != null)
        {
            Canvas canvas = saveNotice.GetComponentInParent<Canvas>(true);
            if (canvas != null)
            {
                // Corrige overrides antigos que deixavam o Canvas com escala zero.
                canvas.transform.localScale = Vector3.one;
                canvas.gameObject.SetActive(true);
                canvas.enabled = true;
            }
            saveNotice.transform.SetAsLastSibling();
            saveNotice.SetActive(true);
            Canvas.ForceUpdateCanvases();
            PosicionarAviso();
        }

        yield return new WaitForSecondsRealtime(
            success ? Mathf.Max(0.5f, saveNoticeDuration) : 4f
        );

        if (saveNotice != null)
            saveNotice.SetActive(false);

        noticeRoutine = null;
    }

    private void PosicionarAviso()
    {
        RectTransform aviso = saveNotice.transform as RectTransform;
        RectTransform area = aviso != null ? aviso.parent as RectTransform : null;
        if (area == null) return;
        Canvas canvas = saveNotice.GetComponentInParent<Canvas>();
        Camera cameraUI = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        Rect segura = Screen.safeArea;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area,
            segura.max, cameraUI, out Vector2 canto)) return;
        aviso.anchorMin = aviso.anchorMax = Vector2.one;
        aviso.pivot = Vector2.one;
        aviso.localScale = Vector3.one;
        aviso.anchoredPosition = canto - area.rect.max - new Vector2(48f, 40f);
    }

    [ContextMenu("Testar salvamento da cena atual")]
    private void TestCurrentSceneSave()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("Entre no Play Mode para testar.", this);
            return;
        }

        SaveCheckpoint(
            SceneManager.GetActiveScene().name
        );
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (noticeRoutine != null)
        {
            StopCoroutine(noticeRoutine);
            noticeRoutine = null;
        }

        if (saveNotice != null)
            saveNotice.SetActive(false);
    }

    private void OnDestroy()
    {
        if (progress != null)
            progress.ProgressChanged -= OnProgressChanged;
    }
}

