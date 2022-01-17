using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using TMPro;

public enum GameState { titleMenu, pauseMenu, guideMenu, creditsMenu, getReady, cityState, latentState }

public class GameManager : MonoBehaviour
{
    // Singleton Definition
    public static GameManager S;

    [Header("Game State")]
    public GameState gameState;
    public float readyStateDuration = 2.0f;

    [Header("UI Variables")]
    public string pauseKey = "p";

    public GameObject menuReturn;
    public GameObject menuPause;
    public GameObject menuWin;

    [Header("Player Variables")]
    public float latentPosShift = 2.0f;

    //private GameObject player;
    private GameObject currentBuilding;
    private int currentBuildingIndex = -1;

    [Header("Building Variables")]

    private BuildingBuilder bBComp;
    private List<GameObject> buildingSelection = new List<GameObject>();

    [Header("Text Information")]
    public TextAsset mapCoords;
    public TextAsset latentCoords;
    public TextAsset latentCodes;

    public List<Vector3> buildingMapCoords = new List<Vector3>();
    public List<Vector3> buildingLatentCoords = new List<Vector3>();
    public List<int> buildingLatentCodes = new List<int>();

    private void Awake()
    {
        //Singleton def
        if (GameManager.S)
        {
            Destroy(this.gameObject);
        }
        else
        {
            S = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        StartNewGame();

        // Load coord text assets
        buildingMapCoords = LoadCoords(mapCoords, true);
        buildingLatentCoords = LoadCoords(latentCoords);
        buildingLatentCodes = LoadCodes(latentCodes);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameState == GameState.cityState || gameState == GameState.latentState)
        {
            CheckPauseGame();
        }

        if (Input.GetKeyDown("c"))
        {
            ClearSelectedBuildings();
        }
    }

    // Game State Functions
    private void StartNewGame()
    {
        // state
        gameState = GameState.latentState;
        Time.timeScale = 1;

        // reset values

        // text

    }

    public void ResetRound()
    {
        // Menus

        // State checks

        // start the get ready coroutine
        StartCoroutine(GetReadyState());
    }


    public IEnumerator GetReadyState()
    {
        // Player Starting
        //player = LevelManager.S.currentPlayer;
        bBComp = LevelManager.S.currentBuilder.GetComponent<BuildingBuilder>();

        // Set State
        if (LevelManager.S.latentSpace)
        {
            gameState = GameState.latentState;
            //PlayerInLatentSpace();
            //Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            gameState = GameState.getReady;
        }

        // Reset round score

        // pause for 2 seconds
        yield return new WaitForSeconds(readyStateDuration);

        // start game
        gameState = GameState.cityState;
        //SoundManager.S.MakeRoundStartSound();
    }

    private void CheckPauseGame()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0;
            gameState = GameState.pauseMenu;
            menuPause.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        // Todo
        // Make sure to go back to correct state (check latent space)
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        menuPause.SetActive(false);

        if (LevelManager.S.latentSpace)
        {
            gameState = GameState.latentState;
        }
        else
        {
            gameState = GameState.cityState;
        }
    }

        // Information functions
        private List<Vector3> LoadCoords(TextAsset coordsFile, bool map=false)
    {
        List<Vector3> retList = new List<Vector3>();

        string allCoords = coordsFile.text;
        //Debug.Log(allCoords);

        string[] coords = allCoords.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        //Debug.Log(coords.Length);

        foreach (string line in coords)
        {
            string[] values = Regex.Split(line, ",");
            if (values.Length == 3)
            {
                float xAdjust = 0f;
                float yAdjust = 0f;
                float zAdjust = 0f;

                if (map)
                {
                    // X and Z adjustments to bring coordinates closer to origin
                    // Hard coded values from observing the coord files - could be done mathematically
                    // by subtracting smallest x and z value from all
                    xAdjust = -294000f;
                    zAdjust = -5035000f;
                }
                else
                {
                    yAdjust = 1f;
                }
                
                float x = float.Parse(values[0]) + xAdjust;
                float y = float.Parse(values[2]) + yAdjust;
                float z = float.Parse(values[1]) + zAdjust;

                retList.Add(new Vector3(x, y, z));
            }
        }
        return retList;
    }

    private List<int> LoadCodes(TextAsset codeFile)
    {
        List<int> retList = new List<int>();

        string allCodes = codeFile.text;
        //Debug.Log(allCoords);

        string[] coords = allCodes.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        //Debug.Log(coords.Length);

        foreach (string line in coords)
        {
            string[] values = Regex.Split(line, ",");
            if (values[0] != "")
            {
                int val = int.Parse(values[0]);
                retList.Add(val);
            }
        }
        return retList;
    }

     private float GetSelectedDistance()
     {
         int firstSel = buildingSelection[0].GetComponent<Building>().GetBuildingIndex();
         int secondSel = buildingSelection[1].GetComponent<Building>().GetBuildingIndex();

        float dist;
        if (LevelManager.S.latentSpace)
        {
            dist = Vector3.Distance(buildingLatentCoords[firstSel], buildingLatentCoords[secondSel]);
        }
        else
        {
            dist = Vector3.Distance(buildingMapCoords[firstSel], buildingMapCoords[secondSel]);
        }

        return dist;
     }

    // UI

    // Getter Setter Functions
    /// Current building
    public GameObject GetCurrBuilding()
    {
        return currentBuilding;
    }

    public int GetCurrBuildingIndex()
    {
        return currentBuildingIndex;
    }

    public void SetCurrBuilding(int i)
    {
        currentBuilding = bBComp.buildings[i];
        currentBuildingIndex = i;
    }

    /// Building Selection
    public void SelectBuilding(GameObject selectedBuilding)
    {
        // Currently maximum two selections
        if (buildingSelection.Count > 1)
        {
            buildingSelection[0].GetComponent<Outline>().eraseRenderer = true;
            buildingSelection.RemoveAt(0);
            buildingSelection.Add(selectedBuilding);
        }
        else
        {
            selectedBuilding.GetComponent<Outline>().eraseRenderer = false;
            buildingSelection.Add(selectedBuilding);
        }
    }

    private void ClearSelectedBuildings()
    {
        foreach (GameObject building in buildingSelection)
        {
            building.GetComponent<Outline>().eraseRenderer = true;
        }

        buildingSelection.Clear();
    }
}
