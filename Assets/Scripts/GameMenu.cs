using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{

    public TMPro.TextMeshProUGUI levelText;
    public TMPro.TextMeshProUGUI highScoreText;
    public TMPro.TextMeshProUGUI highScoreText2;
    public TMPro.TextMeshProUGUI highScoreText3;
    // Start is called before the first frame update
    void Start()
    {
        levelText.text = "0";

        highScoreText.text = PlayerPrefs.GetInt("highscore").ToString();
        highScoreText2.text = PlayerPrefs.GetInt("highscore2").ToString();
        highScoreText3.text = PlayerPrefs.GetInt("highscore3").ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        if (Game.startingLevel == 0)
        {
            Game.startingAtLevelZero = true;
        }
        else
        {
            Game.startingAtLevelZero = false;
        }
        SceneManager.LoadScene("Level");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangedValue (float value)
    {
        Game.startingLevel = (int)value;
        levelText.text = value.ToString();
    }
}
