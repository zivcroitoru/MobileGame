using UnityEngine;

public class SettingsService : ISettingsService
{
    private const string SoundKey = "sound_on";
    private readonly ISaveService _save;

    public SettingsService(ISaveService save)
    {
        _save = save;
        ApplyAudio();
    }

    public bool SoundOn
    {
        get => _save.GetInt(SoundKey, 1) == 1;
        set
        {
            _save.SetInt(SoundKey, value ? 1 : 0);
            ApplyAudio();
        }
    }

    private void ApplyAudio()
    {
        AudioListener.volume = SoundOn ? 1f : 0f;
    }
}