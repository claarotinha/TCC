using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HiddenPhoto : MonoBehaviour
{
    private static HiddenPhoto current;

    public static bool IsPanelOpen =>
        current != null &&
        current.photoPanel != null &&
        current.photoPanel.activeInHierarchy;

    [Header("Foto escondida neste objeto")]
    [Range(1, 5)]
    [SerializeField] private int photoId = 1;

    [SerializeField] private Sprite photoSprite;

    [TextArea(3, 6)]
    [SerializeField] private string inscription;

    [Header("Painel compartilhado da cena")]
    [SerializeField] private GameObject photoPanel;
    [SerializeField] private Image panelImage;
    [SerializeField] private TMP_Text panelText;
    [SerializeField] private Button readButton;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (GetComponent<Collider2D>() == null)
        {
            Debug.LogError(
                "HiddenPhoto: " + name + " precisa de um Collider2D.",
                this
            );
        }
    }

    private void OnMouseEnter()
    {
        if (InvestigationGuard.Blocked)
            return;

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    private void OnMouseDown()
    {
        if (InvestigationGuard.Blocked)
            return;

        // Não investiga o cenário quando o clique está sobre a interface.
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        OpenPhoto();
    }

    private void OpenPhoto()
    {
        if (photoPanel == null ||
            panelImage == null ||
            panelText == null ||
            readButton == null ||
            closeButton == null ||
            photoSprite == null)
        {
            Debug.LogError(
                "HiddenPhoto: preencha a imagem da foto e " +
                "todos os campos do painel em " + name + ".",
                this
            );
            return;
        }

        if (current != null)
            current.ClosePhoto();

        current = this;

        panelImage.sprite = photoSprite;
        panelImage.preserveAspect = true;
        panelText.text = "";

        readButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);

        readButton.onClick.AddListener(ReadPhoto);
        closeButton.onClick.AddListener(ClosePhoto);

        photoPanel.transform.SetAsLastSibling();
        photoPanel.SetActive(true);

        GameProgress.Instance?.MarkPhotoFound(photoId);

        InvestigationGuard.BlockCurrentClick();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    private void ReadPhoto()
    {
        if (current != this)
            return;

        panelText.text = string.IsNullOrWhiteSpace(inscription)
            ? "Não há nada escrito atrás desta fotografia."
            : inscription;

        GameProgress.Instance?.MarkPhotoRead(photoId);
    }

    private void ClosePhoto()
    {
        if (current != this)
            return;

        if (readButton != null)
            readButton.onClick.RemoveListener(ReadPhoto);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePhoto);

        if (photoPanel != null)
            photoPanel.SetActive(false);

        current = null;

        InvestigationGuard.BlockCurrentClick();

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }

    private void OnDisable()
    {
        if (current == this)
            ClosePhoto();
    }
}