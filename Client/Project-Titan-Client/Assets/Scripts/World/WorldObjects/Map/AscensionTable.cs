using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;

public class AscensionTable : StaticObject, IInteractable
{
    public override GameObjectType ObjectType => GameObjectType.AscensionTable;

    public string[] InteractionOptions => new string[] { "Open" };

    public string InteractionTitle => "Ascension Table";

    private AscensionMenu menu;

    public void Interact(int option)
    {
       menu = (AscensionMenu)world.menuManager.ShowMenu(MenuType.Ascension);
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