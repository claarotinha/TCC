using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventário")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();

    [SerializeField] private List<ItemCombination> combinations = new List<ItemCombination>();

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

    // =========================================================
    // ADICIONAR ITEM
    // =========================================================

    public void AddItem(ItemData item)
    {
        if (item == null)
            return;

        items.Add(item);

        Debug.Log("Item adicionado: " + item.itemName);

        OnInventoryChanged?.Invoke();
    }

    // =========================================================
    // REMOVER ITEM
    // =========================================================

    public void RemoveItem(ItemData item)
    {
        if (item == null)
            return;

        if (items.Contains(item))
        {
            items.Remove(item);

            if (SelectedItem == item)
                SelectedItem = null;

            OnInventoryChanged?.Invoke();
        }
    }

    // =========================================================
    // SELECIONAR ITEM
    // =========================================================

    public void SelectItem(ItemData item)
    {
        if (item == null)
            return;

        SelectedItem = item;

        Debug.Log("Item selecionado: " + item.itemName);

        OnInventoryChanged?.Invoke();
    }

    // =========================================================
    // DESELECIONAR
    // =========================================================

    public void Deselect()
    {
        SelectedItem = null;

        Debug.Log("Item deselecionado.");

        OnInventoryChanged?.Invoke();
    }

    // =========================================================
    // COMBINAÇÃO
    // =========================================================

    public bool TryCombine(ItemData first, ItemData second)
    {
        if (first == null || second == null)
            return false;

        if (first == second || !items.Contains(first) || !items.Contains(second)) return false;
        foreach (ItemCombination combination in combinations)
        {
            if (combination == null || combination.result == null) continue;
            bool matches = (combination.itemA == first && combination.itemB == second) ||
                           (combination.itemA == second && combination.itemB == first);
            if (!matches) continue;
            items.Remove(first);
            items.Remove(second);
            items.Add(combination.result);
            SelectedItem = null;
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }
}