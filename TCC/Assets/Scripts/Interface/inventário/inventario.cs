using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    private List<ItemData> items = new();

    public IReadOnlyList<ItemData> Items => items;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(ItemData item)
    {
        items.Add(item);

        Debug.Log("Pegou: " + item.itemName);
    }
}