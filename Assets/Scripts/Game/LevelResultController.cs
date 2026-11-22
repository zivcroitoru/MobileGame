using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelResultController : MonoBehaviour
{
    private const string CoinsKey = "coins";
    private const string UpgradeKey = "reward_upgrade_level";

    private bool _finished;

    public void Win()
    {
        var economy = Services.Config.Economy;

        int upgradeLevel = Services.Save.GetInt(UpgradeKey);

        // Win reward increases with purchased upgrades.
        int reward = economy.winReward +
                     upgradeLevel * economy.rewardBonusPerLevel;

        Finish(reward);
    }

    public void Lose()
    {
        Finish(Services.Config.Economy.loseReward);
    }

    private void Finish(int reward)
    {
        // Prevent multiple rewards for the same level.
        if (_finished) return;
        _finished = true;

        int coins = Services.Save.GetInt(CoinsKey);
        Services.Save.SetInt(CoinsKey, coins + reward);

        Debug.Log($"Level finished! Earned {reward} coins");

        SceneManager.LoadScene("Home");
    }
}