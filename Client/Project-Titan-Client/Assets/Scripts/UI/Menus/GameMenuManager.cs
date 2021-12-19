using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum MenuType
{
    LevelUp,
    Vault,
    Ascension,
    Wardrobe
}

public abstract class GameMenu : MonoBehaviour
{
    public abstract MenuType MenuType { get; }

    public World world;

    public virtual void Setup(World world)
    {
        this.world = world;
    }

    public virtual void Close()
    {
        world.menuManager.CloseMenu(this);
    }
}

public class GameMenuManager : MonoBehaviour
{
    public World world;

    public GameObject[] menus;

    private Dictionary<MenuType, GameObject> prefabs;

    private Dictionary<MenuType, GameMenu> openMenus = new Dictionary<MenuType, GameMenu>();

    private void Awake()
    {
        prefabs = menus.ToDictionary(_ => _.GetComponent<GameMenu>().MenuType);
    }

    public void ToggleMenu(MenuType type)
    {
        if (openMenus.TryGetValue(type, out var menu))
            CloseMenu(menu);
        else
            ShowMenu(type);
    }

    public GameMenu ShowMenu(MenuType type)
    {
        if (openMenus.TryGetValue(type, out var menu))
            return menu;

        var menuObject = Instantiate(prefabs[type]);
        var rect = menuObject.GetComponent<RectTransform>();
        rect.SetParent(transform);

#if UNITY_IOS || UNITY_ANDROID

#else
        rect.anchoredPosition = new Vector2(Screen.width * 0.78f, 0);
#endif

        menu = menuObject.GetComponent<GameMenu>();
        menu.Setup(world);
        openMenus.Add(menu.MenuType, menu);
        return menu;
    }

    public void CloseMenu(GameMenu menu)
    {
        openMenus.Remove(menu.MenuType);
        Destroy(menu.gameObject);
    }

    public void ToggleLevelUp()
    {
        ToggleMenu(MenuType.LevelUp);
    }
}
