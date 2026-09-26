using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CasaKeychain : MonoBehaviour
{
    [SerializeField] private ItemData keyItem;

    private Collider2D keychainCollider;
    private ExamineObject examineObject;
    private bool collected;

    private void Awake()
    {
        keychainCollider = GetComponent<Collider2D>();
        examineObject = GetComponent<ExamineObject>();
    }

    private void Update()
    {
        // Antes da conversa, o ExamineObject que já está no
        // chaveiro continua mostrando sua mensagem original.
        if (!MotherDialogue.FalouSobreTrabalho)
            return;

        // Depois da conversa, trocamos a mensagem antiga
        // pela pergunta de coletar a chave.
        if (examineObject != null && examineObject.enabled)
        {
            examineObject.HidePanel();
            examineObject.enabled = false;
        }

        if (collected || keyItem == null ||
            !Input.GetMouseButtonDown(0) ||
            Camera.main == null ||
            InventoryTabController.Instance == null ||
            InvestigationGuard.Blocked)
            return;

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (!keychainCollider.OverlapPoint(mousePosition))
            return;

        CollectPrompt prompt =
            InventoryTabController.Instance.GetComponent<CollectPrompt>();

        if (prompt == null)
        {
            Debug.LogError("CollectPrompt não encontrado no InventoryCanvas.");
            return;
        }

        prompt.Show(keyItem, () => collected = true);
    }

    private void OnMouseEnter()
    {
        if (!PauseHelper.BlockInput() && CursorManager.Instance != null)
            CursorManager.Instance.SetLupa();
    }

    private void OnMouseExit()
    {
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }
}
