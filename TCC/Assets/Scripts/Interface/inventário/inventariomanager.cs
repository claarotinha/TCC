using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public static Action OnInventoryChanged;

    private List<ItemData> items = new();

    public IReadOnlyList<ItemData> Items => items;

    public ItemData SelectedItem { get; private set; }

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
        if (item == null) return;

        items.Add(item);

        Debug.Log(item.itemName + " adicionado ao inventário");

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemData item)
    {
        if (item == null) return;

        items.Remove(item);

        if (SelectedItem == item)
            SelectedItem = null;

        OnInventoryChanged?.Invoke();
    }

    public bool HasItem(ItemData item)
    {
        return items.Contains(item);
    }

    public void SelectItem(ItemData item)
    {
        SelectedItem = item;
    }

    public void DeselectItem()
    {
        SelectedItem = null;
    }
}