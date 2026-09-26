using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [SerializeField] private List<ItemData> items = new List<ItemData>();

    public IReadOnlyList<ItemData> Items => items;
    public ItemData SelectedItem { get; private set; }

    public static event Action OnInventoryChanged;

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
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemData item)
    {
        if (item == null || !items.Remove(item)) return;

        if (SelectedItem == item)
            SelectedItem = null;

        OnInventoryChanged?.Invoke();
    }

    public void SelectItem(ItemData item)
    {
        if (item == null) return;

        SelectedItem = item;
        OnInventoryChanged?.Invoke();
    }

    public void Deselect()
    {
        SelectedItem = null;
        OnInventoryChanged?.Invoke();
    }

    public bool TryCombine(ItemData first, ItemData second, ItemCombination recipe)
    {
        if (first == null || second == null || recipe == null ||
            recipe.result == null)
            return false;

        bool matches =
            (recipe.itemA == first && recipe.itemB == second) ||
            (recipe.itemA == second && recipe.itemB == first);

        if (!matches || !items.Contains(first))
            return false;

        // Se os dois slots representam o mesmo ItemData, precisamos
        // de duas unidades desse item no inventário.
        if (first == second)
        {
            int quantity = 0;
            foreach (ItemData current in items)
                if (current == first) quantity++;

            if (quantity < 2) return false;
        }
        else if (!items.Contains(second))
        {
            return false;
        }

        items.Remove(first);
        items.Remove(second);
        items.Add(recipe.result);
        SelectedItem = null;

        OnInventoryChanged?.Invoke();
        return true;
    }
}