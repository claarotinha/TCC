using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public static Action OnInventoryChanged;

    [SerializeField] private ItemCombination[] combinations;

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
        Debug.Log(item.itemName + " coletado");

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

    public void SelectItem(ItemData item)
    {
        if (item == null) return;

        // primeiro clique
        if (SelectedItem == null)
        {
            SelectedItem = item;
            Debug.Log("Selecionado: " + item.itemName);
            return;
        }

        // mesmo item desmarca
        if (SelectedItem == item)
        {
            SelectedItem = null;
            return;
        }

        TryCombine(SelectedItem, item);
    }

    private void TryCombine(ItemData a, ItemData b)
    {
        foreach (var combo in combinations)
        {
            bool match =
                (combo.itemA == a && combo.itemB == b) ||
                (combo.itemA == b && combo.itemB == a);

            if (match)
            {
                RemoveItem(a);
                RemoveItem(b);

                AddItem(combo.result);

                SelectedItem = null;

                Debug.Log("✔ Combinado!");
                return;
            }
        }

        Debug.Log("✖ Não combina");

        SelectedItem = b;
    }

    public void Deselect()
    {
        SelectedItem = null;
    }
}