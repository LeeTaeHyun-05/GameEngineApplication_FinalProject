using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Dictionary<ItemType, int> items = new();
    InventoryUI invenUI;
    

    void Start()
    {
        if (invenUI == null)
            invenUI = FindObjectOfType<InventoryUI>();
    }

    public int GetCount(ItemType type)
    {
        items.TryGetValue(type, out var count);
        return count;
    }

    public void Add(ItemType type, int count = 1)
    {
        if (!items.ContainsKey(type)) items[type] = 0;

        items[type] += count;
        Debug.Log($"[Inven] +{count} {type} (รั {items[type]})");

        invenUI.UpdateInventory(this);
    }

    public bool Consume(ItemType type, int count = 1)
    {
        if (!items.TryGetValue(type, out int have) || have < count)
            return false;

        items[type] = have - count;
        Debug.Log($"[Inven] -{count} {type} (รั {items[type]}");
        if (items[type] == 0)
        {
            items.Remove(type); 
            invenUI.selectedIndex = -1;
            invenUI.ResetSelection();
        }
        

        invenUI.UpdateInventory(this);
        return true;
    }
}
