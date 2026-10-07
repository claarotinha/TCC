using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    public Texture2D normalCursor;
    public Texture2D lupaCursor;
    [SerializeField] private Sprite lupaSprite;
    private Texture2D preparedLupa;

    private void Awake()
    {
        Instance = this;
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

    private void Update()
    {
        TutorialManager tutorial = TutorialManager.Instance;

        if (InvestigationGuard.Blocked ||
            Camera.main == null ||
            (tutorial != null &&
             tutorial.CurrentStep <= TutorialManager.TutorialStep.Run))
        {
            SetNormal();
            return;
        }

        Vector2 mousePosition =
            Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Collider2D[] hits =
            Physics2D.OverlapPointAll(mousePosition);

        foreach (Collider2D hit in hits)
        {
            TutorialInvestigationTrigger trigger =
                hit.GetComponent<TutorialInvestigationTrigger>();

            if (trigger != null &&
                trigger.IsCryingBoy &&
                tutorial != null &&
                tutorial.CurrentStep < TutorialManager.TutorialStep.CryingBoy)
                continue;

            if (hit.TryGetComponent(out TutorialBoysDialogue boys))
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
                (hit.TryGetComponent(out SleepBed bed) &&
                 bed.CanInteract) ||

                (hit.TryGetComponent(out LockedChest chest) &&
                 chest.CanInteract) ||

                (hit.TryGetComponent(out HiddenPhoto hiddenPhoto) &&
                 hiddenPhoto.isActiveAndEnabled) ||

                (hit.TryGetComponent(out ExamineObject examine) &&
                 examine.isActiveAndEnabled) ||

                (hit.TryGetComponent(out QuartinhoExit roomExit) &&
                 roomExit.isActiveAndEnabled) ||

                (hit.TryGetComponent(out CollectableExamine collectible) &&
                 collectible.CanInteract) ||

                (hit.TryGetComponent(out CasaKeychain keychain) &&
                 keychain.CanInteract) ||

                (hit.TryGetComponent(out ItemDiscovery discovery) &&
                 discovery.isActiveAndEnabled) ||

                hit.GetComponent<QuartoBaguncaDoor>() != null ||

                hit.GetComponent<MotherDialogue>() != null
            )
            {
                SetLupa();
                return;
            }
        }

        SetNormal();
    }

    public void SetLupa()
    {
        Cursor.SetCursor(preparedLupa != null ? preparedLupa : lupaCursor,
            Vector2.zero, CursorMode.Auto);
    }

    public void SetNormal()
    {
        Cursor.SetCursor(normalCursor, Vector2.zero, CursorMode.Auto);
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
