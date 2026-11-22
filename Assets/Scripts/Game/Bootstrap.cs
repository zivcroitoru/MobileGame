using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField]
    private EconomyConfig economyConfig;

    private void Awake()
    {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

        if (economyConfig == null)
        {
            Debug.LogError("Bootstrap: EconomyConfig is missing!");
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Initialize services in dependency order.
        Services.Save = new PlayerPrefsSaveService();
        Services.Config = new ConfigService(economyConfig);
        Services.Settings = new SettingsService(Services.Save);

        Debug.Log("Bootstrap: services ready");
        SceneManager.LoadScene("Home");
    }
}