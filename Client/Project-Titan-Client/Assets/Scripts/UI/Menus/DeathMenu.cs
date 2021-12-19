using System.Collections;
using System.Collections.Generic;
using TitanCore.Data;
using TitanCore.Data.Entities;
using TitanCore.Net;
using TitanCore.Net.Packets.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    public Image background;

    public Image killerSprite;

    public Image graveSprite;

    public Image playerSprite;

    public Image menuButton;

    public Image vignette;

    public TextMeshProUGUI youDiedLabel;

    public TextMeshProUGUI killedLabel;

    public TextMeshProUGUI playerInfoLabel;

    public TextMeshProUGUI soulsLabel;

    public void Show(Player player, TnDeath death)
    {
        if (gameObject.activeSelf) return;
        player.world.gameManager.client.SetDisconnectCallback(null);
        player.world.gameManager.client.Disconnect();

        SetInfo(player, death);

        gameObject.SetActive(true);

        DoAnimations(death);

        if (AudioManager.TryGetSound("Fallen_Hero", out var music))
        {
            var musicPlayer = AudioManager.GetBackgroundAudioPlayer();
            musicPlayer.ClearQueue();
            musicPlayer.Play(music, false);
        }
    }

    private void SetInfo(Player player, TnDeath death)
    {
        var level = player.GetLevel();

        SetAnchored(killerSprite, TextureManager.GetDisplaySprite(death.killer));
        SetAnchored(playerSprite, TextureManager.GetDisplaySprite(player.GetSkinInfo()));
        SetAnchored(graveSprite, TextureManager.GetDisplaySprite(NetConstants.GetGravestoneType(level, out var lifetime)));

        var killerInfo = GameData.objects[death.killer];
        string killerName = killerInfo.name;
        if (killerInfo is EnemyInfo enemyInfo)
            killerName = enemyInfo.title;

        killedLabel.text = $"Killed on {death.deathTime.ToLocalTime().ToString("MMMM d, yyy")} by {killerName}";
        killedLabel.ForceMeshUpdate();

        playerInfoLabel.text = $"Level {level} {player.info.name}";
        playerInfoLabel.ForceMeshUpdate();

        soulsLabel.text = $"{Constants.Death_Currency_Sprite}0";
        soulsLabel.ForceMeshUpdate();
    }

    private void SetAnchored(Image image, Sprite sprite)
    {
        image.sprite = sprite;
        SpriteUtils.SetAnchorRatio(image.rectTransform, sprite);
    }

    private void DoAnimations(TnDeath death)
    {
        var whiteClear = Color.white;
        whiteClear.a = 0;

        var redClear = Color.red;
        redClear.a = 0;

        youDiedLabel.color = redClear;
        killedLabel.color = whiteClear;
        playerInfoLabel.color = whiteClear;

        killerSprite.color = whiteClear;
        playerSprite.color = whiteClear;
        graveSprite.color = whiteClear;
        menuButton.color = whiteClear;

        vignette.color = redClear;
        LeanTween.value(vignette.gameObject, color =>
        {
            vignette.color = color;
        }, vignette.color, Color.red, 1.5f).setEaseInSine();

        background.color = Color.clear;
        var blackAlpha = Color.black;
        blackAlpha.a = 0.8f;
        LeanTween.value(background.gameObject, color =>
        {
            background.color = color;
        }, Color.clear, blackAlpha, 1.5f).setEaseInSine();

        youDiedLabel.rectTransform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        var youDiedOriginalPosition = youDiedLabel.rectTransform.position;
        youDiedLabel.rectTransform.position = new Vector3(Screen.width / 2, Screen.height / 2);

        float youDiedWaitTime = 1;
        float youDiedFadeInTime = 3;

        float killerSpriteFadeInTime = 1f;
        float killerSpriteWaitTime = youDiedWaitTime + youDiedFadeInTime - killerSpriteFadeInTime + 0.3f;

        // You died label
        LeanTween.sequence().append(youDiedWaitTime).append(youDiedLabel.rectTransform.LeanScale(new Vector3(1, 1, 1), youDiedFadeInTime).setEaseOutSine());//.append(youDiedPauseTime).append(youDiedLabel.rectTransform.LeanScale(Vector3.one, youDiedMoveTime));
        LeanTween.sequence().append(youDiedWaitTime).append(youDiedLabel.transform.LeanMove(youDiedOriginalPosition, youDiedFadeInTime).setEaseOutSine());
        LeanTween.sequence().append(youDiedWaitTime).append(LeanTween.value(youDiedLabel.gameObject, color =>
        {
            youDiedLabel.color = color;
        }, redClear, new Color(0.7f, 0, 0), youDiedFadeInTime).setEaseOutSine());

        killerSprite.transform.FadeInMoveUp(killerSpriteFadeInTime, killerSpriteWaitTime, false);

        float killedLabelFadeInTime = 1;
        float killedLabelWaitTime = killerSpriteWaitTime + killerSpriteFadeInTime - 0.2f;

        killedLabel.transform.FadeInMoveUp(killedLabelFadeInTime, killedLabelWaitTime, false);
        playerInfoLabel.transform.FadeInMoveUp(killerSpriteFadeInTime, killedLabelWaitTime + 0.6f, false);

        graveSprite.transform.FadeInMoveUp(killerSpriteFadeInTime, killedLabelWaitTime + 0.2f, false);
        playerSprite.transform.FadeInMoveUp(killerSpriteFadeInTime, killedLabelWaitTime + 0.4f, false);

        soulsLabel.transform.FadeInMoveUp(killerSpriteFadeInTime, killedLabelWaitTime + 0.6f, false);
        LeanTween.sequence().append(killedLabelWaitTime + 0.6f).append(LeanTween.value(soulsLabel.gameObject, value =>
        {
            soulsLabel.text = $"{Constants.Death_Currency_Sprite}{Mathf.RoundToInt(value)}";
        }, 0, death.baseReward, 2.8f).setEaseOutQuart());

        menuButton.transform.FadeInMoveUp(killerSpriteFadeInTime, killedLabelWaitTime + 0.7f, false);
    }

    public void GotoMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
