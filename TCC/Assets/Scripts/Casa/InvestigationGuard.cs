using System;
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
            if (PauseHelper.BlockInput() || lastPanelClickFrame == Time.frameCount ||
                ExamineObject.IsShowing() || CollectableExamine.IsShowing() ||
                NPCDialogue.IsShowing() || MotherDialogue.IsShowing ||
                (InventoryTabController.Instance != null &&
                 InventoryTabController.Instance.IsOpen))
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

            // Os painéis das cenas e do inventário compartilham esta regra.
            // Só procuramos quando há uma tentativa de interação, não por frame.
            foreach (Canvas canvas in UnityEngine.Object.FindObjectsByType<Canvas>(
                FindObjectsSortMode.None))
            {
                foreach (RectTransform rect in
                    canvas.GetComponentsInChildren<RectTransform>())
                {
                    if (rect == canvas.transform)
                        continue;

                    string panelName = rect.gameObject.name;
                    if (panelName.EndsWith("Panel", StringComparison.OrdinalIgnoreCase) ||
                        panelName.StartsWith("Painel", StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }
    }
}
