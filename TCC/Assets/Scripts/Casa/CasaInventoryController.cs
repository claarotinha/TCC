using UnityEngine;

public class CasaInventoryController : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    private GameObject panel;
    private void Start()
    {
        InventoryUI ui = FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);
        if (ui == null) { Debug.LogWarning("Inventário da casa não encontrado."); return; }
        panel = ui.transform.parent.gameObject;
        panel.SetActive(false);
        IsOpen = false;
    }
    private void Update()
    {
        if (panel != null && Input.GetKeyDown(KeyCode.Escape))
        {
            IsOpen = !IsOpen;
            panel.SetActive(IsOpen);
        }
    }
    public static void CloseInventory()
    {
        CasaInventoryController controller = FindFirstObjectByType<CasaInventoryController>();
        if (controller == null || controller.panel == null) return;
        IsOpen = false;
        controller.panel.SetActive(false);
    }
    private void OnDisable() { IsOpen = false; }
}
