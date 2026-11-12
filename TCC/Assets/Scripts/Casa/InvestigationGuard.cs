using UnityEngine;
using UnityEngine.SceneManagement;

public static class InvestigationGuard
{
    private static int lastPanelClickFrame = -1;

    public static bool TryGetPointerPosition(out Vector2 position)
    {
        position = default;
        Camera camera = Camera.main;
        Scene activeScene = SceneManager.GetActiveScene();
        if (camera == null || camera.gameObject.scene != activeScene)
        {
            camera = null;
            foreach (GameObject root in activeScene.GetRootGameObjects())
                foreach (Camera candidate in root.GetComponentsInChildren<Camera>())
                    if (candidate.isActiveAndEnabled && candidate.CompareTag("MainCamera"))
                    { camera = candidate; break; }
        }
        if (camera == null) return false;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        if (!plane.Raycast(ray, out float distance)) return false;
        position = ray.GetPoint(distance);
        return true;
    }

    private static int lastDiagnosticFrame = -1;

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void TraceQuartinhoClick()
    {
        if (SceneManager.GetActiveScene().name != "Quartinho" ||
            !Input.GetMouseButtonDown(0) || lastDiagnosticFrame == Time.frameCount) return;
        lastDiagnosticFrame = Time.frameCount;
        bool projetou = TryGetPointerPosition(out Vector2 point);
        var hits = new System.Collections.Generic.List<Collider2D>();
        var names = new System.Collections.Generic.List<string>();
        if (projetou) Physics2D.OverlapPoint(point, ContactFilter2D.noFilter, hits);
        foreach (Collider2D hit in hits) names.Add(hit.name);
        var uiNames = new System.Collections.Generic.List<string>();
        var events = UnityEngine.EventSystems.EventSystem.current;
        if (events != null)
        {
            var uiHits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            events.RaycastAll(new UnityEngine.EventSystems.PointerEventData(events)
                { position = Input.mousePosition }, uiHits);
            foreach (var hit in uiHits)
                if (hit.gameObject != null && hit.gameObject.GetComponentInParent<UnityEngine.UI.Selectable>() != null)
                    uiNames.Add(hit.gameObject.name);
        }
        Debug.Log("QUARTINHO_DIAG | bloqueio=" + (OpenPanelReason ?? "nenhum") +
            " | pausa=" + UniversalPauseManager.IsPaused + " | restaurando=" + GameSaveSystem.IsRestoring +
            " | bloqueioDoClique=" + (lastPanelClickFrame == Time.frameCount) + " | tempo=" + Time.timeScale +
            " | camera=" + (Camera.main != null ? Camera.main.name + " / " + Camera.main.gameObject.scene.name : "nenhuma") +
            " | projecao=" + projetou + " | ponto=" + point + " | colliders=" + string.Join(", ", names) +
            " | controlesUI=" + string.Join(", ", uiNames) +
            " | cursorAtivo=" + (CursorManager.Instance != null && CursorManager.Instance.isActiveAndEnabled));
    }

    public static void BlockCurrentClick()
    {
        lastPanelClickFrame = Time.frameCount;
    }

    public static bool Blocked =>
        PauseHelper.BlockInput() ||
        lastPanelClickFrame == Time.frameCount ||
        PanelOpen;

    public static bool PanelOpen =>
        OpenPanelReason != null;

    public static string OpenPanelReason
    {
        get
        {
            if (SleepBed.IsSleeping) return "leitura ou sono";
            SecaMealDialogue refeicao = Object.FindAnyObjectByType<SecaMealDialogue>();
            if (refeicao != null && refeicao.MealInProgress) return "refeição da família";
            SegundaParada estrada = Object.FindAnyObjectByType<SegundaParada>();
            if (estrada != null && estrada.BloqueiaCursor) return "diálogo da estrada";
            if (HouseEntrance.IsChangingScene)
                return "entrada em casa";

            if (HouseEntrance.IsPanelOpen)
                return "confirmação de entrar em casa";

            if (SleepBed.IsSleeping)
                return "sequência de dormir";

            if (SleepBed.IsPanelOpen)
                return "confirmação de dormir";

            if (ChestContentsController.IsOpen)
                return "conteúdo do baú";

            if (ChestCodePanelController.IsOpen)
                return "senha do baú";

            if (HiddenPhoto.IsPanelOpen)
                return "fotografia encontrada";

            if (ExamineObject.IsShowing())
                return "investigação";

            if (CollectableExamine.IsShowing())
                return "coleta";

            if (NPCDialogue.IsShowing())
                return "diálogo dos NPCs";

            if (TutorialBoysDialogue.IsShowing)
                return "diálogo dos garotos";

            if (MotherDiaryDialogue.IsShowing)
                return "conversa sobre o diário";

            if (MotherDialogue.IsShowing)
                return "diálogo da mãe";

            if (QuartoBaguncaDoor.IsPanelOpen)
                return "porta do quartinho";

            if (SecaRoomExit.IsPanelOpen)
                return "saída do quarto da Seca";

            if (RadioInteraction.IsPanelOpen)
                return "rádio";

            if (QuartinhoExit.IsPanelOpen)
                return "saída do quartinho";

            InventoryTabController inventory =
                InventoryTabController.Instance;

            if (inventory != null)
            {
                if (inventory.IsOpen)
                    return "inventário";

                CollectPrompt collectPrompt =
                    inventory.GetComponent<CollectPrompt>();

                if (collectPrompt != null && collectPrompt.IsOpen)
                    return "confirmação de coleta";

                CombinationPrompt combinationPrompt =
                    inventory.GetComponent<CombinationPrompt>();

                if (combinationPrompt != null &&
                    combinationPrompt.IsOpen)
                    return "combinação";
            }

            foreach (PhotoCollect photo in
                Object.FindObjectsByType<PhotoCollect>())
            {
                if (photo.IsDialogOpen)
                    return "fotografia";
            }

            foreach (OldPhoto photo in
                Object.FindObjectsByType<OldPhoto>())
            {
                if (photo.IsDialogOpen)
                    return "fotografia antiga";
            }

            foreach (DiaryWithCutscene diary in
                Object.FindObjectsByType<DiaryWithCutscene>())
            {
                if (diary.IsDialogOpen)
                    return "diário";
            }

            return null;
        }
    }
}