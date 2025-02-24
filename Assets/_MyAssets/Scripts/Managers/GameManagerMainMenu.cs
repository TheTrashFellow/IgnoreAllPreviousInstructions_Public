using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class GameManagerMainMenu : MonoBehaviour
{                                                 
    //public static GameManagerMainMenu Instance;

    //Player
    [SerializeField] private Player _player = default;

    //Variables pour les vibrations
    [SerializeField] private float _hapticStrength = 0.5f;
    [SerializeField] private float _hapticDuration = 0.5f;

    //Pour le UI
    [Space]
    [Header("UI et Panels")]
    [SerializeField] private GameObject _mainMenu = default;
    [SerializeField] private GameObject _testMenu = default;    


    //Pour la zone d'essaie   
    [Space]
    [Header("GameObjects")]
    [SerializeField] private GameObject _spawner = default;
    [SerializeField] private GameObject _btnStopTest = default;
    [SerializeField] private GameObject _enemyPrefab = default;
    [SerializeField] private GameObject _buttonPrefab = default;
    [SerializeField] private GameObject _inventorySpawn = default;
    [SerializeField] private GameObject _ennemiesContainer = default;

    //Pour la zone de ressources
    [Space]
    [Header("Pour zone ressource")]
    [SerializeField] private GameObject _shotgun = default;
    [SerializeField] private GameObject _revolver = default;
    private Vector3 _initialPositionShotgun;
    private Vector3 _initialPositionRevolver;
    
    [Space]
    [SerializeField] private GameObject _spawnerBullet = default;
    [SerializeField] private GameObject _bulletPrefab = default;

    [Space]
    [SerializeField] private GameObject _spawnerShell = default;
    [SerializeField] private GameObject _shellPrefab = default;

    [Space]
    [Header("Audio")]
    [SerializeField] private AudioSource _globalBackground = default;
    [SerializeField] private AudioSource _globalMusic = default;
    
    [Space]
    [Header("Bouton")]
    [SerializeField] private GameObject _btnAudio = default;

    private const int numberOfEnemies = 5;
    private GameObject[] _enemies;

    private GameState _actualState;
    public GameState State => _actualState;

    public delegate void ChangeStateHandler(GameState state);
    public event ChangeStateHandler ChangeState;

    private const string _audioKey = "Audio";
    private const string _snapKey = "Snap";

    private void Awake()
    {
        /*if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }*/

        _initialPositionRevolver = _revolver.transform.position;
        _initialPositionShotgun = _shotgun.transform.position;
        
        if (PlayerPrefs.HasKey(_audioKey))
        {
            bool play = PlayerPrefs.GetInt(_audioKey) == 1;
            if (play)
            {
                StartCoroutine(PlaySounds());
            }
            else
            {
                StartCoroutine(StopSounds());
            }
        }
        else
        {
            PlaySounds();

            PlayerPrefs.SetInt(_audioKey, 1);
            PlayerPrefs.Save();                  
        }
    }

    
    private void Start()
    {
        _testMenu.SetActive(false);
        _btnStopTest.SetActive(false);
        _mainMenu.SetActive(true);
    }

    private void Update()
    {        
        if (_actualState == GameState.Test && AreAllEnemiesDead())
        {
            ChangeState?.Invoke(GameState.Done);
            ShowCanvas();
        }
    }

    /* **** PAS SUR SI BESOIN
    private void OnDestroy()
    {
        GameManagerMainMenu.OnGameStateChanged -= UpdateGameState;
    }*/

    
    public void HandleGameStateChange(GameState state)
    {        
        switch (state)
        {
            case GameState.Start:
                HandleStartState();
                break;

            case GameState.Play:
                HandlePlayState();
                break;

            case GameState.Pause:
                HandlePauseState();
                break;

            case GameState.Test:
                HandleTestState();
                break;
            case GameState.Quit:
                HandleQuitState();
                break;
        }
        _actualState = state;
    }

    public void HandleStartState()
    {
        _mainMenu.SetActive(true);
        _testMenu.SetActive(false);
        _btnStopTest.SetActive(false);
    }

    public void HandlePlayState()
    {
        _player.FadeToBlack();
        StartCoroutine(StopSounds());
        StartCoroutine(LoadScene());
    }

    public void HandlePauseState()
    {       
        
    }

    public void HandleTestState()
    {
        _mainMenu.SetActive(false);
        _testMenu.SetActive(false);
        _btnStopTest.SetActive(true);
        SpawnEnemies();
    }

    public void HandleQuitState()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void SpawnEnemies()
    {
        Vector3 raycastPosition = _spawner.transform.position;
        Vector3 forward = _spawner.transform.forward;
        float spread = 45f;
        _enemies = new GameObject[numberOfEnemies];

        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 randomDirection = GetRandomSpreadDirection(spread, forward);

            RaycastHit hit;
            
            if (Physics.Raycast(raycastPosition, randomDirection, out hit, 100))
            {
                GameObject _hitTarget = hit.collider.gameObject;
                if(_hitTarget.tag == "ZoneTest")
                {
                    Vector3 spawnPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z);

                    _enemies[i] = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
                    _enemies[i].transform.parent = _ennemiesContainer.transform;
                }
                else
                {
                    Destroy(_enemies[i]);
                }
            }
        }        
            
    }

    public void SpawnAmmo()
    {

    }

    private Vector3 GetRandomSpreadDirection(float spreadAngle, Vector3 forward)
    {
        Quaternion randomRotation = Quaternion.Euler(
            Random.Range(-spreadAngle, spreadAngle), // Random pitch
            Random.Range(-spreadAngle, spreadAngle), // Random yaw
            0f // Keep roll at 0 for a consistent spread
        );
        return randomRotation * forward; // Rotate the barrel's forward vector
    }

    public bool AreAllEnemiesDead()
    {
        foreach (GameObject enemy in _enemies)
        {         
            if(enemy != null)
            {                
                 return false;  //S'il y a un ennemie qui vit toujours
            }
            
        }
        return true;  //S'il n'y a aucun 
    }

    public void ShowCanvas()
    {
        if (!_testMenu.activeSelf)
        {
            _testMenu.SetActive(true);
        }
    }

    public void GoBackMenu()
    {
        ChangeState?.Invoke(GameState.Start);
    }
    
    public void StartGame()
    {
        ChangeState?.Invoke(GameState.Play);
    }

    public void RestartTest()
    {
        ChangeState?.Invoke(GameState.Test);
    }

    public void RespawnRevolver()
    {
        _revolver.transform.position = _initialPositionRevolver;
    }

    public void RespawnShotgun()
    {
        _shotgun.transform.position = _initialPositionShotgun;
    }

    public void SpawnBullet()
    {
        Instantiate(_bulletPrefab, _spawnerBullet.transform.position, _spawnerBullet.transform.rotation, _spawnerBullet.transform);
    }

    public void SpawnShell()
    {
        Instantiate (_shellPrefab, _spawnerShell.transform.position, _spawnerShell.transform.rotation, _spawnerShell.transform );
    }

    public void StopTest()
    {       
       DestroyEnnemies();
        
       ChangeState?.Invoke(GameState.Start);
    }

    private void DestroyEnnemies()
    {
        NavMeshAgent[] agents = FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
        foreach(GameObject enemy in _enemies)
        {
            if(enemy != null)
            {
                Destroy(enemy);
            }
        }        

        foreach(NavMeshAgent agent in agents)
        {
            if(agent != null)
                Destroy(agent.gameObject);
        }
    }

    private IEnumerator StopSounds()
    {        
        _btnAudio.GetComponent<Button>().interactable = false; 

        float elapsedTime = 0.0f;
        float duration = 1f;

        //float musicVolume = _globalMusic.volume;
        
        float targetVolume = 0f;                

        while (elapsedTime < duration)
        {

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            
            float musicVolume = Mathf.Lerp(_globalMusic.volume, targetVolume, t);
            float backgroundVolume = Mathf.Lerp(_globalBackground.volume, targetVolume, t);

            _globalMusic.volume = musicVolume;
            _globalBackground.volume = backgroundVolume;

            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _globalMusic.volume = 0f;
        _btnAudio.GetComponent<Button>().interactable = true;

        //PlayerPrefs.SetInt(_audioKey, 0);
    }

    private IEnumerator PlaySounds()
    {
        _btnAudio.GetComponent<Button>().interactable = false;
        
        float elapsedTime = 0.0f;
        float duration = 1f;

        float targetVolumeMusic = 1f;
        float targetVolumeBackground = 0.8f;

        while (elapsedTime < duration)
        {
            float tMusic = Mathf.SmoothStep(0f, targetVolumeMusic, elapsedTime / duration);
            float tBackground = Mathf.SmoothStep(0f, targetVolumeBackground, elapsedTime / duration);

            _globalMusic.volume = Mathf.Lerp(0f, 1f, tMusic);
            _globalBackground.volume = Mathf.Lerp(0f, 0.8f, tBackground);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _globalMusic.volume = 1f;
        _globalBackground.volume = 0.8f;
        _btnAudio.GetComponent<Button>().interactable = true;

        //PlayerPrefs.SetInt(_audioKey, 1);
    }

    public bool IsAudioSourceOn()
    {
        bool choix = PlayerPrefs.GetInt(_audioKey) == 1;               

        return choix;
    }

    public void ToggleAudio()
    {
        if(PlayerPrefs.GetInt(_audioKey) == 1)
        {
            PlayerPrefs.SetInt(_audioKey, 0);
            PlayerPrefs.Save();
            StartCoroutine(StopSounds());
        }
        else
        {
            PlayerPrefs.SetInt(_audioKey, 1);
            PlayerPrefs.Save();
            StartCoroutine(PlaySounds());
        }

        
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(4);

        SceneManager.LoadScene(1);
    }
}
