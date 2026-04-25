using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        if (SceneManager.GetActiveScene().name == "MainMenu" && !shownHelp)
        {
            ShowHelp();
            shownHelp = true;
        }
    }

    [SerializeField] private GameObject helpPanel;    
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameoverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject waveIndicator;
    [SerializeField] private TMP_Text waveReachedText;
    
    private static bool shownHelp = false;
    
    public void LoadNormalGame()
    {
        SceneManager.LoadScene("NormalGame");
    }
    
    public void LoadEndlessGame()
    {
        SceneManager.LoadScene("EndlessGame");
    }
    
    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    
    public void ShowHelp()
    {
        mainPanel.SetActive(false);
        helpPanel.SetActive(true);
    }
    
    public void ShowMain()
    {
        helpPanel.SetActive(false);
        playPanel.SetActive(false);
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }
    
    public void ShowPlay()
    {
        mainPanel.SetActive(false);
        playPanel.SetActive(true);
    }
    
    public void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    public void ShowGameOver()
    {
        waveIndicator.SetActive(false);
        waveReachedText.text = $"Wave Reached: {EnemySpawnerRegistry.Current?.CurrentWave}";
        gameoverPanel.SetActive(true);
    }
    
    public void ShowWin()
    {
        waveIndicator.SetActive(false);
        winPanel.SetActive(true);
    }
}
