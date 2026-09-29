using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-100)]
public class HouseEntrance : MonoBehaviour
{
    [Header("Confirmação")]
    public GameObject confirmPanel;
    public TMP_Text confirmText;
    public Button buttonSim;
    public Button buttonNao;

    [Header("Transição")]
    public GameObject doorText;
    public Image fadeImage;
    public float fadeDuration = 1f;

    private bool playerNearby;
    private bool isTransitioning;
    private Collider2D doorCollider;

    private void Awake()
    {
        doorCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (confirmPanel != null)
            confirmPanel.SetActive(false);
    }

    private void Update()
    {
        if (!playerNearby ||
            isTransitioning ||
            confirmPanel == null ||
            confirmPanel.activeInHierarchy ||
            PauseHelper.BlockInput() ||
            !Input.GetMouseButtonDown(0) ||
            Camera.main == null ||
            doorCollider == null)
            return;

        TutorialManager tutorial = TutorialManager.Instance;

        if (tutorial == null || !tutorial.IsCompleted)
            return;

        if (InvestigationGuard.Blocked)
            return;

        Vector2 mouse =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (doorCollider.OverlapPoint(mouse))
            AbrirConfirmacao();
    }

    private void AbrirConfirmacao()
    {
        if (confirmText == null ||
            buttonSim == null ||
            buttonNao == null)
        {
            Debug.LogError(
                "PortaCasa: ligue o texto e os botões Sim/Não.",
                this
            );
            return;
        }

        confirmText.text = "Deseja entrar em casa?";

        buttonSim.onClick.RemoveAllListeners();
        buttonNao.onClick.RemoveAllListeners();

        buttonSim.onClick.AddListener(ConfirmarEntrada);
        buttonNao.onClick.AddListener(FecharConfirmacao);

        confirmPanel.SetActive(true);
        InvestigationGuard.BlockCurrentClick();
    }

    private void ConfirmarEntrada()
    {
        if (isTransitioning)
            return;

        confirmPanel.SetActive(false);
        InvestigationGuard.BlockCurrentClick();
        StartCoroutine(FadeAndLoadScene());
    }

    private void FecharConfirmacao()
    {
        confirmPanel.SetActive(false);
        InvestigationGuard.BlockCurrentClick();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (doorText != null)
                doorText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (doorText != null)
                doorText.SetActive(false);
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        isTransitioning = true;

        if (doorText != null)
            doorText.SetActive(false);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                color.a = Mathf.Lerp(
                    0f,
                    1f,
                    timer / fadeDuration
                );
                fadeImage.color = color;
            }

            yield return null;
        }

        SceneManager.LoadScene("Casa_Manha");
    }
}
