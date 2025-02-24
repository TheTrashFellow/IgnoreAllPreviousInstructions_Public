using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Start,
    Test,
    Play,
    Pause,
    Done,
    Quit
}

public class GameStateHandler : MonoBehaviour
{
    private GameManagerMainMenu gm_MainMenu;
    private UIManager uiManager;

    private GameManager gameManager;
    private UIManagerInGame uIManagerInGame;

    private GameManagerEndScene gm_EndScene;

    private string SceneName;

    private Player _player;

    public static event Action<GameState> OnGameStateChanged;
    private GameState _actualState;
    public GameState State => _actualState;

    void Start()
    {        
        SceneName = SceneManager.GetActiveScene().name;        
        _player = FindAnyObjectByType<Player>();

        switch (SceneName)
        {
            case "SceneStart":
                SetUpMainMenuScene();                
                break;
            case "SceneFer":
                SetUpGame();
                break;
            case "SceneFIN":
                SetUpEndScene();                
                break;
        }
    }

    private void SetUpMainMenuScene()
    {
        gm_MainMenu = FindObjectOfType<GameManagerMainMenu>();
        uiManager = FindObjectOfType<UIManager>();

        gm_MainMenu.ChangeState += UpdateGameState;
        uiManager.ChangeState += UpdateGameState;

        _player.HidePhone();        
    }

    private void SetUpGame()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        uIManagerInGame = FindAnyObjectByType<UIManagerInGame>();

        gameManager.ChangeState += UpdateGameState;
        uIManagerInGame.ChangeState += UpdateGameState;

        //gameManager.ChangeState += gameManager.HandleGameStateChange;
    }

    private void SetUpEndScene()
    {
        gm_EndScene = FindObjectOfType<GameManagerEndScene>();        

        _player.HidePhone();
        _player.MouvementJoueur(false);
    }

    public void UpdateGameState(GameState state)
    {
        if (state == _actualState) return;

        _actualState = state;

        if (_actualState == GameState.Quit)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        if(gm_MainMenu != null)
        {
            gm_MainMenu.HandleGameStateChange(_actualState);
        }
        
        if(gameManager != null)
        {            
            gameManager.HandleGameStateChange(_actualState);
        }
    }


}
