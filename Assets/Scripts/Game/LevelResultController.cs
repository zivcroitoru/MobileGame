using System;
using UnityEngine;

public class LevelResultController : MonoBehaviour
{
    public event Action<bool, int> Finished;

    private bool _finished;

    public void Win()
    {
        var economy = Services.Config.Economy;
        int upgrade = Services.Save.GetInt(UpgradeShop.UpgradeKey);
        Finish(true, economy.winReward + upgrade * economy.rewardBonusPerLevel);
    }

    public void Lose() => Finish(false, Services.Config.Economy.loseReward);

    private void Finish(bool won, int reward)
    {
        if (_finished) return;
        _finished = true;

        int balance = Services.Save.GetInt(CoinCounter.CoinsKey) + reward;
        if (won)
        {
            int unlocked = Mathf.Max(ScreenFlow.UnlockedLevel, ScreenFlow.CurrentLevel + 1);
            Services.Save.SetInts(CoinCounter.CoinsKey, balance, ScreenFlow.UnlockedLevelKey, unlocked);
        }
        else
        {
            Services.Save.SetInt(CoinCounter.CoinsKey, balance);
        }

        Finished?.Invoke(won, reward);
    }
}
