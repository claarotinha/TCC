using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SecaTravelTransition : MonoBehaviour
{
    [Header("Despedida")]
    [SerializeField] private SecaFarewellSequence farewellSequence;

    [Header("Personagem")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Escurecimento")]
    [SerializeField] private Image fadeImage;
    [SerializeField, Min(0.1f)] private float fadeDuration = 0.8f;

    [Header("Destino")]
    [SerializeField] private string destinationScene = "Estrada";

    private bool loading;
    private bool movementLocked;
    private bool movementWasEnabled;

    public void BeginTravel()
    {
        if (loading || farewellSequence == null ||
            !farewellSequence.FarewellCompleted)
        {
            return;
        }

        if (playerMovement == null || fadeImage == null)
        {
            Debug.LogError(
                "SecaTravelTransition: configure Mari e FadeImage.",
                this
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(destinationScene) ||
            !Application.CanStreamedLevelBeLoaded(destinationScene))
        {
            Debug.LogError(
                "SecaTravelTransition: adicione a cena '" +
                destinationScene + "' à lista de cenas da build.",
                this
            );
            return;
        }

        GameProgress progress = GameProgress.Instance;
        if (progress == null || !progress.Data.secaMealCompleted ||
            progress.Data.secaFoodChoice < 1 || progress.Data.secaFoodChoice > 3)
        {
            Debug.LogError(
                "SecaTravelTransition: falta registrar a refeição no GameProgress.",
                this
            );
            return;
        }

        loading = true;
        StartCoroutine(Travel());
    }

    private IEnumerator Travel()
    {
        movementWasEnabled = playerMovement.enabled;
        playerMovement.enabled = false;
        movementLocked = true;

        Rigidbody2D body = playerMovement.GetComponent<Rigidbody2D>();

        if (body != null)
            body.linearVelocity = Vector2.zero;

        Animator animator = playerMovement.GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
        }

        if (InventoryTabController.Instance != null)
            InventoryTabController.Instance.Close();

        fadeImage.gameObject.SetActive(true);
        fadeImage.raycastTarget = true;

        float initialAlpha = fadeImage.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(
                initialAlpha,
                1f,
                Mathf.Clamp01(elapsed / fadeDuration)
            );

            fadeImage.color = new Color(0f, 0f, 0f, alpha);

            yield return null;
        }

        fadeImage.color = Color.black;

        // BeginTravel só é liberado após terminar a despedida.
        // O checkpoint da Estrada ainda verifica os cinco itens antes de salvar.
        GameProgress progress = GameProgress.Instance;
        if (progress == null)
        {
            Debug.LogError("SecaTravelTransition: GameProgress não encontrado.", this);
            RestoreTransition();
            yield break;
        }

        bool departureWasCompleted = progress.Data.secaDepartureCompleted;
        progress.Data.secaDepartureCompleted = true;

        AsyncOperation operation = SceneManager.LoadSceneAsync(
            destinationScene,
            LoadSceneMode.Single
        );

        if (operation == null)
        {
            progress.Data.secaDepartureCompleted = departureWasCompleted;
            Debug.LogError(
                "SecaTravelTransition: não foi possível carregar a estrada.",
                this
            );

            RestoreTransition();
            yield break;
        }

        while (!operation.isDone)
            yield return null;
    }

    private void RestoreTransition()
    {
        if (movementLocked && playerMovement != null)
            playerMovement.enabled = movementWasEnabled;

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
        }

        movementLocked = false;
        loading = false;
    }

    private void OnDisable()
    {
        if (!loading)
            return;

        StopAllCoroutines();
        RestoreTransition();
    }
}
