using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GameSaveFile
{
    public int version = 1;
    public string checkpointScene;
    public GameProgressData progress;
}

public class GameSaveSystem : MonoBehaviour
{
    [Header("Aviso de salvamento")]
    [SerializeField] private GameObject saveNotice;
    [SerializeField] private TMP_Text saveText;

    private GameProgress progress;
    private Coroutine noticeRoutine;

    private bool knownTutorialCompleted;
    private bool knownChestOpened;
    private bool knownDiaryCollected;
    private bool knownMotherConversationCompleted;
    private bool knownNightCompleted;

    private bool waitingForMorningScene;

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
        if (saveNotice != null)
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

        RememberProgress();

        if (!data.tutorialCompleted)
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

        if (chestJustOpened ||
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
        knownDiaryCollected = data.diaryCollected;

        knownMotherConversationCompleted =
            data.motherNightConversationCompleted;

        knownNightCompleted = data.nightCompleted;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsOwner ||
            !waitingForMorningScene ||
            mode != LoadSceneMode.Single)
            return;

        waitingForMorningScene = false;
        SaveCheckpoint(scene.name);
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

        if (!progress.Data.tutorialCompleted)
        {
            Debug.Log(
                "O salvamento começa após concluir o tutorial.",
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

        GameSaveFile save = new GameSaveFile
        {
            checkpointScene = checkpointScene,
            progress = progress.Data
        };

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
            saveText.text = success
                ? "Progresso salvo"
                : "Não foi possível salvar";
        }

        if (saveNotice != null)
            saveNotice.SetActive(true);

        yield return new WaitForSecondsRealtime(
            success ? 2f : 4f
        );

        if (saveNotice != null)
            saveNotice.SetActive(false);

        noticeRoutine = null;
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