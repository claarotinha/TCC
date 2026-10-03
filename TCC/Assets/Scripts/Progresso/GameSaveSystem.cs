using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;

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

    private bool tutorialSaveRequested;
    private bool isSaving;

    public string SavePath =>
        Path.Combine(Application.persistentDataPath, "currais-save.json");

    private void Start()
    {
        if (saveNotice != null)
            saveNotice.SetActive(false);
    }

    private void Update()
    {
        GameProgress progress = GameProgress.Instance;

        // Apenas o GameProgress que permaneceu entre as cenas salva.
        if (progress == null || progress.gameObject != gameObject)
            return;

        TutorialManager tutorial = TutorialManager.Instance;

        if (!tutorialSaveRequested &&
            tutorial != null &&
            tutorial.IsCompleted)
        {
            tutorialSaveRequested = true;
            progress.MarkTutorialCompleted();

            // O tutorial já terminou. O ponto de retorno será
            // o início da Casa_Manha.
            SaveCheckpoint("Casa_Manha");
        }
    }

    public void SaveCheckpoint(string checkpointScene)
    {
        if (isSaving || GameProgress.Instance == null)
            return;

        if (string.IsNullOrWhiteSpace(checkpointScene))
        {
            Debug.LogError("Salvamento: informe a cena do checkpoint.");
            return;
        }

        GameSaveFile save = new GameSaveFile
        {
            checkpointScene = checkpointScene,
            progress = GameProgress.Instance.Data
        };

        // Captura o progresso deste checkpoint.
        string json = JsonUtility.ToJson(save, true);

        StartCoroutine(WriteSave(json));
    }

    private IEnumerator WriteSave(string json)
    {
        isSaving = true;

        ShowNotice("Salvando...");

        // Permite que a interface apareça antes da gravação.
        yield return null;

        bool success = false;
        string temporaryPath = SavePath + ".tmp";

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(SavePath))
                File.Replace(temporaryPath, SavePath, null);
            else
                File.Move(temporaryPath, SavePath);

            success = true;

            Debug.Log("Progresso salvo em: " + SavePath);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "Não foi possível salvar o progresso: " +
                exception.Message
            );
        }

        ShowNotice(success ? "Salvo" : "Não foi possível salvar");

        yield return new WaitForSecondsRealtime(success ? 1.5f : 3f);

        if (saveNotice != null)
            saveNotice.SetActive(false);

        isSaving = false;

        if (!success)
        {
            // Permite tentar novamente após a falha.
            tutorialSaveRequested = false;
        }
    }

    private void ShowNotice(string message)
    {
        if (saveText != null)
            saveText.text = message;

        if (saveNotice != null)
            saveNotice.SetActive(true);
    }
}