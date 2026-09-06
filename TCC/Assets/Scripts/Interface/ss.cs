using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogoTesteFinal : MonoBehaviour
{
    [Header("Configuração")]
    public string npcName = "NPC Teste";
    public string[] falas = new string[] { 
        "Olá! Este é um teste de diálogo!",
        "Pressione ESPAÇO para continuar.",
        "Você conseguiu ver esta mensagem?"
    };

    private GameObject canvasObj;
    private GameObject panelObj;
    private TextMeshProUGUI textObj;
    private int currentLine = 0;
    private bool isActive = false;

    void Start()
    {
        Debug.Log("=== DIALOGO TESTE FINAL INICIADO ===");
        CriarUI();
        // Começa desativado
        if (canvasObj != null) canvasObj.SetActive(false);
    }

    void CriarUI()
    {
        // 1. CANVAS (IGUAL AO TESTE QUE FUNCIONOU)
        canvasObj = new GameObject("CanvasDialogoTeste");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 2. PANEL (IGUAL AO TESTE)
        panelObj = new GameObject("PainelDialogo");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.2f, 0.3f);
        panelRect.anchorMax = new Vector2(0.8f, 0.7f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.2f, 0.4f, 0.6f, 1f); // AZUL
        
        // 3. TEXTO (IGUAL AO TESTE)
        GameObject textObjGO = new GameObject("TextoDialogo");
        textObjGO.transform.SetParent(panelObj.transform, false);
        
        RectTransform textRect = textObjGO.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, 0.3f);
        textRect.anchorMax = new Vector2(0.9f, 0.7f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        textObj = textObjGO.AddComponent<TextMeshProUGUI>();
        textObj.text = "Clique no NPC para abrir o diálogo!";
        textObj.fontSize = 28;
        textObj.color = Color.white;
        textObj.alignment = TextAlignmentOptions.Center;
        textObj.fontStyle = FontStyles.Bold;
        
        // 4. BOTÃO FECHAR
        GameObject btnObj = new GameObject("BotaoFechar");
        btnObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.8f, 0.9f);
        btnRect.anchorMax = new Vector2(0.95f, 0.98f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;
        
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        
        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            Debug.Log("❌ Fechando diálogo pelo botão");
            canvasObj.SetActive(false);
            isActive = false;
        });
        
        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "X";
        btnText.fontSize = 24;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        
        canvasObj.SetActive(true);
        canvasObj.SetActive(false);
        
        Debug.Log("✅ UI do teste final criada com sucesso!");
    }

    void OnMouseDown()
    {
        Debug.Log("🖱️ Clique no NPC detectado!");
        
        if (!isActive)
        {
            AbrirDialogo();
        }
    }

    void AbrirDialogo()
    {
        Debug.Log("📖 Abrindo diálogo...");
        
        if (canvasObj != null)
        {
            canvasObj.SetActive(true);
            isActive = true;
            currentLine = 0;
            MostrarLinha();
            Debug.Log("✅ Diálogo ABERTO - canvas ativo: " + canvasObj.activeSelf);
        }
        else
        {
            Debug.LogError("❌ Canvas é NULL!");
        }
    }

    void MostrarLinha()
    {
        if (textObj != null && currentLine < falas.Length)
        {
            textObj.text = falas[currentLine];
            Debug.Log($"💬 Linha {currentLine}: {falas[currentLine]}");
        }
        else if (textObj != null)
        {
            textObj.text = "Fim do diálogo! Pressione ESPAÇO para fechar.";
        }
    }

    void Update()
    {
        if (isActive && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("␣ Espaço pressionado!");
            currentLine++;
            
            if (currentLine < falas.Length)
            {
                MostrarLinha();
            }
            else
            {
                Debug.Log("🔒 Fechando diálogo (fim das falas)");
                if (canvasObj != null) canvasObj.SetActive(false);
                isActive = false;
            }
        }
        
        // Fecha com ESC
        if (isActive && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("🔒 Fechando diálogo (ESC)");
            if (canvasObj != null) canvasObj.SetActive(false);
            isActive = false;
        }
    }
}