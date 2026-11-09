using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Base do epílogo. As pistas do ato 2 permanecem no GameProgress existente.
public class EpilogoSceneController : MonoBehaviour
{
    [Header("Ambientes editáveis na Hierarchy")]
    [SerializeField] private GameObject[] ambientes;
    [SerializeField] private SpriteRenderer[] fundos;
    [SerializeField] private Camera cameraCena;

    [Header("Interface de diálogo existente")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicesContainer;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Transição")]
    [SerializeField] private Image fadeImage;
    [SerializeField, Min(0.1f)] private float fadeDuration = 0.7f;
    private bool transitionRunning;
    private int ambienteAtual;
    private float ultimoAspecto;

    public int AmbienteAtual => ambienteAtual;
    public bool EmTransicao => transitionRunning;

    private IEnumerator Start()
    {
        if (ambientes == null || fundos == null || ambientes.Length != 3 || fundos.Length != 3 ||
            cameraCena == null || fadeImage == null || dialogueUI == null || dialoguePanel == null ||
            choicesContainer == null || characterNameText == null || dialogueText == null || portraitImage == null)
        {
            Debug.LogError("Epilogo: confira os três ambientes, a câmera e a interface.", this);
            enabled = false;
            yield break;
        }
        for (int i = 0; i < ambientes.Length; i++)
            if (ambientes[i] == null || fundos[i] == null || fundos[i].sprite == null)
            {
                Debug.LogError("Epilogo: um ambiente está sem fundo.", this);
                enabled = false;
                yield break;
            }

        FecharDialogo();
        InventoryTabController.Instance?.Close();
        transitionRunning = true;
        DefinirFade(1f);
        AtivarAmbiente(0);
        // Aguarda o sistema existente terminar de restaurar o checkpoint.
        while (GameSaveSystem.IsRestoring) yield return null;
        yield return FadePara(0f);
        transitionRunning = false;
    }

    public void MostrarAmbiente(int indice)
    {
        if (!isActiveAndEnabled || transitionRunning || indice < 0 || indice >= ambientes.Length) return;
        StartCoroutine(TrocarAmbiente(indice));
    }

    private IEnumerator TrocarAmbiente(int indice)
    {
        transitionRunning = true;
        InventoryTabController.Instance?.Close();
        FecharDialogo();
        yield return FadePara(1f);
        AtivarAmbiente(indice);
        yield return FadePara(0f);
        transitionRunning = false;
    }

    private void AtivarAmbiente(int indice)
    {
        ambienteAtual = indice;
        for (int i = 0; i < ambientes.Length; i++) ambientes[i].SetActive(i == indice);
        EnquadrarAmbiente();
    }

    private void LateUpdate()
    {
        if (cameraCena != null && !Mathf.Approximately(ultimoAspecto, cameraCena.aspect))
            EnquadrarAmbiente();
    }

    private void EnquadrarAmbiente()
    {
        if (fundos == null || ambienteAtual >= fundos.Length || fundos[ambienteAtual] == null) return;
        Bounds bounds = fundos[ambienteAtual].bounds;
        ultimoAspecto = Mathf.Max(.01f, cameraCena.aspect);
        cameraCena.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x / ultimoAspecto);
        cameraCena.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
    }

    private IEnumerator FadePara(float destino)
    {
        float origem = fadeImage.color.a;
        float tempo = 0f;
        while (tempo < fadeDuration)
        {
            if (!PauseHelper.BlockInput() && Time.timeScale > 0f)
            {
                tempo += Time.deltaTime;
                DefinirFade(Mathf.Lerp(origem, destino, Mathf.Clamp01(tempo / fadeDuration)));
            }
            yield return null;
        }
        DefinirFade(destino);
    }

    private void DefinirFade(float alpha)
    {
        fadeImage.color = new Color(0f, 0f, 0f, alpha);
        fadeImage.raycastTarget = false;
    }

    private void FecharDialogo()
    {
        if (dialogueUI != null) dialogueUI.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        transitionRunning = false;
        if (fadeImage != null) DefinirFade(0f);
    }
}
