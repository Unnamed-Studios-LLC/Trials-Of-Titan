using Mobile;
using System.Collections;
using System.Collections.Generic;
using TitanCore.Core;
using TitanCore.Net.Packets.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpMenu : GameMenu
{
    public override MenuType MenuType => MenuType.LevelUp;

    public Image plusSpeed;
    public Image plusAttack;
    public Image plusDefense;
    public Image plusVigor;
    public Image plusMaxHealth;

    public Image minusSpeed;
    public Image minusAttack;
    public Image minusDefense;
    public Image minusVigor;
    public Image minusMaxHealth;

    public TextMeshProUGUI labelSpeed;
    public TextMeshProUGUI labelAttack;
    public TextMeshProUGUI labelDefense;
    public TextMeshProUGUI labelVigor;
    public TextMeshProUGUI labelMaxHealth;

    public TextMeshProUGUI labelCost;

    public SideMenuManager sideMenu;

    private Player player;

    private int speedChange;
    private int attackChange;
    private int defenseChange;
    private int vigorChange;
    private int maxHealthChange;

    public override void Setup(World world)
    {
        base.Setup(world);

        player = world.player;

        OnEnable();
    }

    private void OnEnable()
    {
        if (world == null) return;
        player = world.player;

        plusSpeed.gameObject.SetActive(PlusActive(StatType.Speed));
        plusAttack.gameObject.SetActive(PlusActive(StatType.Attack));
        plusDefense.gameObject.SetActive(PlusActive(StatType.Defense));
        plusVigor.gameObject.SetActive(PlusActive(StatType.Vigor));
        plusMaxHealth.gameObject.SetActive(PlusActive(StatType.MaxHealth));

        UpdateLabels();
    }

    private void LateUpdate()
    {
        if (player == null) return;
        UpdateLabels();

        UpdateCost();
    }

    private void UpdateLabels()
    {
        UpdateLabel(StatType.Speed, speedChange, labelSpeed);
        UpdateLabel(StatType.Attack, attackChange, labelAttack);
        UpdateLabel(StatType.Defense, defenseChange, labelDefense);
        UpdateLabel(StatType.Vigor, vigorChange, labelVigor);
        UpdateLabel(StatType.MaxHealth, maxHealthChange, labelMaxHealth);
    }

    private void UpdateLabel(StatType type, int change, TextMeshProUGUI label)
    {
        var stat = player.GetStatBase(type);
        var max = GetMax(type);

        if (type == StatType.MaxHealth)
            label.text = (stat + change * 10).ToString();
        else
            label.text = (stat + change).ToString();

        int newStat = stat + (type == StatType.MaxHealth ? change * 10 : change);
        if (newStat == max)
            label.color = Color.yellow;
        else if (newStat > max)
            label.color = World.soulColor;
        else if (change != 0)
            label.color = Color.green;
        else
            label.color = Color.white;
        //label.color = newStat ==  >= max ? Color.yellow : (change == 0 ? Color.white : Color.green);
    }

    private void UpdateCost()
    {
        var total = GetTotalCost();

        labelCost.text = $"<color=#ffffff>Cost:  </color>{Constants.Souls_Sprite}{total}";

        if (world.player.fullSouls < total)
            labelCost.text += $"\n<color=#ff0000>Not enough essence</color>";
    }

    private int GetTotalCost(int speedAdd = 0, int attackAdd = 0, int defAdd = 0, int vigorAdd = 0, int maxHealthAdd = 0)
    {
        var info = (TitanCore.Data.Entities.CharacterInfo)player.info;

        var speedCost = GetCost(info, StatType.Speed, player.GetStatBase(StatType.Speed), speedChange + speedAdd);
        var attackCost = GetCost(info, StatType.Attack, player.GetStatBase(StatType.Attack), attackChange + attackAdd);
        var defenseCost = GetCost(info, StatType.Defense, player.GetStatBase(StatType.Defense), defenseChange + defAdd);
        var vigorCost = GetCost(info, StatType.Vigor, player.GetStatBase(StatType.Vigor), vigorChange + vigorAdd);
        var maxHealthCost = GetCost(info, StatType.MaxHealth, player.GetStatBase(StatType.MaxHealth) / 10 - 5, maxHealthChange + maxHealthAdd);

        var total = speedCost + attackCost + defenseCost + vigorCost + maxHealthCost;
        return total;
    }

    private int GetCost(TitanCore.Data.Entities.CharacterInfo info, StatType type, int stat, int change)
    {
        return StatFunctions.GetLevelUpCost(info, type, stat, change);
    }

    public void Confirm()
    {
        var total = GetTotalCost();
        if (world.player.fullSouls < total) return;
        var packet = new TnLevelUp((byte)maxHealthChange, (byte)speedChange, (byte)attackChange, (byte)defenseChange, (byte)vigorChange);
        world.gameManager.client.SendAsync(packet);
        Close();
    }

    public void Cancel()
    {
        Close();
    }

    public void ConfirmMobile()
    {
        var total = GetTotalCost();
        if (world.player.fullSouls < total || total == 0) return;
        var packet = new TnLevelUp((byte)maxHealthChange, (byte)speedChange, (byte)attackChange, (byte)defenseChange, (byte)vigorChange);
        world.gameManager.client.SendAsync(packet);
        CancelMobile();
        sideMenu.CloseMenu();
    }

    public void CancelMobile()
    {
        speedChange = 0;
        attackChange = 0;
        defenseChange = 0;
        vigorChange = 0;
        maxHealthChange = 0;

        minusSpeed.gameObject.SetActive(false);
        minusAttack.gameObject.SetActive(false);
        minusDefense.gameObject.SetActive(false);
        minusVigor.gameObject.SetActive(false);
        minusMaxHealth.gameObject.SetActive(false);

        plusSpeed.gameObject.SetActive(PlusActive(StatType.Speed));
        plusAttack.gameObject.SetActive(PlusActive(StatType.Attack));
        plusDefense.gameObject.SetActive(PlusActive(StatType.Defense));
        plusVigor.gameObject.SetActive(PlusActive(StatType.Vigor));
        plusMaxHealth.gameObject.SetActive(PlusActive(StatType.MaxHealth));

        UpdateLabels();
        UpdateCost();
    }

    private bool PlusActive(StatType type)
    {
        return player.GetStatBase(type) < GetMax(type);
    }

    private bool ValidCost()
    {
        return GetTotalCost() > 0;
    }

    private int GetMax(StatType stat)
    {
        var info = (TitanCore.Data.Entities.CharacterInfo)player.info;
        return info.stats[stat].maxValue;
    }

    public void PlusSpeed()
    {
        speedChange++;
        if (!ValidCost())
        {
            speedChange--;
            return;
        }

        if (speedChange == 1)
            minusSpeed.gameObject.SetActive(true);

        if (player.GetStatBase(StatType.Speed) + speedChange == GetMax(StatType.Speed))
        {
            plusSpeed.gameObject.SetActive(false);
            return;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (GetTotalCost(speedAdd: 1) < player.fullSouls)
                PlusSpeed();
        }
    }

    public void MinusSpeed()
    {
        if (speedChange == 0) return;
        speedChange--;

        if (speedChange == 0)
        {
            minusSpeed.gameObject.SetActive(false);
        }
        plusSpeed.gameObject.SetActive(true);
    }

    public void PlusAttack()
    {
        attackChange++;
        if (!ValidCost())
        {
            attackChange--;
            return;
        }

        if (attackChange == 1)
            minusAttack.gameObject.SetActive(true);

        if (player.GetStatBase(StatType.Attack) + attackChange == GetMax(StatType.Attack))
        {
            plusAttack.gameObject.SetActive(false);
            return;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (GetTotalCost(attackAdd: 1) < player.fullSouls)
                PlusAttack();
        }
    }

    public void MinusAttack()
    {
        if (attackChange == 0) return;
        attackChange--;

        if (attackChange == 0)
        {
            minusAttack.gameObject.SetActive(false);
        }
        plusAttack.gameObject.SetActive(true);
    }

    public void PlusDefense()
    {
        defenseChange++;
        if (!ValidCost())
        {
            defenseChange--;
            return;
        }

        if (defenseChange == 1)
            minusDefense.gameObject.SetActive(true);

        if (player.GetStatBase(StatType.Defense) + defenseChange == GetMax(StatType.Defense))
        {
            plusDefense.gameObject.SetActive(false);
            return;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (GetTotalCost(defAdd: 1) < player.fullSouls)
                PlusDefense();
        }
    }

    public void MinusDefense()
    {
        if (defenseChange == 0) return;
        defenseChange--;

        if (defenseChange == 0)
        {
            minusDefense.gameObject.SetActive(false);
        }
        plusDefense.gameObject.SetActive(true);
    }

    public void PlusVigor()
    {
        vigorChange++;
        if (!ValidCost())
        {
            vigorChange--;
            return;
        }

        if (vigorChange == 1)
            minusVigor.gameObject.SetActive(true);

        if (player.GetStatBase(StatType.Vigor) + vigorChange == GetMax(StatType.Vigor))
        {
            plusVigor.gameObject.SetActive(false);
            return;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (GetTotalCost(vigorAdd: 1) < player.fullSouls)
                PlusVigor();
        }
    }

    public void MinusVigor()
    {
        if (vigorChange == 0) return;
        vigorChange--;

        if (vigorChange == 0)
        {
            minusVigor.gameObject.SetActive(false);
        }
        plusVigor.gameObject.SetActive(true);
    }

    public void PlusMaxHealth()
    {
        maxHealthChange++;
        if (!ValidCost())
        {
            maxHealthChange--;
            return;
        }

        if (maxHealthChange == 1)
            minusMaxHealth.gameObject.SetActive(true);

        if (player.GetStatBase(StatType.MaxHealth) + maxHealthChange * 10 == GetMax(StatType.MaxHealth))
        {
            plusMaxHealth.gameObject.SetActive(false);
            return;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (GetTotalCost(maxHealthAdd: 1) < player.fullSouls)
                PlusMaxHealth();
        }
    }

    public void MinusMaxHealth()
    {
        if (maxHealthChange == 0) return;
        maxHealthChange--;

        if (maxHealthChange == 0)
        {
            minusMaxHealth.gameObject.SetActive(false);
        }
        plusMaxHealth.gameObject.SetActive(true);
    }
}