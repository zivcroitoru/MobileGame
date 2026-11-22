using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFlow : MonoBehaviour
{
    public const string UnlockedLevelKey = "unlocked_level";

    public static int CurrentLevel { get; private set; } = 1;
    public static int UnlockedLevel => Mathf.Max(1, Services.Save.GetInt(UnlockedLevelKey, 1));

    private bool _loading;

    public void Play() => LoadLevel(UnlockedLevel);
    public void Retry() => LoadLevel(CurrentLevel);
    public void NextLevel() => LoadLevel(Mathf.Min(CurrentLevel + 1, UnlockedLevel));
    public void Back() => Load("Home");

    private void LoadLevel(int level)
    {
        if (_loading) return;
        CurrentLevel = level;
        Load("Level");
    }

    private void Load(string scene)
    {
        if (_loading) return;
        _loading = true;
        SceneManager.LoadScene(scene);
    }
}
