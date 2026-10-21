using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SecaBedInteraction : MonoBehaviour
{
    [Header("Conversa inicial")]
    [SerializeField] private SecaOpeningDialogue openingDialogue;

    [Header("Ação ao escolher dormir")]
    [SerializeField] private UnityEvent onSleepRequested = new UnityEvent();

    private bool sleepRequested;
    private bool showingCursor;

    private bool CanInteract()
    {
        if (sleepRequested ||
            openingDialogue == null ||
            !openingDialogue.Finished ||
            Time.timeScale == 0f)
        {
            return false;
        }

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        return true;
    }

    private void OnMouseOver()
    {
        if (!CanInteract())
        {
            RestoreCursor();
            return;
        }

        if (CursorManager.Instance != null)
        {
            CursorManager.Instance.SetLupa();
            showingCursor = true;
        }
    }

    private void OnMouseExit()
    {
        RestoreCursor();
    }

    private void OnMouseDown()
    {
        if (!CanInteract())
            return;

        sleepRequested = true;
        RestoreCursor();

        Debug.Log(
            "Seca: cama acionada após a conversa inicial.",
            this
        );

        onSleepRequested.Invoke();
    }

    private void RestoreCursor()
    {
        if (!showingCursor)
            return;

        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();

        showingCursor = false;
    }

    private void OnDisable()
    {
        RestoreCursor();
    }
}