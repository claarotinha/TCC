using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField]
    private string interactionMessage;

    public virtual void Interact()
    {
        Debug.Log(interactionMessage);
    }
}