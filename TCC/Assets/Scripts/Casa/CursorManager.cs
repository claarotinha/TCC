using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(10000)]
public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    public Texture2D normalCursor;
    public Texture2D lupaCursor;
    [SerializeField] private Sprite lupaSprite;
    private Texture2D preparedLupa;
    private readonly List<Collider2D> hoverHits = new List<Collider2D>();
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    [SerializeField] private GameObject painelDialogoDaCena;
    private SegundaParada jornada;
    private EntradaCurrais currais;

    private void Awake()
    {
        if (Instance != null && Instance != this && Instance.isActiveAndEnabled &&
            Instance.gameObject.scene == gameObject.scene)
        {
            enabled = false;
            return;
        }
        Instance = this;
        foreach (GameObject raiz in gameObject.scene.GetRootGameObjects())
        {
            if (jornada == null) jornada = raiz.GetComponentInChildren<SegundaParada>(true);
            if (currais == null) currais = raiz.GetComponentInChildren<EntradaCurrais>(true);
        }
        PrepareLupa();
        SetNormal();
    }

    private void PrepareLupa()
    {
        if (lupaSprite == null) return;

        Texture2D source = lupaSprite.texture;
        if (!source.isReadable)
        {
            Debug.LogError("CursorManager: habilite Read/Write na imagem lupa.", this);
            return;
        }

        // Usa só a região da lupa, sem as margens transparentes da imagem.
        Rect rect = lupaSprite.rect;
        int sourceWidth = Mathf.RoundToInt(rect.width);
        int sourceHeight = Mathf.RoundToInt(rect.height);
        Color[] sourcePixels = source.GetPixels(
            Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y),
            sourceWidth, sourceHeight);
        const int size = 32;
        float scale = (float)size / Mathf.Max(sourceWidth, sourceHeight);
        int width = Mathf.Max(1, Mathf.RoundToInt(sourceWidth * scale));
        int height = Mathf.Max(1, Mathf.RoundToInt(sourceHeight * scale));
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int sx = Mathf.Min(sourceWidth - 1, x * sourceWidth / width);
            int sy = Mathf.Min(sourceHeight - 1, y * sourceHeight / height);
            pixels[y * size + x] = sourcePixels[sy * sourceWidth + sx];
        }

        preparedLupa = new Texture2D(size, size, TextureFormat.RGBA32, false);
        preparedLupa.SetPixels(pixels);
        preparedLupa.Apply(false, false);
    }

    // A câmera e os scripts dos objetos terminam antes da decisão do cursor.
    private void LateUpdate()
    {
        if (PauseHelper.BlockInput() || Time.timeScale == 0f)
        { SetNormal(); return; }
        if (ChestContentsController.Instance != null &&
            ChestContentsController.Instance.PointerOverItem())
        {
            SetLupa();
            return;
        }

        TutorialManager tutorial = TutorialManager.Instance;

        if (InvestigationGuard.Blocked || SobreControleUI() ||
            (painelDialogoDaCena != null && painelDialogoDaCena.activeInHierarchy) ||
            (currais != null && currais.BloqueiaCursor) ||
            (jornada != null && jornada.BloqueiaCursor) ||
            Camera.main == null ||
            (tutorial != null &&
             tutorial.CurrentStep <= TutorialManager.TutorialStep.Run))
        {
            SetNormal();
            return;
        }

        // A mesma projeção usada pelos controladores de clique, inclusive com câmera inclinada.
        Ray raio = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plano = new Plane(Vector3.forward, Vector3.zero);
        if (!plano.Raycast(raio, out float distancia)) { SetNormal(); return; }
        Vector2 mousePosition = raio.GetPoint(distancia);
        if ((currais != null && currais.PonteiroSobreInteracao(mousePosition)) ||
            (jornada != null && jornada.PonteiroSobreInteracao(mousePosition)))
        { SetLupa(); return; }

        // Inclui triggers explicitamente e não deixa o chão interceptar a lupa.
        hoverHits.Clear();
        Physics2D.OverlapPoint(mousePosition, ContactFilter2D.noFilter, hoverHits);
        var hits = hoverHits;

        foreach (Collider2D hit in hits)
        {
            // A porta liberada substitui a investigação inicial da casa.
            if (TryActive(hit, out HouseEntrance readyEntrance) && readyEntrance.CanInteract)
            {
                SetLupa();
                return;
            }
            TutorialInvestigationTrigger trigger =
                hit.GetComponentInParent<TutorialInvestigationTrigger>();

            if (trigger != null && tutorial != null &&
                (trigger.IsCryingBoy
                    ? tutorial.CurrentStep != TutorialManager.TutorialStep.CryingBoy
                    : tutorial.CurrentStep != TutorialManager.TutorialStep.Investigate))
                continue;

            TutorialBoysDialogue boys = hit.GetComponentInParent<TutorialBoysDialogue>();
            if (boys != null)
            {
                if (tutorial != null &&
                    tutorial.CurrentStep == TutorialManager.TutorialStep.Boys &&
                    boys.isActiveAndEnabled)
                {
                    SetLupa();
                    return;
                }

                continue;
            }

            if (
                (TryActive(hit, out SleepBed bed) &&
                 bed.CanInteract) ||

                (TryActive(hit, out LockedChest chest) &&
                 chest.CanInteract) ||

                (TryActive(hit, out HiddenPhoto hiddenPhoto) &&
                 hiddenPhoto.isActiveAndEnabled) ||

                (TryActive(hit, out ExamineObject examine) &&
                 examine.isActiveAndEnabled) ||

                (TryActive(hit, out QuartinhoExit roomExit) &&
                 roomExit.isActiveAndEnabled) ||

                (TryActive(hit, out CollectableExamine collectible) &&
                 collectible.CanInteract) ||

                (TryActive(hit, out CasaKeychain keychain) &&
                 keychain.CanInteract) ||

                (TryActive(hit, out ItemDiscovery discovery) &&
                 discovery.isActiveAndEnabled) ||

                (TryActive(hit, out HouseEntrance entrance) &&
                 entrance.CanInteract) ||

                (TryActive(hit, out SecaBedInteraction secaBed) && secaBed.PodeInteragir) ||
                (TryActive(hit, out DialogoMariquinha sister) && sister.CanInteract) ||
                (TryActive(hit, out CacaPassarinho bird) && bird.CanInteract) ||
                (TryActive(hit, out CollectableItem simpleItem) && simpleItem.CanInteract) ||
                (TryActive(hit, out ItemUseTarget itemTarget) && itemTarget.CanInteract) ||
                (TryActive(hit, out SimpleInteraction simpleInteraction) && simpleInteraction.CanInteract) ||
                TryActive(hit, out DiaryWithCutscene diary) ||
                InteracaoGenericaAtiva(hit) ||
                TryActive(hit, out QuartoBaguncaDoor roomDoor) ||
                TryActive(hit, out MotherDialogue mother) ||
                TryActive(hit, out MotherDiaryDialogue diaryMother) ||
                TryActive(hit, out NPCDialogue npc) ||
                TryActive(hit, out PhotoCollect photo) ||
                TryActive(hit, out OldPhoto oldPhoto)
            )
            {
                SetLupa();
                return;
            }
        }

        SetNormal();
    }

    private bool SobreControleUI()
    {
        EventSystem eventos = EventSystem.current;
        if (eventos == null) return false;
        uiHits.Clear();
        eventos.RaycastAll(new PointerEventData(eventos) { position = Input.mousePosition }, uiHits);
        foreach (RaycastResult hit in uiHits)
            if (hit.module is GraphicRaycaster && hit.gameObject != null &&
                (hit.gameObject.GetComponentInParent<Selectable>() != null ||
                 hit.gameObject.GetComponentInParent<InventorySlot>() != null)) return true;
        return false;
    }

    private static bool InteracaoGenericaAtiva(Collider2D hit)
    {
        foreach (MonoBehaviour componente in hit.GetComponentsInParent<MonoBehaviour>())
            if (componente != null && componente.isActiveAndEnabled && componente is IInteractable) return true;
        return false;
    }

    private void OnDisable()
    {
        if (Instance == this) SetNormal();
    }

    private static bool TryActive<T>(Collider2D hit, out T component) where T : Behaviour
    {
        component = hit.GetComponentInParent<T>();
        return component != null && component.isActiveAndEnabled;
    }

    public void SetLupa()
    {
        if (Instance != this || !isActiveAndEnabled) return;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(preparedLupa != null ? preparedLupa : lupaCursor,
            Vector2.zero, CursorMode.ForceSoftware);
    }

    public void SetNormal()
    {
        if (Instance != this) return;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        // O cursor normal é o ponteiro do sistema, nunca o PNG inteiro da lupa.
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SetNormal();
            Instance = null;
        }
        if (preparedLupa != null)
            Destroy(preparedLupa);
    }
}
