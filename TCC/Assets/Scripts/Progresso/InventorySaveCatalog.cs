using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Save Catalog")]
public class InventorySaveCatalog : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public string id;
        public ItemData item;
    }

    [SerializeField] private Entry[] entries;

    public bool TryGetId(ItemData item, out string id)
    {
        id = null;
        if (item == null || entries == null) return false;
        foreach (Entry entry in entries)
        {
            if (entry != null && entry.item == item && !string.IsNullOrEmpty(entry.id))
            {
                id = entry.id;
                return true;
            }
        }
        return false;
    }

    public bool TryGetItem(string id, out ItemData item)
    {
        item = null;
        if (string.IsNullOrEmpty(id) || entries == null) return false;
        foreach (Entry entry in entries)
        {
            if (entry != null && entry.id == id && entry.item != null)
            {
                item = entry.item;
                return true;
            }
        }
        return false;
    }

    public static bool TryCapture(out List<string> ids, out string error)
    {
        ids = new List<string>();
        error = "";
        InventorySaveCatalog catalog = Resources.Load<InventorySaveCatalog>("InventorySaveCatalog");
        if (catalog == null)
        {
            error = "Catálogo de itens do salvamento não encontrado.";
            return false;
        }
        if (InventoryManager.Instance == null) return true;
        foreach (ItemData item in InventoryManager.Instance.Items)
        {
            if (!catalog.TryGetId(item, out string id))
            {
                error = "Item sem registro no catálogo: " + (item != null ? item.name : "nulo");
                return false;
            }
            ids.Add(id);
        }
        return true;
    }

    public static bool TryResolve(GameSaveFile save, out List<ItemData> items, out string error)
    {
        items = new List<ItemData>();
        error = "";
        if (save == null)
        {
            error = "Arquivo de salvamento ausente.";
            return false;
        }
        // A versão anterior não gravava o inventário. Não reaproveita a sessão atual.
        if (save.version == 1) return true;
        if (save.version != 2 || !save.inventoryCaptured || save.inventoryItemIds == null)
        {
            error = "O arquivo não contém os dados do inventário.";
            return false;
        }
        InventorySaveCatalog catalog = Resources.Load<InventorySaveCatalog>("InventorySaveCatalog");
        if (catalog == null)
        {
            error = "Catálogo de itens do salvamento não encontrado.";
            return false;
        }
        foreach (string id in save.inventoryItemIds)
        {
            if (!catalog.TryGetItem(id, out ItemData item))
            {
                error = "Um item do arquivo salvo não existe no catálogo: " + id;
                return false;
            }
            items.Add(item);
        }
        return true;
    }
}
