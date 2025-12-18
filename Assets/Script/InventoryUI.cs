using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Sprite dirtSprite;
    public Sprite grassSprite;
    public Sprite waterSprite;
    public Sprite stoneSprite;
    public Sprite axeSprite;
    public Sprite treeSprite;
    public Sprite leafSprite;
    public Sprite pickAxeSprite;
    public List<Transform> Slot = new List<Transform> ();
    public GameObject SlotItem;
    List<GameObject> items = new List<GameObject> ();

    public int selectedIndex = -1;
    public void UpdateInventory(Inventory myInven)
    {
        foreach(var slotItems in items)
        {
            Destroy(slotItems);
        }
        items.Clear ();

        int idx = 0;
        foreach (var item in myInven.items)
        {
            
            var go = Instantiate(SlotItem, Slot[idx].transform);
            go.transform.localPosition = Vector3.zero;
            SlotItemPrefab sItem = go.GetComponent<SlotItemPrefab>();
            items.Add(go);


            switch (item.Key)
            {
                case ItemType.Dirt:
                    sItem.ItemSetting(dirtSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.Grass:
                    sItem.ItemSetting(grassSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.Water:
                    sItem.ItemSetting(waterSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.Stone:
                    sItem.ItemSetting(stoneSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.Axe:
                    sItem.ItemSetting(axeSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.Tree:
                    sItem.ItemSetting(treeSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.Leaf:
                    sItem.ItemSetting(leafSprite, "x" + item.Value.ToString(), item.Key);
                    break;
                case ItemType.PickAxe:
                    sItem.ItemSetting(pickAxeSprite, "x" + item.Value.ToString(), item.Key);
                    break;
            }

            idx++;
        }
    }

    private void Update()
    {
        for (int i = 0; i < Mathf.Min(9, Slot.Count); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SetSelectedIndex(i);
            }
        }
    }

    public void SetSelectedIndex(int idx)
    {
        ResetSelection();
        if (selectedIndex == idx)
        {
            selectedIndex = -1;
        }
        else
        {
            if (idx >= items.Count)
            {
                selectedIndex = -1;
            }
            else
            {
                SetSelection(idx);
                selectedIndex = idx;
            }
        }
    }

    public void ResetSelection()
    {
        foreach(var slot in Slot)
        {
            slot.GetComponent<Image>().color = Color.white;
        }
    }
    void SetSelection(int _idx)
    {
        Slot[_idx].GetComponent<Image>().color = Color.yellow;
    }

    public ItemType GetInventorySlot()
    {
       return items[selectedIndex].GetComponent<SlotItemPrefab>().blockType;
    }
}
