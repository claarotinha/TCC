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
            InventoryTabController inventory = InventoryTabController.Instance;
            if (PauseHelper.BlockInput() || lastPanelClickFrame == Time.frameCount ||
                ExamineObject.IsShowing() || CollectableExamine.IsShowing() ||
                NPCDialogue.IsShowing() || MotherDialogue.IsShowing ||
                QuartoBaguncaDoor.IsPanelOpen || QuartinhoExit.IsPanelOpen ||
                (inventory != null &&
                 (inventory.IsOpen ||
                  inventory.GetComponent<CollectPrompt>()?.IsOpen == true ||
                  inventory.GetComponent<CombinationPrompt>()?.IsOpen == true)))
                return true;

            foreach (PhotoCollect photo in UnityEngine.Object.FindObjectsByType<PhotoCollect>(
                FindObjectsSortMode.None))
                if (photo.IsDialogOpen) return true;

            foreach (OldPhoto photo in UnityEngine.Object.FindObjectsByType<OldPhoto>(
                FindObjectsSortMode.None))
                if (photo.IsDialogOpen) return true;

            foreach (DiaryWithCutscene diary in
                UnityEngine.Object.FindObjectsByType<DiaryWithCutscene>(FindObjectsSortMode.None))
                if (diary.IsDialogOpen) return true;

            return false;
        }
    }
}
