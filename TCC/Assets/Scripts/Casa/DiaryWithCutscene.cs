using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DiaryWithCutscene : MonoBehaviour
{
    [Header("Diário")]
    [TextArea(3, 6)]
    public string diaryText = "Um diário empoeirado...";

    [Header("UI - Painel de investigação")]
    public GameObject investigationPanel;
    public TMP_Text investigationText;
    public GameObject collectHint;

    [Header("Cutscene")]
    public string cutsceneSceneName = "Cutscene";

    [Header("Jogador")]
    public string playerTag = "Player";

    private bool dialogueOpen = false;
    private bool canClose = false;
    private bool collected = false;

    public static bool cutsceneWatched = false;
    public static Vector3 savedPlayerPosition;
    public static string savedSceneName;

    private Camera mainCamera;
    private bool mouseOver = false;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("Nenhuma câmera principal encontrada!");
    }

    private void Update()
    {
        if (collected) return;

        // Raycast para detectar mouse sobre o diário
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(ray, out hit);
        bool hittingThis = hitSomething && hit.collider.gameObject == gameObject;

        // Atualiza cursor
        if (hittingThis && !mouseOver)
        {
            mouseOver = true;
            if (CursorManager.Instance != null)
                CursorManager.Instance.SetLupa();
        }
        else if (!hittingThis && mouseOver)
        {
            mouseOver = false;
            if (CursorManager.Instance != null)
                CursorManager.Instance.SetNormal();
        }

        // Primeiro clique: abre diálogo
        if (hittingThis && Input.GetMouseButtonDown(0) && !dialogueOpen && !cutsceneWatched)
        {
            OpenDialogue();
        }

        // Fechar diálogo e iniciar cutscene (clique ou Espaço)
        if (dialogueOpen && canClose && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            CloseDialogueAndStartCutscene();
        }

        // Coletar diário (só após cutscene assistida)
        if (cutsceneWatched && hittingThis && Input.GetKeyDown(KeyCode.E))
        {
            CollectDiary();
        }
    }

    private void OpenDialogue()
    {
        dialogueOpen = true;
        investigationPanel.SetActive(true);
        investigationText.gameObject.SetActive(true);
        investigationText.text = diaryText;
        investigationText.color = Color.white;
        investigationText.fontSize = 32;
        collectHint.SetActive(false);
        StartCoroutine(EnableClose());
    }

    private IEnumerator EnableClose()
    {
        yield return null;
        canClose = true;
    }

    private void CloseDialogueAndStartCutscene()
    {
        dialogueOpen = false;
        canClose = false;
        investigationPanel.SetActive(false);
        collectHint.SetActive(false);

        SaveCurrentState();
        StartCoroutine(PlayCutscene());
    }

    private void SaveCurrentState()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
            savedPlayerPosition = player.transform.position;
        else
            savedPlayerPosition = Vector3.zero;

        savedSceneName = SceneManager.GetActiveScene().name;
    }

    private IEnumerator PlayCutscene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(cutsceneSceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone)
            yield return null;
    }

    public static void ReturnToGame()
    {
        cutsceneWatched = true;
        SceneManager.LoadScene(savedSceneName);
    }

    private void CollectDiary()
    {
        collected = true;
        collectHint.SetActive(false);
        gameObject.SetActive(false);

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();

        Debug.Log("Diário coletado com sucesso!");
    }
}