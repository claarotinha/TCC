using UnityEngine;

public static class InvestigationGuard
{
    private static int lastPanelClickFrame = -1;

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