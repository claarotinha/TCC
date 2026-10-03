using UnityEngine;

public static class InvestigationGuard
{
    private static int lastPanelClickFrame = -1;

    public static void BlockCurrentClick()
    {
        lastPanelClickFrame = Time.frameCount;
    }

    public static bool Blocked
    {
        get
        {
            return PauseHelper.BlockInput() ||
                   lastPanelClickFrame == Time.frameCount ||
                   PanelOpen;
        }
    }

    public static bool PanelOpen
    {
        get
        {
            return OpenPanelReason != null;
        }
    }

    public static string OpenPanelReason
    {
        get
        {
            // Impede investigar o cenário enquanto a senha está aberta.
            if (ChestCodePanelController.IsOpen)
             return "senha do baú";
            // Bloqueia a investigação enquanto uma foto está aberta.
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

            if (MotherDialogue.IsShowing)
                return "diálogo da mãe";

            if (QuartoBaguncaDoor.IsPanelOpen)
                return "porta do quartinho";

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
                Object.FindObjectsByType<PhotoCollect>(
                    FindObjectsSortMode.None))
            {
                if (photo.IsDialogOpen)
                    return "fotografia";
            }

            foreach (OldPhoto photo in
                Object.FindObjectsByType<OldPhoto>(
                    FindObjectsSortMode.None))
            {
                if (photo.IsDialogOpen)
                    return "fotografia antiga";
            }

            foreach (DiaryWithCutscene diary in
                Object.FindObjectsByType<DiaryWithCutscene>(
                    FindObjectsSortMode.None))
            {
                if (diary.IsDialogOpen)
                    return "diário";
            }

            return null;
        }
    }
}