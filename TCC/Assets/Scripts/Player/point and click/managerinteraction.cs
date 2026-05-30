using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        DetectInteraction();
    }

    private void DetectInteraction()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector2 mousePosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit =
            Physics2D.Raycast(mousePosition, Vector2.zero);

        if (!hit.collider)
            return;

        IInteractable interactable =
            hit.collider.GetComponent<IInteractable>();

        interactable?.Interact();
    }
}