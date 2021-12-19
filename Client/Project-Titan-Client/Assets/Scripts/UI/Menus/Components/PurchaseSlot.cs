using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PurchaseSlot : MonoBehaviour
{
    public TextMeshProUGUI priceLabel;

    public void SetPrice(int price)
    {
        priceLabel.text = Constants.Premium_Currency_Sprite + price;
    }

    public void OnClick()
    {

    }
}
