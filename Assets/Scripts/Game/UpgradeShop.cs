using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShop : MonoBehaviour
{
    public const string UpgradeKey = "reward_upgrade_level";

    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text upgradeLabel;
    [SerializeField] private CoinCounter coinCounter;
    private TMP_Text _buyLabel;

    public void Bind(Button button, TMP_Text details, TMP_Text priceLabel, CoinCounter balance)
    {
        buyButton = button;
        upgradeLabel = details;
        _buyLabel = priceLabel;
        coinCounter = balance;
        Refresh();
    }

    private void Start() => Refresh();

    public void Buy()
    {
        int level = Services.Save.GetInt(UpgradeKey);
        int coins = Services.Save.GetInt(CoinCounter.CoinsKey);
        int price = Services.Config.Economy.GetUpgradePrice(level);

        if (price > 0 && coins >= price)
        {
            Services.Save.SetInts(CoinCounter.CoinsKey, coins - price, UpgradeKey, level + 1);
            coinCounter.Refresh();
        }

        Refresh();
    }

    public void Refresh()
    {
        var economy = Services.Config.Economy;
        int level = Services.Save.GetInt(UpgradeKey);
        int coins = Services.Save.GetInt(CoinCounter.CoinsKey);
        int price = economy.GetUpgradePrice(level);

        buyButton.interactable = price > 0 && coins >= price;
        upgradeLabel.text = $"Upgrade level {level}\nCurrent bonus: +{level * economy.rewardBonusPerLevel} per win\nNext level: +{economy.rewardBonusPerLevel} more per win";

        if (_buyLabel != null)
            _buyLabel.text = price <= 0 ? "Max level" : $"Buy for {price:N0} coins";

        if (price <= 0)
            upgradeLabel.text = $"Upgrade level {level}\nCurrent bonus: +{level * economy.rewardBonusPerLevel} per win";
    }
}
