using TMPro;
using UnityEngine;

public class SoundToggleButton : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    private void Start()
    {
        RefreshLabel();
    }

    public void ToggleSound()
    {
        Services.Settings.SoundOn = !Services.Settings.SoundOn;
        RefreshLabel();
    }

    private void RefreshLabel()
    {
        if (label != null)
            label.text = Services.Settings.SoundOn ? "Sound: ON" : "Sound: OFF";
    }
}