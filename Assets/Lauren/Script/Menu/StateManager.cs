using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class StateManager : MonoBehaviour
{
    public static StateManager instance;

    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    public bool isPaused = false;
    public bool isGameOver = false;
    public GameState currentState;
    public GameState previousState;

    [Header("screens")]
    public GameObject pauseScreen;
    public GameObject gameoverScreen;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DisableScreens();
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1.0f;
        isPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitApplication()
    {
        Application.Quit();
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.Playing:
                CheckForPauseAndResume();
                break;
            case GameState.Paused:
                CheckForPauseAndResume();
                break;
            case GameState.GameOver:
                if (!isGameOver)
                {
                isGameOver = true;
                gameoverScreen.SetActive(true);
                ChangeState(GameState.GameOver);
                Time.timeScale = 0f;
                }
                break;
        }
    }  

    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            ChangeState(previousState);
            Time.timeScale = 1f;
            pauseScreen.SetActive(false);
        }
    }

    public void Pause()
    {
        if (currentState != GameState.Paused)
        {
            previousState = currentState;
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            pauseScreen.SetActive(true);
        }
    }


    void CheckForPauseAndResume()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
            else
            {
                Pause();
            }
        }
    }

    void DisableScreens()
    {
        pauseScreen.SetActive(false);
        gameoverScreen.SetActive(false);

    }

    public void GameOver()
    {
        gameoverScreen.SetActive(true);
        ChangeState(GameState.GameOver);
    }

}
