using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Image = UnityEngine.UI.Image;

public class Player : MonoBehaviour
{
    [Header("Pour Options")]    
    [SerializeField] private DynamicMoveProvider _dynamicMoveProvider = default;
    [SerializeField] private ActionBasedControllerManager _leftActionBasedControllerManager = default;
    [SerializeField] private ActionBasedControllerManager _rightActionBasedControllerManager = default;
    [SerializeField] private GameObject[] _phones = default;
    [SerializeField] private UIManagerInGame _UIManagerInGame = default;

    [Space]
    [Header("Gestion Vie du joueur")]
    [SerializeField] private GameObject _canvasVie = default;
    [SerializeField] private HealthBar _barreVie;
    [SerializeField] public int _vieMaximale = 3;
    [SerializeField] public int _vieActuelle;

    [Space]
    [Header("Gestion des ressources du joueur")]
    [SerializeField] private int _nombreBallesRevolver = 6;
    [SerializeField] private int _nombreBallesShotGun = 2;
    [SerializeField] private ColliderParticle _colliderParticle = default;
    [SerializeField] private GameObject _canvasFuel = default;
    public GameObject _spawnInventaire = default;

    [Space]
    [Header("Effet Glitch")]
    [SerializeField] private SpriteRenderer _glitchEffect = default;
    [SerializeField] private ColliderGlitch _colliderGlitch = default;
    [SerializeField] private AudioSource _audioSource = default;
    [SerializeField] private AudioClip _clipDamage = default;

    [Space]
    [Header("GameOverEffect")]
    [SerializeField] private GameObject _fadeBlack = default;
    [SerializeField] private GameObject _sphereGameOver = default;
    [SerializeField] private GameObject _cubeGameOver = default;
    [SerializeField] public AudioSource _deathMusic = default;    
    [SerializeField] private TunnelingVignetteController _tunnelingVignetteController = default;
    private bool _isDead = false;
    

    // ??? private XRGrabInteractable _xrGrabInteractable;
    private bool _isGlitchEffectActive = false;

    //IMPORTANT *** Dans le game manager, � partir de sa r�f�rence au player, venir d�finir comment agir quand GameOver est invok�.
    public delegate void GameOverHandler();
    public event GameOverHandler GameOver;

    private const string _snapTurnKey = "SnapTurn";
    private string SceneName;

    private void Awake()
    {
        SceneName = SceneManager.GetActiveScene().name;
        
        //_UIManagerInGame.ToggleTextSnapTurn();
        
        if (PlayerPrefs.HasKey(_snapTurnKey))
        {
            if (PlayerPrefs.GetInt(_snapTurnKey) == 1)
            {
                _rightActionBasedControllerManager.smoothTurnEnabled = false;
                                  
            }
            else
            {
                _rightActionBasedControllerManager.smoothTurnEnabled = true;                
            }
            
        }
        else
        {
            _rightActionBasedControllerManager.smoothTurnEnabled = false;            
            PlayerPrefs.SetInt(_snapTurnKey, 0);
            PlayerPrefs.Save();
        }
    }

    // Start is called before the first frame update
    void Start()
    {        
        if (SceneName == "SceneFer")
        {
            StartCoroutine(FadeIntoScene());            
            ShowHideFuelUI();
        }
        _vieActuelle = _vieMaximale;
        _barreVie.ChangerVieMax(_vieMaximale);
        StartCoroutine(RemoveDisplayCanvas(_canvasVie));

        MouvementJoueur(true);

        //if sc�ne de d�part
        //GererGameStateChange(GameManagerMainMenu.Instance.State);
        //GameManagerMainMenu.OnGameStateChanged += GererGameStateChange;
        //if scene de jeux

        //if scene de fin
    }

    public void ShowHideFuelUI() 
    {
        if (_canvasFuel.gameObject.activeSelf)
        {
            StartCoroutine(RemoveDisplayCanvas(_canvasFuel));
        }
        else
        {
            _canvasFuel.gameObject.SetActive(true);
            StartCoroutine(RemoveDisplayCanvas(_canvasFuel));
        }
        
    }

    void Update()
    {

        if(_colliderGlitch._ennemies.Count > 0 && !_isGlitchEffectActive && !_isDead)
        {
            StartCoroutine(GlitchEffect());
            _isGlitchEffectActive = true;
        }
    }    

    public void MouvementJoueur(bool canMove)
    {
        if(_dynamicMoveProvider != null)
        {
            if (canMove)
            {
                _dynamicMoveProvider.moveSpeed = 3;
                _dynamicMoveProvider.enabled = true;
                _dynamicMoveProvider.gameObject.SetActive(true);
            }
            else
            {                
                _dynamicMoveProvider.moveSpeed = 0;
                _dynamicMoveProvider.enabled = false;
                _dynamicMoveProvider.gameObject.SetActive(false);
            }
        }
        
    }

    public void BaisseDeVie()
    {
        _vieActuelle --;
        if(_vieActuelle == 0)
        {
            _barreVie.ChangerVie(_vieActuelle);
            StopAllCoroutines();
            OnGameOver();
        }
        else if(_vieActuelle > 0)
        {
            _barreVie.ChangerVie(_vieActuelle);
            _canvasVie.gameObject.SetActive(true);
            StopAllCoroutines();
            _isGlitchEffectActive = true;
            StartCoroutine(TookDamage());
            StartCoroutine(RemoveDisplayCanvas(_canvasVie));
        }        
    }

    private void OnGameOver()
    {
        _isDead = true;
        HidePhone();
        MouvementJoueur(false);
        if (_UIManagerInGame._isMenuOpen)
        {
            _UIManagerInGame.TogglePause();
        }
        GameOver?.Invoke();
        StartCoroutine(FadeToBlackDeath());              
    }

    public void FadeToBlack()
    {
        StartCoroutine(FadeToBlackLoad());
    }

    public void GainDeVie()
    {
        if (!_isDead)
        {
            _vieActuelle++;
            _barreVie.ChangerVie(_vieActuelle);
            _canvasVie.gameObject.SetActive(true);
            StartCoroutine(RemoveDisplayCanvas(_canvasVie));
        }
        
    }

    public bool ChangerSnapTurn()
    {
        _rightActionBasedControllerManager.smoothTurnEnabled = !_rightActionBasedControllerManager.smoothTurnEnabled;

        PlayerPrefs.SetInt(_snapTurnKey, _rightActionBasedControllerManager.smoothTurnEnabled ? 0 : 1);
        PlayerPrefs.Save();

        return _rightActionBasedControllerManager.smoothTurnEnabled;
    }

    public bool IsSnapTurnEnabled()
    {
        return !_rightActionBasedControllerManager.smoothTurnEnabled;
    }

    public void HidePhone()
    {
        foreach(GameObject phone in _phones)
        {
            phone.SetActive(false);
        }
    }

    public void WinRun()
    {
        StartCoroutine(FadeToBlacWin());
    }

    private IEnumerator FadeToBlackLoad()
    {
        float elapsedTime = 0.0f;
        float duration = 3f;

        _audioSource.PlayOneShot(_clipDamage);

        _audioSource.volume = 0f;
        float targetVolume = 1f;
        float currentVolume = 0f;

        Image fade = _fadeBlack.GetComponent<Image>();
        Color current = fade.color;
        Color target = new Color(0, 0, 0, 1);        

        while (elapsedTime < 3)
        {

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            float alpha = Mathf.Lerp(current.a, target.a, t);
            float volume = Mathf.Lerp(currentVolume, targetVolume, t);

            Color newColor = new Color(0, 0, 0, alpha);
            _audioSource.volume = volume;

            fade.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Color black = new Color(0, 0, 0, 1);
        fade.color = black;        
    }

    private IEnumerator FadeToBlacWin()
    {
        float elapsedTime = 0.0f;
        float duration = 2f;   

        Image fade = _fadeBlack.GetComponent<Image>();
        Color current = fade.color;
        Color target = new Color(0, 0, 0, 1);

        while (elapsedTime < duration)
        {

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            float alpha = Mathf.Lerp(current.a, target.a, t);            

            Color newColor = new Color(0, 0, 0, alpha);            

            fade.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Color black = new Color(0, 0, 0, 1);
        fade.color = black;
        SceneManager.LoadScene(2);
    }

    private IEnumerator FadeIntoScene()
    {
        float elapsedTime = 0.0f;
        float duration = 3f;

        _audioSource.PlayOneShot(_clipDamage);

        _audioSource.volume = 1f;
        float targetVolume = 0f;
        float currentVolume = 1f;

        Image fade = _fadeBlack.GetComponent<Image>();
        fade.color = new Color(0, 0, 0, 1);
        Color current = fade.color;
        Color target = new Color(0, 0, 0, 0);

        while (elapsedTime < 3)
        {

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            float alpha = Mathf.Lerp(current.a, target.a, t);
            float volume = Mathf.Lerp(currentVolume, targetVolume, t);

            Color newColor = new Color(0, 0, 0, alpha);
            _audioSource.volume = volume;

            fade.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _audioSource.volume = 0f;
        Color black = new Color(0, 0, 0, 0);
        fade.color = black;
    }

    private IEnumerator FadeToBlackDeath()
    {        
        float elapsedTime = 0.0f;
        float duration = 4f;

        _audioSource.PlayOneShot(_clipDamage);
        
        _audioSource.volume = 1f;

        Image fade = _fadeBlack.GetComponent<Image>();
        Color current = fade.color;
        Color target = new Color(0, 0, 0, 1);
        
        
        _glitchEffect.color = new Color(1,1,1,0.4f);

        while (elapsedTime < 4)
        {            

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            float alpha = Mathf.Lerp(current.a, target.a, t);

            Color newColor = new Color(0, 0, 0, alpha);
            fade.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Color black = new Color(0, 0, 0, 1);
        fade.color = black;

        _sphereGameOver.SetActive(true);
        _canvasVie.SetActive(false);
        _canvasFuel.SetActive(false);
        transform.position = new Vector3(1000, 1000, 1000);
        Instantiate(_cubeGameOver, transform.position - new Vector3(0, 7, 0), transform.rotation, transform.parent);        
        StartCoroutine(FadeBack());
        StartCoroutine(TookDamage());
    }    

    public void FadeMusic()
    {
        StartCoroutine(FadeDeathMusic());
    }

    public IEnumerator FadeDeathMusic()
    {
        float elapsedTime = 0.0f;
        float duration = 10f;

        float targetVolume = 0f;

        while (elapsedTime < 3)
        {

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);


            float musicVolume = Mathf.Lerp(_deathMusic.volume, targetVolume, t);
            

            _deathMusic.volume = musicVolume;
            


            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _deathMusic.volume = 0f;
    }

    private IEnumerator FadeBack()
    {
        if (!FindAnyObjectByType<GameManager>().IsAudioOn())
        {
            _deathMusic.volume = 0;
        }
        
        _tunnelingVignetteController.gameObject.SetActive(false);
        float elapsedTime = 0.0f;
        float duration = 4f;        

        Image fade = _fadeBlack.GetComponent<Image>();
        Color current = fade.color;
        Color target = new Color(0, 0, 0, 0);        

        while (elapsedTime < 4)
        {

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            float alpha = Mathf.Lerp(current.a, target.a, t);

            Color newColor = new Color(0, 0, 0, alpha);
            fade.color = newColor;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Color none = new Color(0, 0, 0, 0);
        fade.color = none;        
    }

    private IEnumerator RemoveDisplayCanvas(GameObject canvas)
    {
        yield return new WaitForSeconds(5);
        canvas.SetActive(false);
    }

    private IEnumerator TookDamage()
    {
        
        float elapsedTime = 0.0f;
        float duration = 10f;

        _audioSource.PlayOneShot(_clipDamage);

        _audioSource.volume = 1f;
        float targetVolume = 0f;

        Color startColor = new Color(1, 1, 1, 0.4f);
        _glitchEffect.color = startColor;
        Color target = new Color(1, 1, 1, 0);        

        while (elapsedTime < 2)
        {           

            float t = Mathf.SmoothStep(0f, 1f, elapsedTime/duration);

            _audioSource.volume = Mathf.Lerp(_audioSource.volume, targetVolume, t);
            _glitchEffect.color = Color.Lerp(_glitchEffect.color, target, t);  

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _isGlitchEffectActive = false;
        _audioSource.Stop();
        _glitchEffect.color = target;
    }

    private IEnumerator GlitchEffect()
    {
        List<GameObject> list = new List<GameObject>();
        gameObject.GetChildGameObjects(list);
        GameObject player = null;
        foreach (GameObject go in list)
        {
            if (go.tag == "Player")
            {
                player = go;
            }
        }

        while (_colliderGlitch._ennemies.Count > 0)
        {
            float maxAlpha = 0.03f;
            float maxDistance = 20f;

            float _closestDistance = 20;

            for (int i = 0; i < _colliderGlitch._ennemies.Count; i++)
            {
                if (_colliderGlitch._ennemies[i] != null)
                {
                    float distance = Vector3.Distance(_colliderGlitch._ennemies[i].transform.position, player.transform.position);

                    if (distance < _closestDistance)
                        _closestDistance = distance;
                }
                else
                {
                    _colliderGlitch._ennemies.RemoveAt(i);
                    if (_colliderGlitch._ennemies.Count == 0)
                    {
                        Color endColor = new Color(1, 1, 1, 0);
                        _glitchEffect.color = endColor;
                        _isGlitchEffectActive = false;
                        yield break;
                    }
                }
            }

            float alpha = Mathf.Clamp01(maxAlpha * (maxDistance - _closestDistance) / (maxDistance - 1));
            Color newColor = new Color(1, 1, 1, alpha);
            _glitchEffect.color = newColor;
            yield return null;
        }
        Color nullColor = new Color(1, 1, 1, 0);
        _glitchEffect.color = nullColor;
        _isGlitchEffectActive = false;
    }
}
