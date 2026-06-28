using UnityEngine;

[CreateAssetMenu(fileName = "NovaCombinacao", menuName = "Inventory/Combination")]
public class ItemCombination : ScriptableObject
{
    public ItemData itemA;
    public ItemData itemB;
    public ItemData result;
}
