using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using TitanCore.Net.Packets.Client;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSwapper : MonoBehaviour
{
    public GraphicRaycaster raycaster;

    public World world;

    public ItemDisplay itemDisplay;

    public RectTransform rectTransform;

    private Slot slot;

    public bool DragStarted(Item item, Slot slot)
    {
        if (this.slot != null) return false;
        this.slot = slot;

        rectTransform.sizeDelta = slot.rectTransform.rect.size;
        rectTransform.SetAsLastSibling();

        gameObject.SetActive(true);
        itemDisplay.SetItem(item);

        return true;
    }

    public void OnDrag(Vector2 position, Slot slot)
    {
        if (this.slot != slot) return;
        transform.position = position;
    }

    public void DragEnded(Vector2 position, Slot slot)
    {
        if (this.slot != slot) return;
        this.slot = null;
        gameObject.SetActive(false);
        if (slot.item.IsBlank) return;

        var results = new List<RaycastResult>();
        var pointerEvent = new PointerEventData(EventSystem.current);
        pointerEvent.position = position;
        raycaster.Raycast(pointerEvent, results);

        if (results.Count == 0) return;

        var result = results[0];
        var otherSlot = result.gameObject.GetComponent<Slot>();
        if (otherSlot == null)
        {
            foreach (var r in results)
            {
                if (r.gameObject.tag == "Joysticks") continue;
                if (r.gameObject.tag != "GameView") return;

                for (int i = 0; i < 8; i++)
                {
                    var lootSlot = world.gameManager.ui.lootSlots[i];
                    if (!lootSlot.gameObject.activeSelf) continue;
                    if (!lootSlot.item.IsBlank) continue;
                    slot.Swap(lootSlot);
                    return;
                }
                world.gameManager.client.SendAsync(new TnDrop(slot.owner.GetGameId(), (byte)slot.slotIndex));
            }
            return;
        }

        slot.Swap(otherSlot);
    }
}