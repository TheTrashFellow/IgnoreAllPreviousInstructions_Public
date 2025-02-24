using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using UnityEngine;
using TMPro;
using UnityEngine.AI;
using UnityEngine.XR;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;


public class GameManager : MonoBehaviour
{    

    //Gas
    private int gasCount = 0;
    private int totalGas = 3;

    //Management Gas
    [Header("Management objecitfs")]
    [SerializeField] private GameObject _endZone = default;
    [SerializeField] private GameObject[] spawnKeysPositions = default;
    [SerializeField] private GameObject keySpawnObjet = default;
    [SerializeField] private TMP_Text _fuelCount = default;

    //Vie et ressources
    private int _revolverRessource = 6;
    private int _shotGunRessource = 2;

    //Management des ressources
    [SerializeField] private GameObject[] spawnRessourcesPositions = default;
    [SerializeField] private GameObject ressourceSpawnObjet = default;

    //Player
    [Space]
    [Header("Player")]
    [SerializeField] private GameObject _player = default;

    //Variables pour les vibrations
    [SerializeField] private float _hapticStrength = 0.5f;
    [SerializeField] private float _hapticDuration = 0.5f;

    //Pour la zone d'essaie et UI
    [Space]
    [Header("UI et Panels")]
    [SerializeField] private GameObject _pauseMenu = default;

    [Space]
    [Header("GameObject")]
    [SerializeField] private GameObject _gameObjectWeapon = default;

    [Space]
    [Header("Audio")]
    [SerializeField] private AudioSource _globalBackground = default;
    [SerializeField] private AudioSource _globalMusic = default;
    [SerializeField] private AudioSource _globalEnd = default;

    [Space]
    [Header("Bouton")]
    [SerializeField] private GameObject _btnAudio = default;

    [Space]
    [Header("Ennemie Spawning Algorithm")]
    [SerializeField] private List<GameObject> _spawnZones = new List<GameObject>();
    

    private Dictionary<int, GameObject> _instantiatedWeaponModelsID = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> _instantiatedBulletModelsID = new Dictionary<int, GameObject>();
    private Camera _playerCamera;

    public delegate void ChangeStateHandler(GameState state);
    public event ChangeStateHandler ChangeState;
    private int killCount = 0;
    
    private const string _audioKey = "Audio";

    public int RevolverRessource { get => _revolverRessource; set => _revolverRessource = value; }
    public int ShotGunRessource { get => _shotGunRessource; set => _shotGunRessource = value; }

    private void Awake()
    {
        _player.GetComponent<Player>().GameOver += OnGameOver;
        
        //GameManagerMainMenu.OnGameStateChanged += HandleGameStateChange;
    }

    float sceneStartTime;

    private void Start()
    {
        StaticVariables.gameTime = 0f;
        StaticVariables.gameKills = 0;
        sceneStartTime = Time.time;

        _endZone.GetComponent<EndZone>().EndReached += SaveStats;

        _playerCamera = Camera.main;
        StartCoroutine(InitiatePlayState());
        
        foreach(GameObject zone in _spawnZones)
        {
            zone.GetComponent<SpawnZone>().EnnemieDestroyed += AddKillCounter;
        }
        
        if (!PlayerPrefs.HasKey(_audioKey))
        {
            StartCoroutine(PlaySounds());

            PlayerPrefs.Save();
        }
        else
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
    }

    private void SaveStats()
    {
        StaticVariables.gameKills = killCount;
        StaticVariables.gameTime = Time.time - sceneStartTime;
    }

    private void AddKillCounter()
    {
        killCount++;
        StartCoroutine(PlaySounds());
    }

    private IEnumerator InitiatePlayState()
    {
        yield return new WaitForSeconds(0.5f);
        ChangeState?.Invoke(GameState.Play);
    }

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

            //case GameState.Test:
            //    HandleTestState();
            //    break;
            //case GameState.Quit:
            //    HandleQuitState();
            //    break;
        }
    }

    private void SpawnEnnemies()
    {
        switch (gasCount)
        {
            case 0:
                foreach (GameObject spawnZone in _spawnZones)
                {
                    StartCoroutine(WaitAFrame());
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

    private IEnumerator WaitAFrame()
    {
        yield return null;
    }

    public void HandleStartState()
    {
        _player.GetComponent<Player>().FadeToBlack();
        _player.GetComponent<Player>().FadeDeathMusic();
        StartCoroutine(LoadScene());
    }

    public void HandlePlayState()
    {
        SpamnRessourcesBox();
        SpawnGas();
        SpawnEnnemies();
    }

    public void HandlePauseState()
    {
        _pauseMenu.SetActive(true);
    }

    public bool IsAudioOn()
    {
        return PlayerPrefs.GetInt(_audioKey) == 1;        
    }

    public void ToggleAudio()
    {
        if (PlayerPrefs.GetInt(_audioKey) == 1)
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

    


    public void SelectWeapon(GameManager gm, Weapon selectedWeapon, GameObject button)
    {
       

        if (_instantiatedWeaponModelsID.ContainsKey(selectedWeapon.WeaponID))
        {
            
            //_instantiatedWeaponModelsID[selectedWeapon.WeaponID].transform.localRotation = Quaternion.identity;
            _instantiatedWeaponModelsID[selectedWeapon.WeaponID].transform.position = _player.GetComponent<Player>()._spawnInventaire.transform.position;
            _instantiatedWeaponModelsID[selectedWeapon.WeaponID].transform.LookAt(button.transform.position);

            _instantiatedWeaponModelsID[selectedWeapon.WeaponID].GetComponent<Rigidbody>().useGravity = false;
            _instantiatedWeaponModelsID[selectedWeapon.WeaponID].GetComponent<Rigidbody>().isKinematic = true;

            //StartCoroutine(UpdateWeaponPosition(_instantiatedWeaponModelsID[selectedWeapon.WeaponID], button));
            //StartCoroutine(MoveWeaponToPlayer(_instantiatedWeaponModelsID[selectedWeapon.WeaponID]));
        }
        else
        {
            CreateWeapon(selectedWeapon, button);
        }
    }

    public GameObject CreateWeapon(Weapon selectedWeapon, GameObject button)
    {

        GameObject weaponModelInstance = Instantiate(selectedWeapon.WeaponModel);

        weaponModelInstance.transform.SetParent(_gameObjectWeapon.transform);

        if (weaponModelInstance.gameObject.tag == "Axe")
            weaponModelInstance.transform.localScale = new Vector3(0.007f, 0.007f, 0.007f);
        else
            weaponModelInstance.transform.localScale = Vector3.one;

        weaponModelInstance.transform.localRotation = Quaternion.identity;
        weaponModelInstance.transform.position = _player.GetComponent<Player>()._spawnInventaire.transform.position;
        weaponModelInstance.transform.LookAt(_playerCamera.transform.position);

        weaponModelInstance.GetComponent<Rigidbody>().useGravity = false;
        weaponModelInstance.GetComponent<Rigidbody>().isKinematic = true;

        _instantiatedWeaponModelsID[selectedWeapon.WeaponID] = weaponModelInstance;

        //StartCoroutine(UpdateWeaponPosition(weaponModelInstance, button));

        Vector3 targetPosition = button.transform.TransformPoint(button.transform.localPosition);

        // Update the weapon's position to match the button's position on the canvas
        weaponModelInstance.transform.position = targetPosition;

        // Optionally, ensure the weapon model still faces the player
        weaponModelInstance.transform.LookAt(-_playerCamera.transform.position);

        StartCoroutine(MoveWeaponToPlayer(_instantiatedWeaponModelsID[selectedWeapon.WeaponID]));

        return weaponModelInstance;
    }    

    public void SelectBullet(GameManager canvas, Bullet selectedBullet, GameObject button)
    {
        //Debug.Log("Selected Weapon: " + selectedBullet.GetComponent<Bullet>().BulletName + "\nSelected Weapon ID : " + selectedBullet.GetComponent<Bullet>().BulletID);

        if (selectedBullet.GetComponent<Bullet>().BulletID == 0)
        {
            if (_revolverRessource > 0)
            {
                CreateBullet(selectedBullet, button);
                _revolverRessource--;
            }
        }
        else
        {
            if (_shotGunRessource > 0)
            {
                CreateBullet(selectedBullet, button);
                _shotGunRessource--;
            }
        }
    }

    public void CreateBullet(Bullet selectedBullet, GameObject button)
    {
        GameObject bulletModelInstance = Instantiate(selectedBullet.BulletModel);

        bulletModelInstance.transform.SetParent(_gameObjectWeapon.transform.parent);

        bulletModelInstance.transform.localScale = Vector3.one;
        bulletModelInstance.transform.localRotation = Quaternion.identity;
        bulletModelInstance.transform.localPosition = _player.GetComponent<Player>()._spawnInventaire.transform.position;
        bulletModelInstance.transform.LookAt(_playerCamera.transform.position);

        bulletModelInstance.GetComponent<Rigidbody>().useGravity = false;
        bulletModelInstance.GetComponent<Rigidbody>().isKinematic = true;

        _instantiatedBulletModelsID[selectedBullet.BulletID] = bulletModelInstance;

        //StartCoroutine(UpdateWeaponPosition(bulletModelInstance, button));

        //StartCoroutine(MoveWeaponToPlayer(_instantiatedBulletModelsID[selectedBullet.BulletID]));
    }

    public IEnumerator MoveWeaponToPlayer(GameObject selectedWeapon)
    {
        Vector3 targetPosition = _player.GetComponent<Player>()._spawnInventaire.gameObject.transform.position;
        //if (!selectedWeapon.GetComponent<RevolverManager>().IsGunHeld)
        //{
            //Vector3 targetPosition = selectedWeapon.transform.position + selectedWeapon.transform.forward * 1f;

            if (selectedWeapon.GetComponent<Weapon>() != null)
            {
                if (selectedWeapon.GetComponent<Weapon>().WeaponID != 1)
                {
                    targetPosition.x -= 0.5f;
                }
            }
            else
            {
                targetPosition.x -= 0.8f;
            }

            float journeyLength = Vector3.Distance(selectedWeapon.transform.position, targetPosition);
            float startTime = Time.time;

            while (Vector3.Distance(selectedWeapon.transform.position, targetPosition) > 0.1f)
            {
                float distanceCovered = (Time.time - startTime) * 2f;
                float fractionOfJourney = distanceCovered / journeyLength;

                selectedWeapon.transform.position = Vector3.Lerp(selectedWeapon.transform.position, targetPosition, fractionOfJourney);

                yield return null;
            }

            selectedWeapon.transform.position = targetPosition;
        //}
    }

    /*public IEnumerator UpdateWeaponPosition(GameObject weaponModel, GameObject button)
    {
        // This coroutine will update the weapon's position every frame to follow the button's position
        if (GetComponent<RevolverManager>().IsGunHeld)
            while (weaponModel != null)
            {
                // Get the updated world position of the button
                

                yield return null; // Wait for the next frame
            }
        else
            Debug.Log("Gun is not held");
        yield return null;
    }*/


    private void OnGameOver()
    {
        StartCoroutine(StopSounds());
    }
    private IEnumerator StopSounds()
    {
        _btnAudio.GetComponent<Button>().interactable = false;

        bool _isSoundOn = IsAudioOn();        

        AudioSource deathMusic = _player.GetComponent<Player>()._deathMusic;

        float elapsedTime = 0.0f;
        float duration = 1f;

        //float musicVolume = _globalMusic.volume;

        float targetVolume = 0f;

        while (elapsedTime < duration)
        {

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            if (!_isSoundOn)
            {
                float deathVolume = Mathf.Lerp(deathMusic.volume, targetVolume, t);
                deathMusic.volume = deathVolume;
            }
            
            float musicVolume = Mathf.Lerp(_globalMusic.volume, targetVolume, t);
            float backgroundVolume = Mathf.Lerp(_globalBackground.volume, targetVolume, t);

            
            _globalMusic.volume = musicVolume;
            _globalBackground.volume = backgroundVolume;


            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _globalMusic.volume = 0f;
        _btnAudio.GetComponent<Button>().interactable = true;        
    }
    
    public float DynamicBackgroundTarget()
    {
        if (killCount == 0)
        {
            return 0.5f;
        }
        else 
            return 0f;
    }

    public float DynamicMusicTarget()
    {
        

        if (killCount == 0)
        {
            return 0f;
        }
        else
        {
            _globalMusic.gameObject.SetActive(true);            
            return 1f;
        }

        if(gasCount == 3)
        {
            _globalMusic.gameObject.SetActive(false);
            _globalEnd.gameObject.SetActive(true);
        }
            
    }


    private IEnumerator PlaySounds()
    {
        _btnAudio.GetComponent<Button>().interactable = false;
        
        float elapsedTime = 0.0f;
        float duration = 1f;

        float targetVolumeMusic = DynamicMusicTarget();
        float targetVolumeBackground = DynamicBackgroundTarget();

        while (elapsedTime < duration)
        {
            float tMusic = Mathf.SmoothStep(0f, targetVolumeMusic, elapsedTime / duration);
            float tBackground = Mathf.SmoothStep(0f, targetVolumeBackground, elapsedTime / duration);
            float tEnd = 0f;
            if (gasCount == 3)
            {
                tEnd = Mathf.SmoothStep(0f, targetVolumeMusic, elapsedTime / duration);
            }

            _globalMusic.volume = Mathf.Lerp(0f, 1f, tMusic);            
            _globalBackground.volume = Mathf.Lerp(0f, 0.8f, tBackground);
            _globalEnd.volume = Mathf.Lerp(0f, 0.8f, tEnd);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _globalMusic.volume = targetVolumeMusic;
        _globalBackground.volume = targetVolumeBackground;

        if(gasCount == 3)
        {
            _globalEnd.volume = targetVolumeMusic;
        }

        _btnAudio.GetComponent<Button>().interactable = true;

        //PlayerPrefs.SetInt(_audioKey, 1);
    }

    void SpamnRessourcesBox()
    {
        foreach (GameObject spawnPoint in spawnRessourcesPositions)
        {
            Vector3 position = spawnPoint.transform.position;
            GameObject ressource = Instantiate(ressourceSpawnObjet, position, Quaternion.identity);
            ressource.GetComponent<Destructible>()._player = _player; 
        }
    }

    public void GainRessourceShotGunBullet()
    {
        _shotGunRessource += 2;
    }

    public void GainRessourceRevolverBullet()
    {
        _revolverRessource += 6;
    }

    public void TriggerHapticFeedback()
    {
        StartCoroutine(HapticFeedbackCoroutine());
    }

    public void SpawnGas()
    {
        if (spawnKeysPositions.Length < 3 || keySpawnObjet == null)
        {
            Debug.LogWarning("Not enough spawn points or no object to spawn assigned!");
            return;
        }

        // Create a list to keep track of used spawn points to avoid duplicates
        List<int> usedIndices = new List<int>();

        for (int i = 0; i < 3; i++) // Spawn 3 objects
        {
            int randomIndex;

            // Find a unique random index
            do
            {
                randomIndex = Random.Range(0, spawnKeysPositions.Length);
            }
            while (usedIndices.Contains(randomIndex)); // Ensure the index isn't reused

            // Add the index to the used list
            usedIndices.Add(randomIndex);

            // Access the transform of the chosen spawn point
            Transform chosenSpawnPoint = spawnKeysPositions[randomIndex].transform;

            // Instantiate the object at the chosen spawn point's position and rotation
            Instantiate(keySpawnObjet, chosenSpawnPoint.position, chosenSpawnPoint.rotation);
        }
    }



    public void CollectGas()
    {
        gasCount++;

        _fuelCount.text = $"{gasCount}/{totalGas}";

        _player.GetComponent<Player>().ShowHideFuelUI();
        SpawnEnnemies();
        /*
        if (gasCount >= totalGas)
        {
            Debug.Log("Tout le Gas recueilli!");

            SceneManager.LoadScene("SceneFIN");

        }*/
    }

    


    public int GetGasCount()
    {
        return gasCount;
    }

    public void QuitMenu()
    {
        SceneManager.LoadScene(0);
    }

    private IEnumerator HapticFeedbackCoroutine()
    {
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (leftHand.isValid)
        {
            leftHand.SendHapticImpulse(0, _hapticStrength, _hapticDuration);
        }

        if (rightHand.isValid)
        {
            rightHand.SendHapticImpulse(0, _hapticStrength, _hapticDuration);
        }

        yield return new WaitForSeconds(_hapticDuration);
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(4);

        SceneManager.LoadScene(1);
    }
}
