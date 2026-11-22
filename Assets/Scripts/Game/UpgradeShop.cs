using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
    private const string CoinsKey = "coins";
    private const string UpgradeKey = "reward_upgrade_level";

    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text upgradeLabel;
    [SerializeField] private CoinCounter coinCounter;

    private void Start()
    {
        Refresh();
    }

    public void Buy()
    {
        int level = Services.Save.GetInt(UpgradeKey);
        int coins = Services.Save.GetInt(CoinsKey);
        int price = Services.Config.Economy.GetUpgradePrice(level);

        // Only purchase when there are enough coins.
        if (price <= 0 || coins < price)
            return;

        // Persist the coins and upgrade level.
        Services.Save.SetInt(CoinsKey, coins - price);
        Services.Save.SetInt(UpgradeKey, level + 1);

        coinCounter.Refresh();
        Refresh();

        Debug.Log($"Upgrade purchased! New level: {level + 1}");
    }

    private void Refresh()
    {
        var economy = Services.Config.Economy;

        int level = Services.Save.GetInt(UpgradeKey);
        int coins = Services.Save.GetInt(CoinsKey);
        int price = economy.GetUpgradePrice(level);

        // Disable purchasing when coins are insufficient.
        buyButton.interactable = price > 0 && coins >= price;

        upgradeLabel.text =
            $"Reward +{economy.rewardBonusPerLevel}\n" +
            $"Level {level} → {level + 1} | {price} Coins";
    }
}