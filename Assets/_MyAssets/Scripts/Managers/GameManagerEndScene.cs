using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using Random = UnityEngine.Random;


public class GameManagerEndScene : MonoBehaviour
{
    //public static GameManagerEndScene Instance;        

    //Player
    [SerializeField] private GameObject _player = default;

    //UniqueSceneFin
    [Space]
    [Header("Pour Scene de Fin")]
    [SerializeField] GameObject _baseTerrain = default;
    [SerializeField] GameObject _firstTerrain = default;    

    [Space]
    [Header("UI Final")]
    [SerializeField] private float _fadeDuration = 3.0f;
    [SerializeField] private TextMeshProUGUI _show1 = default;
    [SerializeField] private TextMeshProUGUI _show2 = default;
    [SerializeField] private TextMeshProUGUI _show3 = default;
    [SerializeField] private TextMeshProUGUI _show4 = default;
    [SerializeField] private TextMeshProUGUI _show5 = default;
    [SerializeField] private List<GameObject> _show6 = default;
    [SerializeField] private GameObject _buttonNext = default;

    [SerializeField] private GameObject _menu = default;

    [Space]
    [SerializeField] private TextMeshProUGUI _killCount = default;
    [SerializeField] private TextMeshProUGUI _completionTime = default;

    [Space]
    [Header("Audio")]
    [SerializeField] private AudioSource _globalBackground = default;
    [SerializeField] private AudioSource _globalMusic = default;


    private const string _audioKey = "Audio";
    private const string _snapKey = "Snap";
    //UI SHOW STEPS
    //show "Vous avez survecu"
    //show cette fois...
    //show Les dangers rodent...
    //show temps + ennemies
    //show buttons
    private int _showStep = 1;
    private int _showTemp = 0;

    private List<GameObject> _terrains = new List<GameObject>();

    private SceneManager sceneManager;
    
    private void Awake()
    {/*
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }*/
        if (PlayerPrefs.GetInt(_audioKey) == 1)
        {
            _globalMusic.volume = 1f;
        }
        else
        {
            _globalMusic.volume = 0f;
        }
    }

    private void Start()
    {
        _completionTime.text = "Temps : " + StaticVariables.gameTime;
        _killCount.text = "Ennemies : " + StaticVariables.gameKills;

        _menu.SetActive(true);
        _player.GetComponent<Player>().HidePhone();
        _player.GetComponent<Player>().MouvementJoueur(false);
        
        _terrains.Add(_firstTerrain);
    }    

    private Vector3 _move = new Vector3(0f, 0f, -0.2f);
    private Vector3 _spawnTerrain = new Vector3(0f, 0f, 900f);   
    private GameObject _toAdd = null;    

    private void Update()
    {
        ShowCurrentStepUI();
        MoveTerrain();
    }

    //Bouge les terrains instanciés pour donner l'illusion que le bateau avance. 
    private void MoveTerrain()
    {
        for (int i = 0; i < _terrains.Count; i++)
        {
            GameObject terrain = _terrains[i];
            terrain.transform.position += _move;
            if (i == _terrains.Count - 1 && terrain.transform.position.z <= -100 && _toAdd == null)
            {
                _toAdd = Instantiate(_baseTerrain, _spawnTerrain, _baseTerrain.transform.rotation);
                Debug.Log(_terrains.Count);
            }
        }
        if (_toAdd != null)
        {
            AddTerrain();
        }
    }

    //Gère l'ajout et le retret des terrains dans la scène.
    private void AddTerrain()
    {
        Debug.Log("TEST");
        Debug.Log(_toAdd);
        _terrains.Add(_toAdd);
        _toAdd = null;    
        if(_terrains.Count == 3)
        {
            Destroy(_terrains[0]);
            _terrains.RemoveAt(0);
            List<GameObject> temp = new List<GameObject> ();
            temp[0] = _terrains[1];
            temp[1] = _terrains[2];

            _terrains = temp;
        }
    }

    //Montre les éléments UI un à la suite des autres
    private void ShowCurrentStepUI()
    {
        switch (_showStep)
        {
            case 1:
                StartCoroutine(FadeIn(_show1));
                _showTemp = _showStep;
                _showStep = 0;
                break;
            case 2:
                StartCoroutine(FadeIn(_show2));
                _showTemp = _showStep;
                _showStep = 0;
                break;
            case 3:
                StartCoroutine(FadeIn(_show3));
                _showTemp = _showStep;
                _showStep = 0;
                break;
            case 4:
                StartCoroutine(FadeIn(_show4));
                _showTemp = _showStep;
                _showStep = 0;
                break;
            case 5:
                StartCoroutine(FadeIn(_show5));
                _showTemp = _showStep;
                _showStep = 0;
                break;
            case 6:
                StartCoroutine(FadeInButtons(_show6));
                _showTemp = _showStep;
                _showStep = 0;
                break;
        }
    }

    //Force l'apparition du prochain élément UI
    public void NextShowUI()
    {
        StopAllCoroutines();

        Debug.Log("Yippy");

        _showStep = _showTemp + 1;

        switch (_showStep)
        {
            case 2:
                Graphic thisShow = _show1;
                thisShow.color = new Color(255, 255, 255, 255);
                StartCoroutine(FadeIn(_show2));
                _showTemp = _showStep;
                _showStep= 0;
                break;
            case 3:
                Graphic thisShow2 = _show2;
                thisShow2.color = new Color(255, 255, 255, 255);
                StartCoroutine(FadeIn(_show3));
                _showTemp = _showStep;
                _showStep = 0;
                break;
            case 4:
                Graphic thisShow3 = _show3;
                thisShow3.color = new Color(255, 255, 255, 255);
                StartCoroutine(FadeIn(_show4));
                _showTemp = _showStep;
                _showStep = 0;
                break;
            case 5:
                Graphic thisShow4 = _show4;
                thisShow4.color = new Color(255, 255, 255, 255);
                StartCoroutine(FadeIn(_show5));
                _showTemp = _showStep;
                _showStep = 0;
                break;
            case 6:
                Graphic thisShow5 = _show5;
                thisShow5.color = new Color(255, 255, 255, 255);
                StartCoroutine(FadeInButtons(_show6));
                _showTemp = _showStep;
                _showStep = 0;
                break;
            case 7:
                ForceFadeInButtons(_show6);
                //thisShow6.color = new Color(255, 255, 255, 255);                
                _buttonNext.gameObject.SetActive(false);
                _showStep = 0;
                break;
        }
    }    

    public void Rejouer()
    {
        _menu.SetActive(false);
        StartCoroutine(StopSounds());
        _player.GetComponent<Player>().FadeToBlack();
        StartCoroutine(LoadScene());
    }

    public void RetourMenu()
    {
        SceneManager.LoadScene(0);
    }
    

    public void Quitter()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void ForceFadeInButtons(List<GameObject> buttons)
    {
        Color targetColorWhite = new Color(255, 255, 255, 255);
        Color targetColorBlack = new Color(0, 0, 0, 255);

        foreach (GameObject button in buttons)
        {            
            button.GetComponent<Image>().color = targetColorWhite;
            button.GetComponentInChildren<TextMeshProUGUI>().color = targetColorBlack;
            button.GetComponent<Button>().interactable = true;
        }

        
        
    }

    //Coroutine faire apparaitre éléments Graphics
    private IEnumerator FadeIn(Graphic uiElement)
    {
        Color originalColor = uiElement.color;
        Color targetColor = originalColor;
        targetColor.a = 1.0f;

        uiElement.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);

        float elapsedTime = 0.0f;

        while (elapsedTime < 3f)
        {
            uiElement.color = Color.Lerp(uiElement.color, targetColor, elapsedTime/_fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        uiElement.color = targetColor;
        _showStep = _showTemp + 1;
        _showTemp = 0;
    }

    //Coroutine faire apparaitre les 3 bouttons simultanément
    private IEnumerator FadeInButtons(List<GameObject> buttons)
    {
        List<Image> imageList = new List<Image>();
        List<TextMeshProUGUI> textList = new List<TextMeshProUGUI>();

        foreach(GameObject button in buttons)
        {            
            imageList.Add(button.GetComponent<Image>());
            textList.Add(button.GetComponentInChildren<TextMeshProUGUI>());
        }

        Color targetColorWhite = new Color(255, 255, 255, 255);
        Color targetColorBlack = new Color(0, 0, 0, 255);

        float elapsedTime = 0.0f;

        while (elapsedTime < 2f)
        {
            foreach (Image image in imageList)
            {
                image.color = Color.Lerp(image.color, targetColorWhite, elapsedTime / 200);
            }
            foreach (TextMeshProUGUI text in textList)
            {
                text.color = Color.Lerp(text.color, targetColorBlack, elapsedTime / 200);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach(GameObject button in buttons)
        {
            button.GetComponent<Button>().interactable = true;
        }

        _showStep = _showTemp + 1;
        _showTemp = 0;
    }

    private IEnumerator StopSounds()
    {
        float elapsedTime = 0.0f;
        float duration = 10f;

        //float musicVolume = _globalMusic.volume;

        float targetVolume = 0f;

        while (elapsedTime < 3)
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
    }

    private IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(4);

        SceneManager.LoadScene(1);
    }
}
