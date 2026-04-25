using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public List<Transform> spawnPoints;
    public int amount;
    public float interval;
}

[System.Serializable]
public class SpawnWave
{
    public List<EnemySpawnData> spawnData;
}

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }
    public List<SpawnWave> SpawnWaves;
    public int WaveToSpawn = 0;
    int enemiesLeft = 0;
    
    [SerializeField] TMP_Text waveIndicator;
    
    [Header("Areanas")]
    [SerializeField] GameObject Arena1;
    [SerializeField] GameObject Arena2;
    [SerializeField] GameObject ArenaBoss;
    
    [Header("Transition")]
    [SerializeField] Image fadePanel;
    [SerializeField] float fadeDuration = 0.6f;
    
    const int WaveBeforeArena2 = 6;
    const int WaveBeforeArenaBoss = 9;

    void Awake()
    {
        Instance = this;
        Arena1.SetActive(true);
        Arena2.SetActive(false);
        ArenaBoss.SetActive(false);
        StartCoroutine(GameLoop());
    }
    
    IEnumerator GameLoop()
    {
        while(WaveToSpawn < SpawnWaves.Count)
        {
            waveIndicator.text = $"Wave: {WaveToSpawn + 1}";
            yield return new WaitForSeconds(2);
            StartWave();
            yield return new WaitUntil(() => enemiesLeft <= 0);
            
            int justFinished = WaveToSpawn - 1;

            if (justFinished == WaveBeforeArena2)
            {
                yield return StartCoroutine(TransitionToArena(Arena2));
            }
            else if (justFinished == WaveBeforeArenaBoss)
            {
                yield return StartCoroutine(TransitionToArena(ArenaBoss));
            }
            
            yield return new WaitForSeconds(2);

            if (WaveToSpawn < SpawnWaves.Count)
            {
                GameManager.EndOfWave();
                yield return new WaitUntil(() => Time.timeScale == 1f);
            }
        }
        waveIndicator.text = $"All waves cleared, congratulations!";
    }
    
    IEnumerator TransitionToArena(GameObject nextArena)
    {
        yield return StartCoroutine(Fade(0f, 1f));

        Arena1.SetActive(false);
        Arena2.SetActive(false);
        ArenaBoss.SetActive(false);
        nextArena.SetActive(true);

        Transform player = GameManager.Instance.Player.transform.parent;
        player.position = Vector3.zero;

        yield return new WaitForSeconds(0.1f);

        yield return StartCoroutine(Fade(1f, 0f));
    }
    
    IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            c.a = Mathf.Lerp(from, to, t);
            fadePanel.color = c;
            yield return null;
        }

        c.a = to;
        fadePanel.color = c;
    }

    
    public void StartWave()
    {
        foreach (EnemySpawnData spawnData in SpawnWaves[WaveToSpawn].spawnData)
        {
            enemiesLeft += spawnData.amount;
            StartCoroutine(SpawnEnemies(spawnData));
        }
        WaveToSpawn++;
    }

    public void EnemyDeath()
    {
        enemiesLeft--;
    }

    IEnumerator SpawnEnemies(EnemySpawnData spawnData)
    {
        for (int i = 0; i < spawnData.amount; i++)
        {
            yield return new WaitForSeconds(spawnData.interval);
            Instantiate(spawnData.enemyPrefab, spawnData.spawnPoints[Random.Range(0, spawnData.spawnPoints.Count)].position, Quaternion.identity);
        }
    }
}
