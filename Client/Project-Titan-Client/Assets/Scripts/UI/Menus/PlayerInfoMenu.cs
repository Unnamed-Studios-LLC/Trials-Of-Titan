using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoMenu : MonoBehaviour
{
    public GameObject currentMenu;

    public GameObject loginMenu;

    public TextMeshProUGUI currency;

    private void LateUpdate()
    {
        currency.text = Constants.Premium_Currency_Sprite + Account.describe.currency.ToString();
    }

    public void SetMenu(GameObject menu)
    {
        currentMenu.SetActive(false);
        menu.SetActive(true);
        currentMenu = menu;
    }

    public void OnLogout()
    {
        gameObject.SetActive(false);
        loginMenu.SetActive(true);
    }
}
