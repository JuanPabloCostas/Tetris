using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Game : MonoBehaviour
{
    /// <summary>
    /// The width of the grid...
    /// </summary>
    public static int gridWidth = 10;
    /// <summary>
    /// The height of the grid...
    /// </summary>
    public static int gridHeight = 20;

    /// <summary>
    /// The grid of the game
    /// </summary>
    public static Transform[,] grid = new Transform[gridWidth, gridHeight];

    public static bool startingAtLevelZero;
    public static int startingLevel;
    int startingHighscore; 
    int startingHighscore2; 
    int startingHighscore3; 


    // Scores
    public int scoreOneLine = 40;
    public int scoreTwoLine = 100;
    public int scoreThreeLine = 300;
    public int scoreFourLine = 1200;


    // Canvas
    public Canvas hud_canvas;
    public Canvas pause_canvas;

    /// <summary>
    /// Fall sapped, the lower, the faster
    /// </summary>
    public static float fallSpeed = 1.0f;
    public static bool isPaused = false;

    /// <summary>
    /// Number of rows that a tetromino just cleared
    /// </summary>
    private int numberOfRowsThisTurn = 0;

    public TMPro.TextMeshProUGUI hud_score;
    public TMPro.TextMeshProUGUI hud_level;
    public TMPro.TextMeshProUGUI hud_lines;

    public static int currentScore = 0;

    public AudioClip lineClearSound;
    private AudioSource audioSource;

    private GameObject previewTetromino;
    private GameObject nextTetromino;
    private GameObject savedTetromino;
    private GameObject ghostTetromino;

    public int currentLevel = 0;
    private int numLinesCleared = 0;

    private bool gameStarted = false;

    private Vector2 previewTetrominoPosition = new  Vector2(-6.5f, 16);
    private Vector2 savedTetrominoPosition = new Vector2(-6.5f, 10);

    public int maxSeaps = 2;
    private int currentSwaps = 0;

    // Start is called before the first frame update
    void Start()
    {
        currentLevel = startingLevel;
        currentScore = 0;
        startingHighscore = PlayerPrefs.GetInt("highscore");
        startingHighscore2 = PlayerPrefs.GetInt("highscore2");
        startingHighscore3 = PlayerPrefs.GetInt("highscore3");
        audioSource = GetComponent<AudioSource>();
        SpawnNextTetromino();
    }

    void Update()
    {
        UpdateScore();
        UpdateUI();
        UpdateLevel();
        UpdateSpeed();
        CheckUserInput();
        
    }

    void CheckUserInput()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            if (Time.timeScale > 0)
            {
                PauseGame();
            } else
            {
                ResumeGame();
            }
            
        }
        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            GameObject tempNextTetromino = GameObject.FindGameObjectWithTag("currentActiveTetromino");
            SaveTetromino(tempNextTetromino.transform);
        }
       
    }

    void PauseGame()
    {
        Time.timeScale = 0;
        audioSource.Pause();
        isPaused = true;
        hud_canvas.enabled = false;
        pause_canvas.enabled = true;
    }

    void ResumeGame()
    {
        Time.timeScale = 1;
        audioSource.Play(); 
        isPaused = false;
        hud_canvas.enabled = true;
        pause_canvas.enabled = false;
    }

    void UpdateLevel()
    {
        if (startingAtLevelZero || !startingAtLevelZero && numLinesCleared / 10 > startingLevel)
        {
            currentLevel = numLinesCleared / 10;
        }
        
    }

    void UpdateSpeed()
    {
        fallSpeed = 1.0f - ((float)currentLevel * 0.1f);
    }

    public void UpdateUI() 
    { 
        hud_score.text = currentScore.ToString();
        hud_level.text = currentLevel.ToString();
        hud_lines.text = numLinesCleared.ToString();
    }

    /// <summary>
    /// Keep track of the score
    /// </summary>
    public void UpdateScore ()
    {
        if (numberOfRowsThisTurn > 0) 
        {
            if (numberOfRowsThisTurn == 1)
            {
                ClearedOneLine();
            }
            else if (numberOfRowsThisTurn == 2)
            {
                ClearedTwoLine();
            }
            else if (numberOfRowsThisTurn == 3)
            {
                ClearedThreeLine();
            }
            else if (numberOfRowsThisTurn == 4)
            {
                ClearedFourLine();
            }
            numberOfRowsThisTurn = 0;

            PlayLineClearSound();
        }
    }

    /// <summary>
    /// Things for one line cleared
    /// </summary>
    public void ClearedOneLine()
    {
        currentScore += scoreOneLine + (currentLevel * 20);
        numLinesCleared++;
    }
    /// <summary>
    /// Things for two lines cleared
    /// </summary>
    public void ClearedTwoLine()
    {
        currentScore += scoreTwoLine + (currentLevel * 25);
        numLinesCleared+=2;
    }
    /// <summary>
    /// Things for three lines cleared
    /// </summary>
    public void ClearedThreeLine()
    {
        currentScore += scoreThreeLine + (currentLevel * 30);
        numLinesCleared+=3;
    }
    /// <summary>
    /// Things for four lines cleare
    /// </summary>
    public void ClearedFourLine()
    {
        currentScore += scoreFourLine + (currentLevel * 40);
        numLinesCleared+=4;
    }

    void PlayLineClearSound()
    {
        audioSource.PlayOneShot(lineClearSound);
    }

    bool CheckIsValidPosition (GameObject tetromino)
    {
        foreach (Transform mino in tetromino.transform)
        {
            Vector2 pos = Round(mino.position);

            if (!CheckIsInsideGrid(pos))
            {
                return false;
            }
            if (GetTransformAtGridPosition(pos) != null && GetTransformAtGridPosition(pos).parent != tetromino.transform)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// It check that the tetromino is on above the grid
    /// in each part of the tetromino
    /// </summary>
    /// <param name="tetromino"></param>
    /// <returns></returns>
    public bool CheckIsAboveGrid (Tetromino tetromino)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            foreach (Transform mino in tetromino.transform)
            {
                Vector2 pos = Round(mino.position);
                if (pos.y > gridHeight - 1)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the row is full at y
    /// </summary>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsFullRowAt (int y)
    {
        for (int x = 0; x < gridWidth; ++x)
        {
            if (grid[x, y] == null)
            {
                return false;
            }
        }

        //we just found a full row, we increment the variable
        numberOfRowsThisTurn++; 

        return true;
    }

    /// <summary>
    /// Deletes individual minos
    /// </summary>
    /// <param name="y"></param>
    public void DeleteMinoAt(int y)
    {
        for (int x = 0;x < gridWidth; x++)
        {
            Destroy (grid[x, y].gameObject);
            grid[x, y] = null; 
        }
    }

    /// <summary>
    /// It moves a row down per call
    /// </summary>
    /// <param name="y"></param>
    public void MoveRowDown(int y)
    {
        for (int x = 0; x < gridWidth; ++x)
        {
            if (grid[x, y] != null) 
            {
                grid[x, y - 1] = grid[x, y];
                grid[x, y] = null;
                grid[x, y - 1].position += new Vector3(0, -1, 0);
            }
        }
    }

    /// <summary>
    /// It calls MoveRowDown to move all rows down
    /// </summary>
    /// <param name="y"></param>
    public void MoveAllRowsDown (int y)
    {
        for (int i = y; i < gridHeight; ++i)
        {
            MoveRowDown(i);
        }
    }

    /// <summary>
    /// It checks every row for full rows, then
    /// it deletes all the minos on full rows 
    /// and move the rows down
    /// </summary>
    public void DeleteRow()
    {
        for(int y = 0; y < gridHeight; ++y)
        {
            if (IsFullRowAt(y))
            {
                DeleteMinoAt(y);

                MoveAllRowsDown(y + 1);
                --y;
            }
        }
    }

    /// <summary>
    /// It Updates the grid,
    /// called when the tetraminos moves
    /// </summary>
    /// <param name="tetromino"></param>
    public void UpdateGrid(Tetromino tetromino)
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    if (grid[x,y].parent == tetromino.transform)
                    {
                        grid[x,y] = null;
                    }
                }
            }
        }
        foreach (Transform mino in tetromino.transform)
        {
            Vector2 pos = Round(mino.position);
            if (pos.y < gridHeight)
            {
                grid[(int)pos.x,(int)pos.y] = mino;
            }
        }
    }

    /// <summary>
    /// It get the object transform of the grid position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Transform GetTransformAtGridPosition (Vector2 pos)
    {
        if (pos.y > gridHeight - 1)
        {
            return null;
        }
        else
        {
            return grid[(int)pos.x, (int)pos.y];
        }
    }

    /// <summary>
    /// Makes a new tetreamino appear
    /// </summary>
    public void SpawnNextTetromino()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            //- Spawn a tetromino (from the resources folder)
            nextTetromino = (GameObject)Instantiate(Resources.Load<GameObject>(GetRandomTetromino()), new Vector3(5.0f, 20.0f, 0.0f), Quaternion.identity);
            previewTetromino = (GameObject)Instantiate(Resources.Load<GameObject>(GetRandomTetromino()), previewTetrominoPosition, Quaternion.identity);
            previewTetromino.GetComponent<Tetromino>().enabled = false;
            nextTetromino.tag = "currentActiveTetromino";

            SpawnGhostTetromino();
        } 
        else
        {
            previewTetromino.transform.localPosition = new Vector2(5.0f, 20.0f);
            nextTetromino = previewTetromino;
            nextTetromino.GetComponent <Tetromino>().enabled = true;
            nextTetromino.tag = "currentActiveTetromino";
            previewTetromino = (GameObject)Instantiate(Resources.Load<GameObject>(GetRandomTetromino()), previewTetrominoPosition, Quaternion.identity);
            previewTetromino.GetComponent<Tetromino>().enabled = false;

            SpawnGhostTetromino();
        }

        currentSwaps = 0;

    }

    public void SpawnGhostTetromino()
    {
        if (GameObject.FindGameObjectWithTag("currentGhostTetromino") != null)
        {
            Destroy(GameObject.FindGameObjectWithTag("currentGhostTetromino"));
        }
        
        ghostTetromino = (GameObject)Instantiate (nextTetromino, nextTetromino.transform.position, Quaternion.identity);
        Destroy(ghostTetromino.GetComponent<Tetromino>());
        ghostTetromino.AddComponent<GhostTetromino>(); 
    }

    public void SaveTetromino (Transform t)
    {
        currentSwaps++;
        if (currentSwaps > maxSeaps)
        {
            return;
        }
        if (savedTetromino != null)
        {
            //- There is currently a tetromino being held
            GameObject tempSavedTetromino = GameObject.FindGameObjectWithTag("currentSavedTetromino");
            tempSavedTetromino.transform.localPosition = new Vector2(gridWidth / 2, gridHeight);

            if (!CheckIsValidPosition (tempSavedTetromino))
            {
                tempSavedTetromino.transform.localPosition = savedTetrominoPosition;
                return;
            }

            savedTetromino = (GameObject)Instantiate(t.gameObject);
            savedTetromino.GetComponent<Tetromino>().enabled = false;
            savedTetromino.transform.localPosition = savedTetrominoPosition;
            savedTetromino.tag = "currentSavedTetromino";

            nextTetromino = (GameObject)Instantiate(tempSavedTetromino);
            nextTetromino.GetComponent<Tetromino>().enabled = true;
            nextTetromino.transform.localPosition = new Vector2(gridWidth / 2, gridHeight);
            nextTetromino.tag = "currentActiveTetromino";

            DestroyImmediate (t.gameObject);
            DestroyImmediate (tempSavedTetromino);

            SpawnGhostTetromino();
        } 
        else
        {
            //- There is currently no tetromino being held
            savedTetromino = (GameObject)Instantiate(GameObject.FindGameObjectWithTag("currentActiveTetromino"));
            savedTetromino.GetComponent <Tetromino>().enabled = false;
            savedTetromino.transform.localPosition = savedTetrominoPosition;
            savedTetromino.tag = "currentSavedTetromino";

            DestroyImmediate(GameObject.FindGameObjectWithTag("currentActiveTetromino"));

            SpawnNextTetromino();
        }
    }

    /// <summary>
    /// return true or false depending if the position is inside the grid
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckIsInsideGrid (Vector2 pos)
    {
        return((int)pos.x >= 0 && (int)pos.x < gridWidth && pos.y >= 0);
    }

    /// <summary>
    /// Round the position
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Vector2 Round (Vector2 pos)
    {
        return new Vector2 (Mathf.Round(pos.x),Mathf.Round(pos.y));
    }

    /// <summary>
    /// Get the name of a random tetromino
    /// </summary>
    /// <returns></returns>
    string GetRandomTetromino()
    {
        int randomTetromino = Random.Range(1,8);
        string randomTetrominoName = "Tetromino_T";
        switch (randomTetromino)
        {
            case 1:
                randomTetrominoName = "Prefabs/Tetromino_T";
                break;
            case 2:
                randomTetrominoName = "Prefabs/Tetromino_Long";
                break;
            case 3:
                randomTetrominoName = "Prefabs/Tetromino_Square";
                break;
            case 4:
                randomTetrominoName = "Prefabs/Tetromino_J";
                break;
            case 5:
                randomTetrominoName = "Prefabs/Tetromino_L";
                break;
            case 6:
                randomTetrominoName = "Prefabs/Tetromino_S";
                break;
            case 7:
                randomTetrominoName = "Prefabs/Tetromino_Z";
                break;
        }
        return randomTetrominoName;
    }

    public void UpdateHighScore()
    {
        if (currentScore > startingHighscore) 
        {
            PlayerPrefs.SetInt("highscore3", startingHighscore2);
            PlayerPrefs.SetInt("highscore2", startingHighscore);
            PlayerPrefs.SetInt("highscore", currentScore);
        } 
        else if (currentScore > startingHighscore2) 
        {
            PlayerPrefs.SetInt("highscore3", startingHighscore2);
            PlayerPrefs.SetInt("highscore2", currentScore);
        }
        else if (currentScore > startingHighscore3) 
        {
            PlayerPrefs.SetInt("highscore3", currentScore);
        }
        PlayerPrefs.SetInt("lastHighScore", currentScore);
    }

    public void GameOver()
    {
        UpdateHighScore();
        SceneManager.LoadScene("GameOver");
    }

}
