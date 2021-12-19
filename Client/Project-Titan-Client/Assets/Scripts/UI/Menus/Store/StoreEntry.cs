using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Iap;
using TMPro;
using UnityEngine;

public class StoreEntry : MonoBehaviour
{
    public TextMeshProUGUI priceLabel;

    public TextMeshProUGUI rewardLabel;

    public IapProduct product;

    public void Setup(IapProduct product, string priceString)
    {
        this.product = product;

        priceLabel.text = priceString;
        rewardLabel.text = Constants.Premium_Currency_Sprite + product.currencyReward.ToString();
    }
}
