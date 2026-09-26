using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryTabController : MonoBehaviour
{
    public static InventoryTabController Instance { get; private set; }

    [SerializeField] private GameObject inventoryPanel;

    public bool IsOpen => inventoryPanel != null && inventoryPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    private void Start()
    {
        // Permite iniciar o teste diretamente por uma cena que não tenha
        // o objeto antigo "inventaru".
        if (InventoryManager.Instance == null)
            new GameObject("InventoryManager").AddComponent<InventoryManager>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (inventoryPanel != null && Input.GetKeyDown(KeyCode.Tab))
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }
}
