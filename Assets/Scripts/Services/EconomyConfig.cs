using UnityEngine;

[CreateAssetMenu(
    fileName = "EconomyConfig",
    menuName = "Paw Path/Economy Config")]
public class EconomyConfig : ScriptableObject
{
    [Header("Level Rewards")]
    public int winReward;
    public int loseReward;

    [Header("Upgrades")]
    public int upgradeBasePrice;
    public float upgradeGrowthMultiplier;
    public int rewardBonusPerLevel;

    // Calculates the cost of the next upgrade.
    public int GetUpgradePrice(int currentLevel)
    {
        return Mathf.CeilToInt(
            upgradeBasePrice *
            Mathf.Pow(upgradeGrowthMultiplier, currentLevel)
        );
    }
}