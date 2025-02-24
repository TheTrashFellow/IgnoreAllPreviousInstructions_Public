using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class UIManagerInGame : MonoBehaviour
{
    [Header("Pour menus de pause")]    
    [SerializeField] private GameObject _UIPauseCanvas = default;
    [SerializeField] private GameObject _UIOptionsCanvas = default;
    [SerializeField] private GameObject _UIConfirmationRecommencer = default;
    [SerializeField] private GameObject _UIConfirmationQuitter = default;
    [SerializeField] private GameObject _deathMenu = default;

    [Space]
    [Header("Pour menu options")]
    [SerializeField] private TextMeshProUGUI _TextSnapTurn = default;
    [SerializeField] private TextMeshProUGUI _TextAudio = default;

    [SerializeField] private GameObject _UIWeaponsCanvas = default;

    [SerializeField] private CanvasGroup _ballesPanel = default;
    [SerializeField] private CanvasGroup _weaponsPanel = default;

    [SerializeField] private List<Bullet> _listBullets;
    [SerializeField] private List<Weapon> _listWeapons;
    [SerializeField] private GameObject _buttonPrefab;
    [SerializeField] private GameObject _buttonPrefabBullet;

    [SerializeField] private GameObject _weaponButtonParent;
    [SerializeField] private GameObject _bulletButtonParent;

    [SerializeField] private GameObject _gameObjectWeapon = default;
    [SerializeField] private Player _player = default;

    [Space]
    [Header("BOUTTONS")]
    [SerializeField] private Button _btnWeapons = default;
    [SerializeField] private Button _btnBullets = default;
    [SerializeField] private Button _btnRetour = default;

    private CanvasGroup _activePanel;

    public bool _isMenuOpen;
    private bool _isPauseOpen;
    private bool _isOptionsOpen;
    private bool _isConfRestartOpen;
    private bool _isConfQuitOpen;

    private Camera _playerCamera;
    [SerializeField] private GameManager _gameManager;

    private Dictionary<int, GameObject> _instantiatedWeaponModelsID = new Dictionary<int, GameObject>();
    private Dictionary<int, GameObject> _instantiatedBulletModelsID = new Dictionary<int, GameObject>();

    public delegate void ChangeStateHandler(GameState state);
    public event ChangeStateHandler ChangeState;

    private void Awake()
    {
        _UIWeaponsCanvas.SetActive(true);

        StartCoroutine(SetupUISequence());

        _UIWeaponsCanvas.SetActive(false);
    }

    private void Start()
    {
        _isMenuOpen = false;
        _isPauseOpen = false;
        _isOptionsOpen = false;
        _isConfRestartOpen = false;
        _isConfQuitOpen = false;

        _activePanel = null;
        _ballesPanel.alpha = 0;
        _weaponsPanel.alpha = 0;

        _playerCamera = Camera.main;

        bool initialState = _player.GetComponent<Player>().IsSnapTurnEnabled();
        _TextSnapTurn.text = initialState ? "ON" : "OFF";

        bool initialAudio = _gameManager.IsAudioOn();
        _TextAudio.text = initialAudio ? "ON" : "OFF";
    }

    private void Update()
    {
        if (_bulletButtonParent.transform.childCount != 0)
        {
            for (int i = 0; i < _bulletButtonParent.transform.childCount; i++)
            {
                GameObject button = _bulletButtonParent.transform.GetChild(i).gameObject;

                if (button != null)
                {
                    if (button.transform.Find("TxtNom").GetComponent<TMP_Text>().text == "Revolver")
                    {
                        button.transform.Find("TxtQuantite").GetComponent<TMP_Text>().text = _gameManager.RevolverRessource.ToString();
                    }
                    else
                    {
                        button.transform.Find("TxtQuantite").GetComponent<TMP_Text>().text = _gameManager.ShotGunRessource.ToString();
                    }
                }
            }
        } 
    }

    public void ToggleMenu()
    {
        //if (GameManagerMainMenu.Instance.State != GameState.Start)
        //{
        _isMenuOpen = !_isMenuOpen;

        _UIWeaponsCanvas.SetActive(_isMenuOpen);
        //OnWeaponsClick();

        
        //if (_isMenuOpen)
        //{
        //    GameManagerMainMenu.Instance.UpdateGameState(GameState.Pause);
        //}
        //else
        //{
        //    GameManagerMainMenu.Instance.UpdateGameState(GameState.Play);
        //}
        //}
    }
    public void OnWeaponsClick()
    {
        if (_activePanel != _weaponsPanel)
        {
            StartCoroutine(FadePanels(_activePanel, _weaponsPanel));
            _activePanel = _weaponsPanel;
            DisplayWeapons();
        }
    }

    public void OnBulletsClick()
    {
        if (_activePanel != _ballesPanel)
        {
            StartCoroutine(FadePanels(_activePanel, _ballesPanel));
            _activePanel = _ballesPanel;
            DisplayBullets();
        }
    }

    public void OnRetourClick()
    {
        ToggleMenu();
    }

    public void TogglePause()
    {        
        _isMenuOpen = !_isMenuOpen;

        _UIPauseCanvas.SetActive(_isMenuOpen);
        _UIOptionsCanvas.SetActive(false);
        _UIConfirmationRecommencer.SetActive(false);
        _UIConfirmationQuitter.SetActive(false);
    }

    public void ToggleOptions()
    {
        _isPauseOpen = !_isPauseOpen;

        _UIPauseCanvas.SetActive(!_isPauseOpen);
        _UIOptionsCanvas.SetActive(_isPauseOpen);
    }

    public void ToggleTextSnapTurn()
    {
        bool isSnapTurnOn = _player.IsSnapTurnEnabled();

        _TextSnapTurn.text = isSnapTurnOn ? "OFF" : "ON";

        _player.ChangerSnapTurn();
    }
    
    public void ToggleAudio()
    {

        bool isAudioTurnOn = _gameManager.IsAudioOn();        

        _TextAudio.text = isAudioTurnOn ? "OFF" : "ON";
        _gameManager.ToggleAudio();
    }

    /*
    public void ToggleTextAudio()
    {
        bool isMusicOn = GameManagerMainMenu.Instance.ChoixAudioSource();

        _TextAudio.text = isMusicOn ? "ON" : "OFF";
    }*/

    public void ToggleConfRestart()
    {
        _isConfRestartOpen = !_isConfRestartOpen;

        _UIPauseCanvas.SetActive(!_isConfRestartOpen);
        _UIConfirmationRecommencer.SetActive(_isConfRestartOpen);
    }    

    public void Restart()
    {
        TogglePause();
        ChangeState?.Invoke(GameState.Start);
    }

    public void RestartDeath()
    {
        ToggleDeathMenu();
        _player.FadeMusic();
        ChangeState?.Invoke(GameState.Start);
    }

    private void ToggleDeathMenu()
    {
        _deathMenu.SetActive(false);
    }



    public void ToggleConfQuit()
    {
        _isConfQuitOpen = !_isConfQuitOpen;

        _UIPauseCanvas.SetActive(!_isConfQuitOpen);
        _UIConfirmationQuitter.SetActive(_isConfQuitOpen);
    }

    public void Quit()
    {
        ChangeState?.Invoke(GameState.Quit);
    }

    public void OnPauseClick()
    {
        ChangeState?.Invoke(GameState.Pause);
    }

    public void DisplayWeapons()
    {
        if (_weaponButtonParent.transform.childCount == 0)
        {
            foreach (var entry in _instantiatedWeaponModelsID.Values)
            {
                Destroy(entry);
            }
            _instantiatedWeaponModelsID.Clear();

            foreach (Weapon weapon in _listWeapons)
            {
                GameObject button = Instantiate(_buttonPrefab);

                button.GetComponentInChildren<TMP_Text>().text = weapon.WeaponName.ToString();
                button.GetComponent<Image>().sprite = weapon.WeaponImage;

                button.transform.SetParent(_weaponButtonParent.transform);

                button.transform.localScale = Vector3.one;
                button.transform.localRotation = Quaternion.identity;
                button.transform.localPosition = Vector3.zero;

                button.GetComponent<Button>().onClick.AddListener(() => _gameManager.SelectWeapon(_gameManager, weapon, button));
            }
        }
    }

    public void DisplayBullets()
    {
        if (_bulletButtonParent.transform.childCount == 0)
        {
            foreach (var entry in _instantiatedBulletModelsID.Values)
            {
                Destroy(entry);
            }
            _instantiatedBulletModelsID.Clear();

            foreach (Bullet bullet in _listBullets)
            {
                GameObject button = Instantiate(_buttonPrefabBullet);

                button.transform.Find("TxtNom").GetComponent<TMP_Text>().text = bullet.BulletName.ToString();

                if (bullet.BulletID == 0)
                    button.transform.Find("TxtQuantite").GetComponent<TMP_Text>().text = _gameManager.RevolverRessource.ToString();
                else
                    button.transform.Find("TxtQuantite").GetComponent<TMP_Text>().text = _gameManager.ShotGunRessource.ToString();

                button.GetComponent<Image>().sprite = bullet.BulletImage;

                button.transform.SetParent(_bulletButtonParent.transform);

                button.transform.localScale = Vector3.one;
                button.transform.localRotation = Quaternion.identity;
                button.transform.localPosition = Vector3.zero;

                button.GetComponent<Button>().onClick.AddListener(() => _gameManager.SelectBullet(_gameManager, bullet, button));
            }
        }
    }

    private IEnumerator SetupUISequence()
    {
        // Show the weapons panel first
        OnWeaponsClick();

        // Wait for a moment (you can adjust the delay time)
        yield return new WaitForSeconds(0.2f);

        // Show the bullets panel
        OnBulletsClick();

        // Wait for another moment before showing the weapons panel again
        yield return new WaitForSeconds(0.2f);

        // Show the weapons panel again, so it's ready when the player accesses it
        OnWeaponsClick();
    }

    public IEnumerator FadePanels(CanvasGroup fromPanel, CanvasGroup toPanel)
    {
        if (fromPanel != null)
        {
            yield return StartCoroutine(FadeOut(fromPanel));
        }

        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(FadeIn(toPanel));
    }

    public IEnumerator FadeOut(CanvasGroup panel)
    {
        _btnWeapons.interactable = false;
        _btnBullets.interactable = false;        
        panel.interactable = false;
        panel.blocksRaycasts = false;
        for (float t = 1; t >= 0; t -= Time.deltaTime)
        {
            panel.alpha = t;
            yield return null;
        }
        panel.alpha = 0;
    }

    public IEnumerator FadeIn(CanvasGroup panel)
    {
        panel.interactable = true;
        panel.blocksRaycasts = true;

        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            panel.alpha = t;
            yield return null;
        }
        panel.alpha = 1;

        _btnWeapons.interactable = true;
        _btnBullets.interactable = true;        
    }
    
}