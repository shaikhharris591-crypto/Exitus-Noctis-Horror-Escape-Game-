using UnityEngine;
using UnityEngine.EventSystems;



public class SlotSelector : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.SelectSlot(slotIndex);
    }
}