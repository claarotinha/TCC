using UnityEngine;

[CreateAssetMenu(fileName = "NovoItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;

    public Sprite icon;

    [TextArea]
    public string description;
}