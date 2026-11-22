using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    public const string CoinsKey = "coins";

    [SerializeField] private TMP_Text label;

    public void Bind(TMP_Text target)
    {
        label = target;
        Refresh();
    }

    private void Start() => Refresh();

    public void Add(int amount)
    {
        Services.Save.SetInt(
            CoinsKey,
            Services.Save.GetInt(CoinsKey) + amount
        );

        Refresh();
    }

    // Other UI components can refresh the coin display.
    public void Refresh()
    {
        label.text = Services.Save.GetInt(CoinsKey).ToString("N0");
    }
}
