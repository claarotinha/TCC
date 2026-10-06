using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinueGameController : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject gameProgressPrefab;

    private bool loading;
    private string SavePath => Path.Combine(Application.persistentDataPath, "currais-save.json");

    private void Start()
    {
        if (continueButton == null || gameProgressPrefab == null ||
            gameProgressPrefab.GetComponent<GameProgress>() == null ||
            gameProgressPrefab.GetComponent<GameSaveSystem>() == null)
        {
            Debug.LogError("ContinueGameController: configure o botão e o prefab GameProgress.", this);
            if (continueButton != null) continueButton.interactable = false;
            return;
        }

        continueButton.interactable = TryReadSave(out _, out _);
        continueButton.onClick.AddListener(ContinueGame);
    }

    public void ContinueGame()
    {
        if (loading) return;
        if (!TryReadSave(out GameSaveFile save, out string error))
        {
            Debug.LogError("Não foi possível continuar: " + error, this);
            continueButton.interactable = false;
            return;
        }

        StartCoroutine(LoadCheckpoint(save));
    }

    private bool TryReadSave(out GameSaveFile save, out string error)
    {
        save = null;
        error = "";
        if (!File.Exists(SavePath))
        {
            error = "não existe um arquivo salvo.";
            return false;
        }

        try
        {
            save = JsonUtility.FromJson<GameSaveFile>(File.ReadAllText(SavePath));
            if (save == null || (save.version != 1 && save.version != 2) || save.progress == null ||
                !save.progress.tutorialCompleted)
            {
                error = "o arquivo não contém um checkpoint válido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(save.checkpointScene) ||
                !Application.CanStreamedLevelBeLoaded(save.checkpointScene))
            {
                error = "a cena do checkpoint não está disponível na build.";
                return false;
            }

            if (save.hasPlayerPosition &&
                (!Finite(save.playerPosition.x) || !Finite(save.playerPosition.y) || !Finite(save.playerPosition.z)))
            {
                error = "a posição salva da personagem é inválida.";
                return false;
            }

            return InventorySaveCatalog.TryResolve(save, out _, out error);
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private IEnumerator LoadCheckpoint(GameSaveFile save)
    {
        if (!InventorySaveCatalog.TryResolve(save, out List<ItemData> restoredItems, out string error))
        {
            Debug.LogError("Não foi possível restaurar o inventário: " + error, this);
            yield break;
        }
        loading = true;
        continueButton.interactable = false;
        if (UniversalPauseManager.Instance != null)
            UniversalPauseManager.Instance.ResumeGame();
        Time.timeScale = 1f;
        if (InventoryTabController.Instance != null)
            InventoryTabController.Instance.Close();

        // Substitui a sessão antes de iniciar o salvamento do progresso carregado.
        if (GameProgress.Instance != null)
        {
            Destroy(GameProgress.Instance.gameObject);
            yield return null;
        }

        GameObject progressObject = Instantiate(gameProgressPrefab);
        progressObject.name = "GameProgress";
        GameProgress restoredProgress = progressObject.GetComponent<GameProgress>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(save.progress), restoredProgress.Data);
        restoredProgress.NormalizeData();
        if (save.version == 1)
        {
            // Os saves antigos ainda não registravam a chave e a porta.
            GameProgressData data = restoredProgress.Data;
            if (data.chestOpened || data.diaryCollected || data.foundPhotos.Count > 0)
            {
                data.quartinhoUnlocked = true;
                data.motherWorkConversationCompleted = true;
                data.keyCollected = true;
            }
            Debug.Log("Save antigo: inventário e posição não estavam registrados; usando o início do checkpoint.", this);
        }
        progressObject.GetComponent<GameSaveSystem>().PrepareRestore(save, restoredItems);

        if (GameManager.Instance != null)
            GameManager.Instance.ChangeState(GameState.Gameplay);
        SceneManager.LoadScene(save.checkpointScene);
    }

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueGame);
    }
}

