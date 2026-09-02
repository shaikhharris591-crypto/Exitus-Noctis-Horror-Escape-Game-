using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public int amount;
    public List<GameObject> instances = new List<GameObject>();
}
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("Inventory")]
    public int range = 3;
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("UI")]
    public Image[] slots;
    public Image[] slotOutlines;
    public TMP_Text[] amountTexts;
    public Sprite[] imageCollection;

    private int selectedIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RefreshUI();
        EnableOutline();
    }

    private void Update()
    {
        Selection();
    }

    void Selection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            selectedIndex = 0;

        if (Input.GetKeyDown(KeyCode.Alpha2) && range > 1)
            selectedIndex = 1;

        if (Input.GetKeyDown(KeyCode.Alpha3) && range > 2)
            selectedIndex = 2;

        EnableOutline();
    }
    public void AddItem(GameObject item)
    {
        foreach (InventoryItem invItem in items)
        {
            if (invItem.itemName == item.name)
            {
                invItem.amount++;
                invItem.instances.Add(item);

                RefreshUI();
                return;
            }
        }

        if (items.Count >= range)
            return;

        InventoryItem newItem = new InventoryItem
        {
            itemName = item.name,
            amount = 1
        };

        newItem.instances.Add(item);

        items.Add(newItem);

        RefreshUI();
        EnableOutline();
    }
    public void RemoveItem(GameObject usedItem)
    {
        InventoryItem item = items.Find(x => x.itemName == usedItem.name);

        if (item == null)
            return;

        item.instances.Remove(usedItem);
        item.amount--;

        if (item.amount <= 0)
        {
            items.Remove(item);

            if (selectedIndex >= items.Count)
                selectedIndex = Mathf.Max(0, items.Count - 1);
        }
      

        RefreshUI();
        EnableOutline();
    }
    void RefreshUI()
    {
        // Clear UI
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].sprite = null;
            slots[i].enabled = false;

            amountTexts[i].text = "";
            amountTexts[i].gameObject.SetActive(false);
        }

        // Fill UI
        for (int i = 0; i < items.Count; i++)
        {
            Sprite sprite = null;

            foreach (Sprite s in imageCollection)
            {
                if (items[i].itemName.Contains(s.name))
                {
                    sprite = s;
                    break;
                }
            }

            if (sprite != null)
            {
                slots[i].enabled = true;
                slots[i].sprite = sprite;
            }

            if (items[i].amount > 1)
            {
                amountTexts[i].gameObject.SetActive(true);
                amountTexts[i].text = items[i].amount.ToString();
            }
        }
    }

    void EnableOutline()
    {
        for (int i = 0; i < slotOutlines.Length; i++)
            slotOutlines[i].gameObject.SetActive(false);

        if (items.Count == 0)
            return;

        if (selectedIndex >= items.Count)
            selectedIndex = items.Count - 1;

        slotOutlines[selectedIndex].gameObject.SetActive(true);
    }

    public InventoryItem GetSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= items.Count)
            return null;

        return items[selectedIndex];
    }

    public bool HasItem(string itemName)
    {
        return items.Exists(x => x.itemName == itemName);
    }

    public int GetItemAmount(string itemName)
    {
        InventoryItem item = items.Find(x => x.itemName == itemName);

        return item == null ? 0 : item.amount;
    }
    public GameObject GetSelectedGameObject()
    {
        InventoryItem item = GetSelectedItem();

        if (item == null)
            return null;

        if (item.instances.Count == 0)
            return null;

        return item.instances[0];
    }
    public void SelectSlot(int index)
    {
        if (index < 0 || index >= items.Count)
            return;

        selectedIndex = index;
        EnableOutline();
    }
}