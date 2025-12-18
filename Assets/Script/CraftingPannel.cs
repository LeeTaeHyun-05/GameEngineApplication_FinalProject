using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingPannel : MonoBehaviour
{
    public Inventory inventory;
    public List<CraftingRecipe> recipesList;
    public GameObject root;
    public TMP_Text plannedText;
    public Button craftButton;
    public Button clearButton;
    public TMP_Text hintText;

    readonly Dictionary<ItemType, int> planned = new();

    bool isOpen;

    void Start()
    {
        
        SetOpen(false);
        craftButton.onClick.AddListener(DoCraft);
        clearButton.onClick.AddListener(ClearPlanned);
        RefreshPlannedUI();
        Debug.Log($"plannedText type: {plannedText?.GetType().Name}");
        Debug.Log($"hintText type: {hintText?.GetType().Name}");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            SetOpen(!isOpen);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        if (root)
            root.SetActive(open);
        if (!open)
            ClearPlanned();
    }

    public void AddPlanned(ItemType type, int count = 1)
    {
        int have = inventory != null ? inventory.GetCount(type) : 0;
        planned.TryGetValue(type, out int already);

        if (already + count > have)
        {
            SetHint($"{type} not enough.");
            return;
        }

        if (!planned.ContainsKey(type))
            planned[type] = 0;
        planned[type] += count;

        RefreshPlannedUI();
        SetHint($"{type} x{count} Addition Succeed");
    }

    public void ClearPlanned()
    {
        planned.Clear();
        RefreshPlannedUI();
        SetHint("ResetCompleted");
    }

    void RefreshPlannedUI()
    {
        if (!plannedText)
            return;
        if (planned.Count == 0)
        {
            plannedText.text = "Add Ingredient to RightClick";
            return;
        }

        var sb = new StringBuilder();

        foreach (var item in planned)
            sb.AppendLine($"{item.Key} x{item.Value}");
        plannedText.text = sb.ToString();
    }

    void SetHint(string msg)
    {
        if (hintText)
            hintText.text = msg;
    }

    void DoCraft()
    {
        if (planned.Count == 0)
        {
            SetHint("More Ingredient");
            return;
        }

        foreach (var plannedItem in planned)
        {
            if (inventory.GetCount(plannedItem.Key) < plannedItem.Value)
            {
                SetHint($"{plannedItem.Key} is not enough.");
                return;
            }
        }

        var matchedProduct = FindMatch(planned);
        if (matchedProduct == null)
        {
            SetHint("There's no right recipe");
            return;
        }

        foreach (var itemforConsume in planned)
            inventory.Consume(itemforConsume.Key, itemforConsume.Value);

        foreach (var p in matchedProduct.outputs)
            inventory.Add(p.type, p.count);
        ClearPlanned();

        SetHint($"Crafting Completed : {matchedProduct.displayName}");
    }

    CraftingRecipe FindMatch(Dictionary<ItemType, int> planned)
    {
        foreach (var recipe in recipesList)
        {
            bool ok = true;
            foreach (var ing in recipe.inputs)
            {
                if (!planned.TryGetValue(ing.type, out int have) || have != ing.count)
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
                return recipe;
        }
        return null;
    }

}
