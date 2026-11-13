using UnityEngine;

[CreateAssetMenu(fileName = "NovoItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;

    public Sprite icon;
    [Min(0.1f)] public float inventoryIconScale = 1f;

    [TextArea]
    public string description;
}
