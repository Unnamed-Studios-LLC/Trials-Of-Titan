using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TooltipType
{
    Item
}

public abstract class Tooltip : MonoBehaviour
{
    public TooltipManager tooltipManager;

    public int id;

    public abstract void LoadObject(Player player, bool owned, object obj);

    public abstract void Hide();

    protected void CloseTooltip()
    {
        tooltipManager.HideTooltip(id);
    }
}
