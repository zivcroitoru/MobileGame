using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private void Start() => Refresh();

    public void Add(int amount)
    {
        Services.Save.SetInt(
            "coins",
            Services.Save.GetInt("coins") + amount
        );

        Refresh();
    }

    // Other UI components can refresh the coin display.
    public void Refresh()
    {
        label.text = Services.Save.GetInt("coins").ToString();
    }
}