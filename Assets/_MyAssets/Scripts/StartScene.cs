using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour
{
    [SerializeField] private GameObject _enemyPrefab = default;
    [SerializeField] private GameObject _restartButton = default;
    [SerializeField] private GameObject _startButton = default;
    [SerializeField] private GameObject _practiceArea = default;

    private const int numberOfEnemies = 5;
    private GameObject[] enemies;

    private void Start()
    {
        _restartButton.SetActive(false);
        _startButton.SetActive(false);

        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        Vector3 areaCenter = _practiceArea.transform.position;
        Vector3 areaSize = _practiceArea.transform.localScale;

        enemies = new GameObject[numberOfEnemies];

        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(areaCenter.x - areaSize.x / 2, areaCenter.x + areaSize.x / 2),
                0,
                Random.Range(areaCenter.z - areaSize.z / 2, areaCenter.z + areaSize.z / 2)
            );

            enemies[i] = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void Update()
    {
        if (AreAllEnemiesDead())
        {
            ShowButtons();
        }
    }

    public bool AreAllEnemiesDead()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null) 
            {
                return false;
            }
        }
        return true;
    }

    public void ShowButtons()
    {
        if (!_restartButton.activeSelf && !_startButton.activeSelf)
        {
            _restartButton.SetActive(true);
            _startButton.SetActive(true);
        }
    }

    public void RestartPractice()
    {
        DestroyEnemies();

        SpawnEnemies();

        _restartButton.SetActive(false);
        _startButton.SetActive(false);
    }

    public void DestroyEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
    }
}