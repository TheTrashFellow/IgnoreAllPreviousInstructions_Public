using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using Unity.VisualScripting;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using System.Collections.Generic;
using Button = UnityEngine.UI.Button;

public class UIManager : MonoBehaviour
{
    [Space]
    [Header("UI Menu et panels")]
    [SerializeField] private GameManagerMainMenu gameManagerMainMenu = default;
    [SerializeField] private GameObject _mainMenu = default;
    [SerializeField] private GameObject _testMenu = default;
    [SerializeField] private GameObject _btnTest = default;    
    [SerializeField] private CanvasGroup _optionsPanel = default;
    [SerializeField] private CanvasGroup _instructionsPanel = default;
    [SerializeField] private CanvasGroup _txtBienvenuePanel = default;
    [SerializeField] private List<Button> _buttons = new List<Button>();

    [Space]
    [Header("Texte des options")]
    [SerializeField] private TMP_Text _txtSnapTurn = default;
    [SerializeField] public TMP_Text _txtMusiqueFond = default;

    [Space]
    [Header("Liste des instructions")]
    [SerializeField] private List<GameObject> _listeTxtInstructions = default;

    private int i;
    private CanvasGroup _activePanel;
    private float _slideDistance = 500f;
    private Vector3 _originalPausePosition;
    [SerializeField] private Player player;

    public delegate void ChangeStateHandler(GameState state);
    public event ChangeStateHandler ChangeState;

    private void Start()
    {
        _activePanel = _txtBienvenuePanel;
        _instructionsPanel.alpha = 0;
        _optionsPanel.alpha = 0;
        _btnTest.SetActive(false);
        _testMenu.SetActive(false);
        
        player = FindObjectOfType<Player>();

        i = 0;

        bool initialState = player.IsSnapTurnEnabled();
        _txtSnapTurn.text = initialState ? "ON" : "OFF";

        bool isMusicOn = gameManagerMainMenu.IsAudioSourceOn();
        _txtMusiqueFond.text = isMusicOn ? "ON" : "OFF";
    }

    public void OnDemarrerClick()
    {        
        ChangeState?.Invoke(GameState.Play);
    }

    public void OnContinueClick()
    {
        //Continue le jeu
        ChangeState?.Invoke(GameState.Play);
    }

    public void OnOptionsClick()
    {
        if (_activePanel != _optionsPanel)
        {
            StopAllCoroutines();
            StartCoroutine(FadePanels(_activePanel, _optionsPanel));
            _activePanel = _optionsPanel;
            _btnTest.SetActive(false);
        }
    }   

    public void OnInstructionsClick()
    {
        if( _activePanel != _instructionsPanel)
        {
            StopAllCoroutines();
            StartCoroutine(FadePanels(_activePanel, _instructionsPanel));
            _activePanel = _instructionsPanel;
            _btnTest.SetActive(true);
        }
    }

    public void OnRetourClick()
    {
        StartCoroutine(FadePanels(_activePanel, _txtBienvenuePanel));
        _activePanel = _txtBienvenuePanel;
        _btnTest.SetActive(false);
    }

    public void OnTestClick()
    {
        Debug.Log("OnTestClick");
        ChangeState?.Invoke(GameState.Test);
    }

    public void OnRetourTestClick()
    {
        _testMenu.SetActive(false);
        _mainMenu.SetActive(true);
        StartCoroutine(FadePanels(_activePanel, _txtBienvenuePanel));
        _activePanel = _txtBienvenuePanel;
        _btnTest.SetActive(false);
    }

    public void OnSnapturnClick()
    {
        bool isSnapTurnOn = player.ChangerSnapTurn();

        _txtSnapTurn.text = isSnapTurnOn ? "OFF" : "ON";
    }

    public void OnChoixAudioClick()
    {
        bool isMusicOn = gameManagerMainMenu.IsAudioSourceOn();
        _txtMusiqueFond.text = isMusicOn ? "OFF" : "ON";

        gameManagerMainMenu.ToggleAudio();
    }

    public void ChangerInstructionsDroite()
    {
        Debug.Log("Suivant pressed " + i);
        if (i < _listeTxtInstructions.Count - 1)
        {
            _listeTxtInstructions[i].SetActive(false);
            i++;
            _listeTxtInstructions[i].SetActive(true);
        }
    }

    public void ChangerInstructionsGauche()
    {
        if (i > 0)
        {
            _listeTxtInstructions[i].SetActive(false);
            i--;
            _listeTxtInstructions[i].SetActive(true);
        }
    }

    private IEnumerator FadePanels(CanvasGroup fromPanel, CanvasGroup toPanel)
    {
        StartHorrorEffects(fromPanel, toPanel);

        yield return StartCoroutine(FadeOut(fromPanel));

        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(FadeIn(toPanel));
    }

    private void StartHorrorEffects(CanvasGroup fromPanel, CanvasGroup toPanel)
    {
        StartCoroutine(PanelFlicker(fromPanel, 1f));
        StartCoroutine(DistortPanel(toPanel, 1f));
    }

    private IEnumerator PanelFlicker(CanvasGroup panel, float duration)
    {
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            float randomAlpha = Random.Range(0.1f, 1f); 
            panel.alpha = randomAlpha;
            yield return new WaitForSeconds(0.05f); 
            panel.alpha = 0f; 
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator DistortPanel(CanvasGroup panel, float duration)
    {
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        Vector3 originalScale = rectTransform.localScale;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float distortionFactor = Mathf.Sin(elapsedTime * Mathf.PI * 2 / duration); // Sin wave for smooth distortion
            rectTransform.localScale = originalScale + new Vector3(distortionFactor * 0.1f, distortionFactor * 0.1f, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = originalScale; // Reset scale
    }

    private IEnumerator FadeOut(CanvasGroup panel)
    {
        foreach(Button button in _buttons)
        {
            button.interactable = false;
        }
        panel.interactable = false;
        panel.blocksRaycasts = false;
        for (float t = 1; t >= 0; t -= Time.deltaTime)
        {
            panel.alpha = t;
            yield return null;
        }
        panel.alpha = 0;
    }

    private IEnumerator FadeIn(CanvasGroup panel)
    {
        panel.interactable = true;
        panel.blocksRaycasts = true;

        for (float t = 0; t <= 1; t += Time.deltaTime)
        {
            panel.alpha = t;
            yield return null;
        }
        panel.alpha = 1;
        foreach (Button button in _buttons)
        {
            button.interactable = true;
        }
    }

    public void OnQuitterClick()
    {
        //  GameManagerMainMenu.Instance.UpdateGameState(GameState.Quit);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // Stop playmode in the editor
#else
        Application.Quit();  // Quit the application in the build
#endif
    }
}
