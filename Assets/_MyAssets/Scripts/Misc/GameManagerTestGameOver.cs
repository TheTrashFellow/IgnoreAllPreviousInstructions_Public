using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerTestGameOver : MonoBehaviour
{
    [Header("Pour GameOVer")]
    [SerializeField] private GameObject _player = default;


    [Space]
    [Header("Ennemie Spawning Algorithm")]
    [SerializeField] private List<GameObject> _spawnZones = new List<GameObject>();
    [SerializeField] private GameObject _endZone = default;

    private int gasCount = 0;
    private int totalGas = 3;

    private void Start()
    {
        SpawnEnnemies();
    }

    private void Update()
    {
        int total = 0;
        foreach(GameObject spawnZone in _spawnZones)
        {
            total += spawnZone.GetComponent<SpawnZone>().TotalEnnemies();
        }
        Debug.Log("Total : " + total);
    }

    private void SpawnEnnemies()
    {
        switch (gasCount)
        {
            case 0:
                foreach (GameObject spawnZone in _spawnZones)
                {
                    spawnZone.GetComponent<SpawnZone>().DifficultyAdd(1, false);
                }
                break;
            case 1: 
                foreach (GameObject spawnZone in _spawnZones)
                {
                    spawnZone.GetComponent<SpawnZone>().DifficultyAdd(2, false);
                }
                break;
            case 2:
                foreach (GameObject spawnZone in _spawnZones)
                {
                    spawnZone.GetComponent<SpawnZone>().DifficultyAdd(3, true);
                }
                break;
            case 3:
                foreach (GameObject spawnZone in _spawnZones)
                {
                    spawnZone.GetComponent<SpawnZone>().DifficultyAdd(10, true);
                }
                break;
        }
    }

    public void CollectGas()
    {
        gasCount++;
        //Debug.Log($" nbr Gas obtenu: {gasCount}/{totalGas}");

        //_fuelCount.text = $"{gasCount}/{totalGas}";

        _player.GetComponent<Player>().ShowHideFuelUI();

        
        if (gasCount >= totalGas)
        {
            Debug.Log("Tout le Gas recueilli!");
            _endZone.SetActive(true);
            //SceneManager.LoadScene("SceneFIN");

        }
    }

}
