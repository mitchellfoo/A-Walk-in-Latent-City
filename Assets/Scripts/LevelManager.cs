using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager S;

    [Header("Level Info")]
    public string sceneName; // string to display at level start

    [Header("Game Objects")]
    public GameObject currentPlayer;
    public GameObject currentBuilder;


    [Header("Scene Info")]
    public string nextScene; // string of level name
    public bool titleScene;
    public bool latentSpace;

    private void Awake()
    {
        S = this;
    }

    private void Start()
    {
        if (GameManager.S)
        {
            GameManager.S.ResetRound();
        }
    }

    // Scene Management
    public void ChangeScene()
    {
        SceneManager.LoadScene(nextScene);
    }

    public void RestartLevel()
    {
        // reload this scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Destroy(GameManager.S.gameObject);
        SceneManager.LoadScene("TitleScene");
    }
}
