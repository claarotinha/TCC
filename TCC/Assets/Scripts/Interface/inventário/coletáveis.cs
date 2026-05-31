using UnityEngine;

public class CollectableItem : InteractableObject
{
    [SerializeField]
    private ItemData item;

    public override void Interact()
    {
        Inventory.Instance.AddItem(item);

        Destroy(gameObject);
    }
}