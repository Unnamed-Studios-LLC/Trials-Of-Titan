using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{
    public Tooltip[] tooltips;

    private Dictionary<Type, Tooltip> tooltipTypes;

    private int tooltipId = 0;

    private Tooltip currentTooltip;

    public World world;

    private void Awake()
    {
        tooltipTypes = tooltips.ToDictionary(_ => _.GetType().BaseType.GenericTypeArguments[0]);
    }

    public bool IsCurrentTooltip(int id) => id == tooltipId;

    public int ShowTooltip(object obj, bool owned = false)
    {
        var tooltip = GetTooltip(obj);
        if (tooltip == null) return -1;
        if (currentTooltip != null) HideTooltip(tooltipId);

        tooltip.tooltipManager = this;
        tooltip.id = ++tooltipId;
        tooltip.LoadObject(world?.player ?? null, owned, obj);
        currentTooltip = tooltip;
        return tooltip.id;
    }

    public void HideTooltip(int id)
    {
        if (id != tooltipId) return;
        HideTooltip();
    }

    public void HideTooltip()
    {
        if (currentTooltip == null) return;
        currentTooltip.Hide();
        currentTooltip = null;
    }

    private Tooltip GetTooltip(object obj)
    {
        var type = obj.GetType();
        if (!tooltipTypes.TryGetValue(type, out var tooltip))
        {
            foreach (var pair in tooltipTypes)
            {
                if (pair.Key.IsAssignableFrom(type))
                {
                    tooltip = pair.Value;
                    break;
                }
            }
        }

        if (tooltip == null) return null;

        var copy = Instantiate(tooltip.gameObject);
        copy.transform.SetParent(transform);
        copy.transform.SetAsLastSibling();
        return copy.GetComponent<Tooltip>();
    }
}
