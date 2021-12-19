using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;

public class Wardrobe : StaticObject, IInteractable
{
    public override GameObjectType ObjectType => GameObjectType.Wardrobe;

    public string[] InteractionOptions => new string[] { "Open" };

    public string InteractionTitle => info.name;

    private WardrobeMenu menu;

    public void Interact(int option)
    {
        menu = (WardrobeMenu)world.menuManager.ShowMenu(MenuType.Wardrobe);
        menu.SetGameId(gameId);
    }

    public void OnEnter()
    {

    }

    public void OnExit()
    {
        if (menu == null) return;
        menu.Close();
        menu = null;
    }
}
