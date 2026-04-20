using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
    
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameoverPanel;
    [SerializeField] private GameObject waveIndicator;
    [SerializeField] private TMP_Text waveReachedText;
    
    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
    }
    
    public void LoadMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }
    
    public void ShowMain()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }
    
    public void ShowGameOver()
    {
        waveIndicator.SetActive(false);
        waveReachedText.text = $"Wave Reached: {EnemySpawner.Instance.WaveToSpawn}";
        gameoverPanel.SetActive(true);
    }
}
