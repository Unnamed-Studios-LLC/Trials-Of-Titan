using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestBanner : MonoBehaviour
{
    public Image questImage;

    public TextMeshProUGUI nameLabel;

    public Image healthImage;

    public Slider healthSlider;

    public Image damageImage;

    public Slider damageSlider;

    public Image arrow;

    public TextMeshProUGUI distanceLabel;

    public World world;

    public QuestBannerStyle style;

    private GameObjectInfo questInfo;

    private int damage;

    private bool showing = true;

    private Option questBannerStyle;

    private TnQuest quest;

    private void Awake()
    {
        questBannerStyle = Options.Get(OptionType.QuestBannerStyle);
        questBannerStyle.AddIntCallback(QuestBannerStyleUpdated);
        QuestBannerStyleUpdated(questBannerStyle.GetInt());

        Hide();
    }

    private void OnDestroy()
    {
        questBannerStyle.RemoveIntCallback(QuestBannerStyleUpdated);
    }

    private void QuestBannerStyleUpdated(int styleInt)
    {
        var optionStyle = (QuestBannerStyle)styleInt;
        if (optionStyle != style)
        {
            Hide();
            return;
        }
        world.questBanner = this;
    }

    private void SetQuest(TnQuest quest)
    {
        if (this.quest == quest) return;
        this.quest = quest;

        Hide();

        SetHealthSlider(0);
        SetDamageSlider(0);
        damage = 0;

        if (quest == null || quest.objectType == 0 || !GameData.objects.TryGetValue(quest.objectType, out questInfo))
        {
            questInfo = null;
            return;
        }

        var sprite = TextureManager.GetDisplaySprite(questInfo);
        questImage.rectTransform.sizeDelta = new Vector2(questImage.rectTransform.rect.height * (sprite.textureRect.width / sprite.textureRect.height), questImage.rectTransform.sizeDelta.y);
        questImage.sprite = sprite;

        if (nameLabel != null)
        {
            if (questInfo is EnemyInfo enemyInfo)
                nameLabel.text = enemyInfo.title;
            else
                nameLabel.text = questInfo.name;
        }

        distanceLabel.text = "";
    }

    public void UpdateQuest()
    {
        SetQuest(world.quest);

        if (quest == null || quest.gameId == 0)
        {
            Hide();
            return;
        }

        Show();

        if (!world.TryGetObject(quest.gameId, out var questObject))
        {
            HideStats();
            return;
        }

        ShowStats();

        var angle = ((Vector2)world.player.Position).ToVec2().AngleTo(((Vector2)questObject.transform.localPosition).ToVec2()) * Mathf.Rad2Deg + world.CameraRotation - 90;
        arrow.transform.localEulerAngles = new Vector3(0, 0, angle);

        distanceLabel.text = Mathf.RoundToInt(Vector2.Distance(world.player.Position, questObject.transform.localPosition)).ToString();

        if (!(questObject is Enemy enemy))
        {
            return;
        }

        SetHealthSlider(enemy.health / (float)enemy.maxHealth);
        SetDamageSlider(damage / (float)enemy.maxHealth);
    }

    private void Hide()
    {
        if (!showing) return;
        showing = false;

        gameObject.SetActive(false);
    }

    private void ShowStats()
    {
        arrow.gameObject.SetActive(true);
        distanceLabel.gameObject.SetActive(true);
    }

    private void HideStats()
    {
        arrow.gameObject.SetActive(false);
        distanceLabel.gameObject.SetActive(false);
    }

    private void SetHealthSlider(float value)
    {
        value = Mathf.Clamp01(value);
        if (healthImage != null)
            healthImage.fillAmount = value;
        if (healthSlider != null)
            healthSlider.value = value;
    }

    private void SetDamageSlider(float value)
    {
        value = Mathf.Clamp01(value);
        if (damageImage != null)
            damageImage.fillAmount = value;
        if (damageSlider != null)
            damageSlider.value = value;
    }

    private void Show()
    {
        if (showing || questInfo == null) return;
        showing = true;

        gameObject.SetActive(true);
        gameObject.LeanCancel();
        transform.FadeInMoveUp(0.3f, 0, true);

        var graphic = gameObject.GetComponent<Image>();
        if (graphic != null)
            TweenUtils.FadeIn(gameObject, graphic, 0.3f, 0, 0.7f);
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }
}
