using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static Material spriteWorld;

    public static Material spriteOutlineWorld;

    public static Material spriteGlowWorld;

    public static Material spriteSimpleWorld;

    public static Material[] spriteUIs;

    public Material spriteWorldMaterial;

    public Material spriteOutlineWorldMaterial;

    public Material spriteGlowWorldMaterial;

    public Material spriteSimpleWorldMaterial;

    public Material[] spriteUIMaterials;

    private void Awake()
    {
        spriteWorld = spriteWorldMaterial;
        spriteOutlineWorld = spriteOutlineWorldMaterial;
        spriteGlowWorld = spriteGlowWorldMaterial;
        spriteSimpleWorld = spriteSimpleWorldMaterial;
        spriteUIs = spriteUIMaterials;
    }

    public static Material GetWorldMaterial(WorldDrawStyle style)
    {
        switch (style)
        {
            case WorldDrawStyle.Glow:
                return spriteGlowWorld;
            case WorldDrawStyle.Outline:
                return spriteOutlineWorld;
            case WorldDrawStyle.None:
                return spriteSimpleWorld;
            default:
                return spriteWorld;
        }
    }

    public static Material GetUIMaterial(int enchantLevel)
    {
        return spriteUIs[Math.Min(spriteUIs.Length - 1, enchantLevel)];
    }
}
