using UnityEngine;

public static class InvestigationGuard
{
    private static int lastPanelClickFrame = -1;

    // Evita que o clique usado para fechar um painel abra algo no cenário.
    public static void BlockCurrentClick()
    {
        lastPanelClickFrame = Time.frameCount;
    }

    public static bool Blocked
    {
        get
        {
            return PauseHelper.BlockInput() || lastPanelClickFrame == Time.frameCount ||
                   PanelOpen;
        }
    }

    // Distingue um painel ainda aberto do bloqueio do clique que acabou de fechá-lo.
    public static bool PanelOpen
    {
        get
        {
            return OpenPanelReason != null;
        }
    }

    // Indica qual interface está impedindo a investigação quando não há painel visível.
    public static string OpenPanelReason
    {
        get
        {
            if (ExamineObject.IsShowing()) return "investigação";
            if (CollectableExamine.IsShowing()) return "coleta";
            if (NPCDialogue.IsShowing()) return "diálogo dos NPCs";
            if (TutorialBoysDialogue.IsShowing) return "diálogo dos garotos";
            if (MotherDialogue.IsShowing) return "diálogo da mãe";
            if (QuartoBaguncaDoor.IsPanelOpen) return "porta do quartinho";
            if (QuartinhoExit.IsPanelOpen) return "saída do quartinho";

            InventoryTabController inventory = InventoryTabController.Instance;
            if (inventory != null)
            {
                if (inventory.IsOpen) return "inventário";
                if (inventory.GetComponent<CollectPrompt>()?.IsOpen == true)
                    return "confirmação de coleta";
                if (inventory.GetComponent<CombinationPrompt>()?.IsOpen == true)
                    return "combinação";
            }

            foreach (PhotoCollect photo in UnityEngine.Object.FindObjectsByType<PhotoCollect>(
                FindObjectsSortMode.None))
                if (photo.IsDialogOpen) return "fotografia";

            foreach (OldPhoto photo in UnityEngine.Object.FindObjectsByType<OldPhoto>(
                FindObjectsSortMode.None))
                if (photo.IsDialogOpen) return "fotografia antiga";

            foreach (DiaryWithCutscene diary in
                UnityEngine.Object.FindObjectsByType<DiaryWithCutscene>(FindObjectsSortMode.None))
                if (diary.IsDialogOpen) return "diário";

            return null;
        }
    }
}
